using Azure;
using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;

namespace Jogl.Server.Business
{
    public class ResourceService : BaseService, IResourceService
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly INotificationService _notificationService;

        public ResourceService(IWorkspaceRepository workspaceRepository, INotificationService notificationService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _workspaceRepository = workspaceRepository;
            _notificationService = notificationService;
        }

        public Resource Get(string resourceId)
        {
            return _resourceRepository.Get(resourceId);
        }

        public async Task<string> CreateAsync(Resource resource)
        {
            //create entity
            var id = await _resourceRepository.CreateAsync(resource);

            //process notifications
            await _notificationService.NotifyResourceCreatedAsync(resource);

            //return
            return id;
        }

        public List<Resource> ListForFeed(string feedId, string search, int page, int pageSize)
        {
            return _resourceRepository.List((p) =>
              (string.IsNullOrEmpty(search) || p.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase))
              && (p.FeedId == feedId)
              && (!p.Deleted),
              page, pageSize);
        }

        public List<Resource> ListForNode(string userId, string nodeId, string search, int page, int pageSize)
        {
            var communityIds = _relationRepository.List(r => r.TargetCommunityEntityId == nodeId && r.SourceCommunityEntityType == CommunityEntityType.Workspace && !r.Deleted)
                .Select(pn => pn.SourceCommunityEntityId)
                .ToList();

            var communities = _workspaceRepository.Get(communityIds);
            var filteredCommunities = GetFilteredWorkspaces(communities, userId);//TODO pass collections from above to optimize

            return _resourceRepository.ListForFeeds(filteredCommunities.Select(p => p.FeedId).Union(new List<string> { nodeId }))
                .Where(n => string.IsNullOrEmpty(search) || n.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) || (n.Description != null && n.Description.Contains(search, StringComparison.CurrentCultureIgnoreCase)) && (!n.Deleted))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int CountForNode(string currentUserId, string nodeId, PaperType? type, string search)
        {
            var communityIds = _relationRepository.List(r => r.TargetCommunityEntityId == nodeId && r.SourceCommunityEntityType == CommunityEntityType.Workspace && !r.Deleted)
              .Select(pn => pn.SourceCommunityEntityId)
              .ToList();

            var communities = _workspaceRepository.Get(communityIds);
            var filteredCommunities = GetFilteredWorkspaces(communities, currentUserId);//TODO pass collections from above to optimize

            return _resourceRepository.ListForFeeds(filteredCommunities.Select(p => p.FeedId).Union(new List<string> { nodeId }))
                .Count(n => string.IsNullOrEmpty(search) || n.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) || (n.Description != null && n.Description.Contains(search, StringComparison.CurrentCultureIgnoreCase)) && (!n.Deleted));
        }

        public async Task UpdateAsync(Resource resource)
        {
            await _resourceRepository.UpdateAsync(resource);
        }

        public async Task DeleteAsync(string id)
        {
            await _resourceRepository.DeleteAsync(id);
        }
    }
}