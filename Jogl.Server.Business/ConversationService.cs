using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using MongoDB.Bson;
using System.Drawing.Drawing2D;

namespace Jogl.Server.Business
{
    public class ConversationService : BaseService, IConversationService
    {
        private readonly IConversationRepository _conversationRepository;

        public ConversationService(IConversationRepository conversationRepository, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<string> CreateAsync(Conversation conversation)
        {
            //create feed
            var feed = new Feed()
            {
                CreatedUTC = conversation.CreatedUTC,
                CreatedByUserId = conversation.CreatedByUserId,
                Type = FeedType.Conversation,
            };

            var id = await _feedRepository.CreateAsync(feed);

            //mark feed write
            await _userFeedRecordRepository.SetFeedWrittenAsync(conversation.CreatedByUserId, id, DateTime.UtcNow);

            //create conversation
            conversation.Id = ObjectId.Parse(id);
            conversation.UpdatedUTC = conversation.CreatedUTC; //the purpose of this is to always have a value in the UpdatedUTC field, so that sorting by last update works
            await _conversationRepository.CreateAsync(conversation);

            //process notifications
            //await _notificationFacade.NotifyCreatedAsync(conversation);

            //return
            return id;
        }

        public Conversation Get(string conversationId, string currentUserId)
        {
            var conversation = _conversationRepository.Get(conversationId);
            if (conversation == null)
                return null;

            EnrichConversationData(new List<Conversation> { conversation }, currentUserId);
            EnrichConversationsWithPermissions(new List<Conversation> { conversation }, currentUserId);
            //conversation.Path = _feedEntityService.GetPath(conversation, currentUserId);

            return conversation;
        }


        public Conversation GetForUsers(IEnumerable<string> userIds, string currentUserId)
        {
            var conversation = _conversationRepository.Get(c => userIds.All(userId => c.UserVisibility.Any(uv => uv.UserId == userId)));
            if (conversation == null)
                return null;


            EnrichConversationData(new List<Conversation> { conversation }, currentUserId);
            //conversation.Path = _feedEntityService.GetPath(conversation, currentUserId);

            return conversation;
        }

        public ListPage<Conversation> List(string currentUserId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var conversations = _conversationRepository
                .Query(search)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .ToList();

            var total = conversations.Count;

            var conversationPage = GetPage(conversations, page, pageSize);
            EnrichConversationData(conversationPage, currentUserId);
            RecordListings(currentUserId, conversationPage);

            return new ListPage<Conversation>(conversationPage, total);
        }

        public long Count(string currentUserId, string search)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _conversationRepository
                .Query(search)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Count();
        }

        private void EnrichConversationData(IEnumerable<Conversation> conversations, string currentUserId)
        {
            var conversationIds = conversations.Select(c => c.Id.ToString()).ToList();
            var userIds = conversations.SelectMany(c => c.UserVisibility.Select(u => u.UserId)).Distinct().ToList();
            var users = _userRepository.Get(userIds);
            var contentEntities = _contentEntityRepository
                .Query(ce => conversationIds.Contains(ce.FeedId))
                .Sort(SortKey.CreatedDate, false)
                .GroupBy(ce => ce.FeedId, grp => grp.First())
                .ToList();

            EnrichConversationData(conversations, users, contentEntities, currentUserId);
        }

        private void EnrichConversationData(IEnumerable<Conversation> conversations, IEnumerable<User> users, IEnumerable<ContentEntity> contentEntities, string currentUserId)
        {
            foreach (var conversation in conversations)
            {
                var otherUserId = conversation.UserVisibility?.FirstOrDefault(u => u.UserId != currentUserId)?.UserId;
                if (otherUserId != null)
                    conversation.User = users.SingleOrDefault(u => u.Id.ToString() == otherUserId);

                conversation.LatestMessage = contentEntities.FirstOrDefault(ce => ce.FeedId == conversation.Id.ToString());
            }

            EnrichFeedEntitiesWithVisibilityData(conversations);
            EnrichConversationsWithPermissions(conversations, currentUserId);
            EnrichEntitiesWithCreatorData(conversations);
        }


        public async Task UpdateAsync(Conversation conversation)
        {
            await _conversationRepository.UpdateAsync(conversation);
            //      await _notificationFacade.NotifyUpdatedAsync(conversation);
        }

        public async Task DeleteAsync(Conversation conversation)
        {
            await DeleteFeedAsync(conversation.Id.ToString());
            await _conversationRepository.DeleteAsync(conversation);
        }
    }
}