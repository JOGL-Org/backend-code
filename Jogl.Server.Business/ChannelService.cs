﻿using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using MongoDB.Bson;

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

            await _membershipService.CreateAsync(new Membership
            {
                UserId = channel.CreatedByUserId,
                CreatedByUserId = channel.CreatedByUserId,
                CreatedUTC = channel.CreatedUTC,
                AccessLevel = AccessLevel.Admin,
                CommunityEntityId = channelId,
                CommunityEntityType = CommunityEntityType.Channel,
            });

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

        public bool ListForNodeHasNewContent(string currentUserId, string nodeId)
        {
            var entityIds = GetFeedEntityIdsForNode(nodeId);

            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var channelIds = _channelRepository.Query(c => entityIds.Contains(c.CommunityEntityId))
                .Filter(c => currentUserMemberships.Select(m => m.CommunityEntityId).Contains(c.Id.ToString()))
                .ToList(c => c.Id.ToString());

            var unreadUFRs = _userFeedRecordRepository
                  .Query(ufr => ufr.UserId == currentUserId)
                  .Filter(ufr => channelIds.Contains(ufr.FeedId))
                  .Filter(ufr => ufr.FollowedUTC.HasValue)
                  .Filter(ufr => ufr.Unread)
                  .ToList();

            var unreadUCERs = _userContentEntityRecordRepository
                 .Query(ucer => ucer.UserId == currentUserId)
                 .Filter(ucer => channelIds.Contains(ucer.FeedId))
                 .Filter(ucer => ucer.FollowedUTC.HasValue)
                 .Filter(ucer => ucer.Unread)
                 .ToList();

            return unreadUFRs.Any() || unreadUCERs.Any();
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
            await DeleteFeedAsync(id);
            await _channelRepository.DeleteAsync(id);
            await _membershipRepository.DeleteAsync(m => m.CommunityEntityId == id);
        }
    }
}