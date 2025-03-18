using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using Jogl.Server.Notifications;
using MongoDB.Bson;

namespace Jogl.Server.Business
{
    public class NeedService : BaseService, INeedService
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly INodeRepository _nodeRepository;
        private readonly INotificationService _notificationService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly INotificationFacade _notificationFacade;

        public NeedService(IWorkspaceRepository workspaceRepository, INodeRepository nodeRepository, INotificationService notificationService, ICommunityEntityService communityEntityService, INotificationFacade notificationFacade, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _workspaceRepository = workspaceRepository;
            _nodeRepository = nodeRepository;
            _notificationService = notificationService;
            _communityEntityService = communityEntityService;
            _notificationFacade = notificationFacade;
        }

        public async Task<string> CreateAsync(Need need)
        {
            //create feed
            var feed = new Feed()
            {
                CreatedUTC = need.CreatedUTC,
                CreatedByUserId = need.CreatedByUserId,
                Type = FeedType.Need,
            };

            var id = await _feedRepository.CreateAsync(feed);

            //mark feed write
            await _userFeedRecordRepository.SetFeedWrittenAsync(need.CreatedByUserId, id, DateTime.UtcNow);

            //create need
            need.Id = ObjectId.Parse(id);
            need.UpdatedUTC = need.CreatedUTC; //the purpose of this is to always have a value in the UpdatedUTC field, so that sorting by last update works
            await _needRepository.CreateAsync(need);

            //process notifications
            await _notificationFacade.NotifyCreatedAsync(need);

            //return
            return id;
        }

        public Need Get(string needId, string userId)
        {
            var need = _needRepository.Get(needId);
            if (need == null)
                return null;

            EnrichNeedData(new List<Need> { need }, userId);
            need.Path = _feedEntityService.GetPath(need, userId);

            return need;
        }

        public ListPage<Need> List(string currentUserId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var needs = _needRepository
                .Query(search)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .WithFeedRecordData()
                .Sort(sortKey, ascending)
                .ToList();


            var total = needs.Count;

            var needPage = GetPage(needs, page, pageSize);
            EnrichNeedData(needPage, currentUserId);
            RecordListings(currentUserId, needPage);

            return new ListPage<Need>(needPage, total);
        }

