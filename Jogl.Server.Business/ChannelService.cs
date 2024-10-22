using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace Jogl.Server.Business
{
    public class ChannelService : BaseService, IChannelService
    {
        private readonly IMembershipService _membershipService;

        public ChannelService(IMembershipService membershipService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _membershipService = membershipService;
        }

        public async Task<string> CreateAsync(Channel channel)
        {
            var feed = new Feed()
            {
                CreatedUTC = channel.CreatedUTC,
                CreatedByUserId = channel.CreatedByUserId,
                Type = FeedType.Channel
            };

            var id = await _feedRepository.CreateAsync(feed);
            channel.Id = ObjectId.Parse(id);

            if (channel.Settings == null)
                channel.Settings = new List<string>();

            var channelId = await _channelRepository.CreateAsync(channel);

            //create community membership record
            var membership = new Membership
            {
                UserId = channel.CreatedByUserId,
                CreatedByUserId = channel.CreatedByUserId,
                CreatedUTC = channel.CreatedUTC,
                AccessLevel = AccessLevel.Admin,
                CommunityEntityId = channelId,
                CommunityEntityType = CommunityEntityType.Channel,
            };

            await _membershipRepository.CreateAsync(membership);

            //create user feed record
            await _userFeedRecordRepository.SetFeedReadAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);

            //auto-add all members if autojoin toggled on
            if (channel.AutoJoin)
            {
                var allMembers = _membershipService.ListMembers(channel.CommunityEntityId);
                await _membershipService.AddMembersAsync(allMembers.Select(m => new Membership
                {
                    AccessLevel = channel.Members?.SingleOrDefault(mem => mem.UserId == m.UserId)?.AccessLevel ?? AccessLevel.Member,
                    CommunityEntityId = channel.Id.ToString(),
                    CommunityEntityType = CommunityEntityType.Channel,
                    CreatedUTC = channel.UpdatedUTC ?? channel.CreatedUTC,
                    CreatedByUserId = channel.UpdatedByUserId ?? channel.CreatedByUserId,
                    UserId = m.UserId,
                }).ToList());
            }
            else
            {
                await _membershipService.AddMembersAsync(channel.Members.Select(m => new Membership
                {
                    AccessLevel = m.AccessLevel,
                    CommunityEntityId = channel.Id.ToString(),
                    CommunityEntityType = CommunityEntityType.Channel,
                    CreatedUTC = channel.UpdatedUTC ?? channel.CreatedUTC,
                    CreatedByUserId = channel.UpdatedByUserId ?? channel.CreatedByUserId,
                    UserId = m.UserId,
                }).ToList());
            }

            return channelId;
        }

        public Channel Get(string channelId, string userId)
        {
            var channel = _channelRepository.Get(channelId);
            if (channel == null)
                return null;

            EnrichChannelData(new Channel[] { channel }, userId);

            return channel;
        }

        public Channel GetDetail(string channelId, string userId)
        {
            var channel = Get(channelId, userId);
            if (channel == null)
                return null;

            channel.Path = _feedEntityService.GetPath(channel, userId);
            var stats = GetDiscussionStats(userId, channelId);
            channel.UnreadPosts = stats.UnreadPosts;
            channel.UnreadThreads = stats.UnreadThreads;
            channel.UnreadMentions = stats.UnreadMentions;

            return channel;
        }

        public List<Channel> ListForEntity(string userId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var channels = _channelRepository.AutocompleteList(c => c.CommunityEntityId == entityId && !c.Deleted, search, sortKey, sortAscending);
            var channelIds = channels.Select(c => c.Id.ToString()).ToList();
            var channelMemberships = _membershipRepository.List(m => channelIds.Contains(m.CommunityEntityId) && !m.Deleted);
            var currentUserMemberships = _membershipRepository.List(m => m.UserId == userId && !m.Deleted);
            var filteredChannels = GetFilteredChannels(channels, currentUserMemberships, page, pageSize);
            EnrichChannelData(filteredChannels, channelMemberships, currentUserMemberships);

            return filteredChannels;
        }

        public async Task UpdateAsync(Channel channel)
        {
            var existingChannel = _channelRepository.Get(channel.Id.ToString());
            await _channelRepository.UpdateAsync(channel);

            //auto-add all members if autojoin toggled on
            if (!existingChannel.AutoJoin && channel.AutoJoin)
            {
                var allMembers = _membershipService.ListMembers(existingChannel.CommunityEntityId);
                await _membershipService.AddMembersAsync(allMembers.Select(m => new Membership
                {
                    AccessLevel = channel.Members?.SingleOrDefault(mem => mem.UserId == m.UserId)?.AccessLevel ?? AccessLevel.Member,
                    CommunityEntityId = channel.Id.ToString(),
                    CommunityEntityType = CommunityEntityType.Channel,
                    CreatedUTC = channel.UpdatedUTC ?? channel.CreatedUTC,
                    CreatedByUserId = channel.UpdatedByUserId ?? channel.CreatedByUserId,
                    UserId = m.UserId,
                }).ToList());
            }
        }

        public async Task DeleteAsync(string id)
        {
            var community = _channelRepository.Get(id);

            await _membershipRepository.DeleteAsync(m => m.CommunityEntityId == id);
            await _userFeedRecordRepository.DeleteAsync(ufr => ufr.FeedId == id);
            await _userContentEntityRecordRepository.DeleteAsync(ufr => ufr.FeedId == id);
            await _documentRepository.DeleteAsync(d => d.FeedId == id);
            await _contentEntityRepository.DeleteAsync(ce => ce.FeedId == id);
            await _commentRepository.DeleteAsync(c => c.FeedId == id);
            await _reactionRepository.DeleteAsync(r => r.FeedId == id);

            await _feedRepository.DeleteAsync(id);
            await _channelRepository.DeleteAsync(id);
        }
    }
}