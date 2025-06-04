using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using MongoDB.Bson;

namespace Jogl.Server.Business
{
    public class WorkspaceService : BaseService, IWorkspaceService
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IChannelService _channelService;
        private readonly INotificationService _notificationService;

        public WorkspaceService(IWorkspaceRepository workspaceRepository, IChannelService channelService, INotificationService notificationService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _workspaceRepository = workspaceRepository;
            _channelService = channelService;
            _notificationService = notificationService;
        }

        public async Task<string> CreateAsync(Workspace community)
        {
            var feed = new Feed()
            {
                CreatedUTC = community.CreatedUTC,
                CreatedByUserId = community.CreatedByUserId,
                Type = FeedType.Workspace
            };

            var id = await _feedRepository.CreateAsync(feed);
            community.Id = ObjectId.Parse(id);
            community.FeedId = id;

            if (community.Onboarding == null)
                community.Onboarding = new OnboardingConfiguration
                {
                    Presentation = new OnboardingPresentation { Items = new List<OnboardingPresentationItem> { } },
                    Questionnaire = new OnboardingQuestionnaire { Items = new List<OnboardingQuestionnaireItem> { } },
                    Rules = new OnboardingRules { Text = string.Empty }
                };

            if (community.Settings == null)
                community.Settings = new List<string>();

            if (community.Tabs == null)
                community.Tabs = new List<string>();

            var communityId = await _workspaceRepository.CreateAsync(community);

            //create community membership record
            await _membershipRepository.CreateAsync(new Membership
            {
                UserId = community.CreatedByUserId,
                CreatedByUserId = community.CreatedByUserId,
                CreatedUTC = community.CreatedUTC,
                AccessLevel = AccessLevel.Owner,
                CommunityEntityId = communityId,
                CommunityEntityType = CommunityEntityType.Workspace,
            });

            //create user feed record
            await _userFeedRecordRepository.SetFeedReadAsync(community.CreatedByUserId, communityId, DateTime.UtcNow);

            //create channel
            community.HomeChannelId = await _channelService.CreateAsync(new Channel
            {
                AutoJoin = true,
                Visibility = ChannelVisibility.Open,
                Title = "General",
                IconKey = "table",
                Settings = new List<string> { CONTENT_MEMBER_POST, COMMENT_MEMBER_POST },
                CreatedByUserId = community.CreatedByUserId,
                CreatedUTC = community.CreatedUTC,
                CommunityEntityId = communityId
            });

            //update entity with home channel id
            await _workspaceRepository.UpdateAsync(community);


            return communityId;
        }

        public Workspace Get(string communityId, string userId)
        {
            var community = _workspaceRepository.Get(communityId);
            if (community == null)
                return null;

            EnrichWorkspaceData(new Workspace[] { community }, userId);
            return community;
        }

        public Workspace GetDetail(string communityId, string userId)
        {
            var community = _workspaceRepository.Get(communityId);
            if (community == null)
                return null;

            EnrichCommunityDataDetail(new Workspace[] { community }, userId);
            community.Path = _feedEntityService.GetPath(community, userId);

            return community;
        }

        public List<Workspace> Autocomplete(string userId, string search, int page, int pageSize)
        {
            var communities = _workspaceRepository.Autocomplete(search);
            return GetFilteredWorkspaces(communities, userId, null, page, pageSize);
        }

        public ListPage<Workspace> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var communities = _workspaceRepository.SearchSort(search, sortKey, ascending);
            var filteredWorkspaces = GetFilteredWorkspaces(communities, userId);
            var total = filteredWorkspaces.Count;

            var filteredCommunityPage = GetPage(filteredWorkspaces, page, pageSize);
            EnrichWorkspaceData(filteredCommunityPage, userId);

            return new ListPage<Workspace>(filteredWorkspaces, total);
        }

        public List<Workspace> ListForUser(string userId, string targetUserId, Permission? permission, string search, int page, int pageSize)
        {
            var communityMemberships = _membershipRepository.List(p => p.UserId == targetUserId && p.CommunityEntityType == CommunityEntityType.Workspace && !p.Deleted);
            var communityIds = communityMemberships
              .Select(m => m.CommunityEntityId)
              .ToList();

            var communities = _workspaceRepository.SearchGet(communityIds, search);

            var filteredWorkspaces = GetFilteredWorkspaces(communities, userId, permission != null ? new List<Permission> { permission.Value } : null, page, pageSize);
            EnrichWorkspaceData(filteredWorkspaces, userId);
            EnrichCommunityEntityDataWithContribution(filteredWorkspaces, communityMemberships, targetUserId);

            return filteredWorkspaces;
        }

        public List<Workspace> ListForWorkspace(string userId, string workspaceId, string search, int page, int pageSize)
        {
            var communityRelations = _relationRepository.List(r => r.TargetCommunityEntityId == workspaceId && r.SourceCommunityEntityType == CommunityEntityType.Workspace && !r.Deleted);
            var communityIds = communityRelations.Select(m => m.SourceCommunityEntityId).ToList();

            var communities = _workspaceRepository.Get(communityIds);
            var filteredWorkspaces = GetFilteredWorkspaces(communities, userId, null, page, pageSize);
            EnrichWorkspaceData(filteredWorkspaces, userId);

            return filteredWorkspaces;
        }

        public List<Workspace> ListForOrganization(string userId, string organizationId, string search, int page, int pageSize)
        {
            var communityIds = _relationRepository.List(r => r.TargetCommunityEntityId == organizationId && r.SourceCommunityEntityType == CommunityEntityType.Workspace && !r.Deleted)
             .Select(pn => pn.SourceCommunityEntityId)
             .ToList();

            var communities = _workspaceRepository.SearchGet(communityIds, search);
            var filteredWorkspaces = GetFilteredWorkspaces(communities, userId, null, page, pageSize);
            EnrichWorkspaceData(filteredWorkspaces, userId);

            return filteredWorkspaces;
        }

        public List<Workspace> ListForNode(string userId, string nodeId, string search, int page, int pageSize)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            var communities = _workspaceRepository.SearchGet(entityIds, search);

            var filteredWorkspaces = GetFilteredWorkspaces(communities, userId, null, page, pageSize);
            EnrichWorkspaceData(filteredWorkspaces, userId);

            return filteredWorkspaces;
        }

        public long CountForNode(string userId, string nodeId, string search)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            var communities = _workspaceRepository.SearchGet(entityIds, search);

            var filteredWorkspaces = GetFilteredWorkspaces(communities, userId, null);
            return filteredWorkspaces.Count;
        }

        public List<Workspace> ListForPaperExternalId(string userId, string externalId)
        {
            var papers = _paperRepository
                .Query(p => p.ExternalId == externalId)
                .ToList();

            var workspaces = _workspaceRepository.Get(papers.Select(p => p.FeedId).ToList());
            var filteredWorkspaces = GetFilteredWorkspaces(workspaces, userId, null);
            EnrichWorkspaceData(filteredWorkspaces, userId);

            return filteredWorkspaces;
        }

        public async Task UpdateAsync(Workspace community)
        {
            await _workspaceRepository.UpdateAsync(community);
        }

        public async Task DeleteAsync(string id)
        {
            await DeleteCommunityEntityAsync(id);
            await _workspaceRepository.DeleteAsync(id);
        }
    }
}