        public long Count(string currentUserId, string search)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _needRepository
                .Query(search)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Count();
        }

        public List<Need> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var needs = _needRepository
                .Query(search)
                .Filter(n => n.EntityId == entityId)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Sort(sortKey, ascending)
                .Page(page, pageSize)
                .ToList();

            EnrichNeedData(needs, currentUserId);
            RecordListings(currentUserId, needs);

            return needs;
        }

        public bool ListForEntityHasNew(string currentUserId, string entityId)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _needRepository
                   .Query(n => n.EntityId == entityId)
                   .WithFeedRecordData()
                   .FilterFeedEntities(currentUserId, currentUserMemberships)
                   .Filter(p => p.LastOpenedUTC == null)
                   .Any();
        }

        public ListPage<Need> ListForNode(string currentUserId, string nodeId, List<string> communityEntityIds, FeedEntityFilter? filter, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var communityEntities = _communityEntityService.List(entityIds);
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var needs = _needRepository
                .Query(search)
                .Filter(n => entityIds.Contains(n.EntityId))
                .WithFeedRecordData()
                .FilterFeedEntities(currentUserId, currentUserMemberships, filter)
                .Sort(sortKey, ascending)
                .ToList();

            var needPage = GetPage(needs, page, pageSize);
            EnrichNeedData(needPage, communityEntities, currentUserId);
            RecordListings(currentUserId, needPage);

            return new ListPage<Need>(needPage, needs.Count);
        }

        public bool ListForNodeHasNew(string currentUserId, string nodeId, FeedEntityFilter? filter)
        {
            var entityIds = GetFeedEntityIdsForNode(nodeId);

            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _needRepository
                   .Query(n => entityIds.Contains(n.EntityId))
                   .WithFeedRecordData()
                   .FilterFeedEntities(currentUserId, currentUserMemberships, filter)
                   .Filter(n => n.LastOpenedUTC == null)
                   .Any();
        }

        public long CountForNode(string currentUserId, string nodeId, string search)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);

            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _needRepository
                .Query(search)
                .Filter(n => entityIds.Contains(n.EntityId))
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Count();
        }

        private void EnrichNeedData(IEnumerable<Need> needs, string currentUserId)
        {
            var communityEntities = _communityEntityService.List(needs.Select(e => e.CommunityEntityId).Distinct());
            EnrichNeedData(needs, communityEntities, currentUserId);
        }

        private void EnrichNeedData(IEnumerable<Need> needs, IEnumerable<CommunityEntity> communityEntities, string currentUserId)
        {
            var memberships = _membershipRepository.List(m => !m.Deleted && m.UserId == currentUserId);
            var contentEntities = _contentEntityRepository.List(ce => needs.Any(n => n.Id.ToString() == ce.FeedId) && !ce.Deleted);
            var needIds = needs.Select(e => e.Id.ToString());
            var userFeedRecords = _userFeedRecordRepository.List(ufr => ufr.UserId == currentUserId && needIds.Contains(ufr.FeedId));
            var userContentEntityRecords = _userContentEntityRecordRepository.List(ucer => ucer.UserId == currentUserId && needIds.Contains(ucer.FeedId) && !ucer.Deleted);
            var mentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.Unread && needIds.Contains(m.OriginFeedId) && !m.Deleted);

            foreach (var need in needs)
            {
                var feedRecord = userFeedRecords.SingleOrDefault(ufr => ufr.FeedId == need.Id.ToString());

                need.CommunityEntity = communityEntities.SingleOrDefault(ce => ce.Id.ToString() == need.EntityId);
                need.PostCount = contentEntities.Count(ce => ce.FeedId == need.Id.ToString());
                need.NewPostCount = contentEntities.Count(ce => ce.FeedId == need.Id.ToString() && ce.CreatedUTC > (feedRecord?.LastReadUTC ?? DateTime.MaxValue));
                need.NewMentionCount = mentions.Count(m => m.OriginFeedId == need.Id.ToString());
                need.NewThreadActivityCount = contentEntities.Count(ce => ce.FeedId == need.Id.ToString() && ce.LastActivityUTC > (userContentEntityRecords.SingleOrDefault(ucer => ucer.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue));

                if (need.CommunityEntity != null)
                {
                    var membership = memberships.SingleOrDefault(m => need.EntityId == m.CommunityEntityId);
                    need.CommunityEntity.AccessLevel = membership?.AccessLevel;
                }
            }

            EnrichFeedEntitiesWithVisibilityData(needs);
            EnrichNeedsWithPermissions(needs, currentUserId);
            EnrichEntitiesWithCreatorData(needs);
        }

        public List<Need> ListForUser(string currentUserId, string targetUserId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var needs = _needRepository.Query(search)
                .Filter(n => n.CreatedByUserId == targetUserId)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Sort(sortKey, ascending)
                .Page(page, pageSize)
                .ToList();

            EnrichNeedData(needs, currentUserId);
            RecordListings(currentUserId, needs);

            return needs;
        }

        public async Task UpdateAsync(Need need)
        {
            await _needRepository.UpdateAsync(need);
            await _notificationFacade.NotifyUpdatedAsync(need);
        }

        public async Task DeleteAsync(string id)
        {
            await DeleteFeedAsync(id);
            await _needRepository.DeleteAsync(id);
        }

        public List<CommunityEntity> ListCommunityEntitiesForNodeNeeds(string currentUserId, string nodeId, string search, int page, int pageSize)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var needs = _needRepository
              .Query(search)
              .Filter(p => entityIds.Contains(p.EntityId))
              .FilterFeedEntities(currentUserId, currentUserMemberships)
              .ToList();

            EnrichNeedData(needs, currentUserId);
            return GetPage(needs.Select(e => e.CommunityEntity).Where(e => e != null).DistinctBy(e => e.Id), page, pageSize);
        }
    }
}