//using Jogl.Server.Data;
//using Jogl.Server.Data.Enum;
//using Jogl.Server.Data.Util;
//using Jogl.Server.DB;
//using MongoDB.Bson;
//using MongoDB.Driver.Linq;

//namespace Jogl.Server.Business
//{
//    public class ProjectService : BaseService, IProjectService
//    {
//        private readonly IProjectRepository _projectRepository;
//        private readonly IChannelService _channelService;
//        private readonly INotificationService _notificationService;

//        public ProjectService(IProjectRepository projectRepository, IChannelService channelService, INotificationService notificationService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
//        {
//            _projectRepository = projectRepository;
//            _channelService = channelService;
//            _notificationService = notificationService;
//        }

//        public async Task<string> CreateAsync(Project project)
//        {
//            var feed = new Feed()
//            {
//                CreatedUTC = project.CreatedUTC,
//                CreatedByUserId = project.CreatedByUserId,
//                Type = FeedType.Project,
//            };

//            var id = await _feedRepository.CreateAsync(feed);
//            project.Id = ObjectId.Parse(id);
//            project.FeedId = id;

//            if (project.Onboarding == null)
//                project.Onboarding = new OnboardingConfiguration
//                {
//                    Presentation = new OnboardingPresentation { Items = new List<OnboardingPresentationItem> { } },
//                    Questionnaire = new OnboardingQuestionnaire { Items = new List<OnboardingQuestionnaireItem> { } },
//                    Rules = new OnboardingRules { Text = string.Empty }
//                };

//            if (project.Settings == null)
//                project.Settings = new List<string>();

//            if (project.Tabs == null)
//                project.Tabs = new List<string>();

//            var projectId = await _projectRepository.CreateAsync(project);

//            //create project membership record
//            var membership = new Membership
//            {
//                UserId = project.CreatedByUserId,
//                CreatedByUserId = project.CreatedByUserId,
//                CreatedUTC = project.CreatedUTC,
//                AccessLevel = AccessLevel.Owner,
//                CommunityEntityId = projectId,
//                CommunityEntityType = CommunityEntityType.Project,
//            };

//            await _membershipRepository.CreateAsync(membership);

//            //create user feed record
//            await _userFeedRecordRepository.SetFeedReadAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);

//            //create channel
//            project.HomeChannelId = await _channelService.CreateAsync(new Channel
//            {
//                AutoJoin = true,
//                Visibility = ChannelVisibility.Open,
//                Title = "General",
//                IconKey = "table",
//                Settings = new List<string> { CONTENT_MEMBER_POST, COMMENT_MEMBER_POST },
//                CreatedByUserId = project.CreatedByUserId,
//                CreatedUTC = project.CreatedUTC,
//                CommunityEntityId = projectId
//            });

//            //update entity with home channel id
//            await _projectRepository.UpdateAsync(project);

//            return projectId;
//        }

//        public Project Get(string projectId, string userId)
//        {
//            var project = _projectRepository.Get(projectId);
//            if (project == null)
//                return null;

//            EnrichProjectData(new Project[] { project }, userId);
//            return project;
//        }

//        public Project GetDetail(string projectId, string userId)
//        {
//            var project = _projectRepository.Get(projectId);
//            if (project == null)
//                return null;

//            EnrichProjectDataDetail(new Project[] { project }, userId);
//            project.Path = _feedEntityService.GetPath(project, userId);

//            return project;
//        }

//        public List<Project> Autocomplete(string userId, string search, int page, int pageSize)
//        {
//            var projects = _projectRepository.Autocomplete(search);
//            return GetFilteredProjects(projects, userId, null, page, pageSize);
//        }

//        public ListPage<Project> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            var projects = _projectRepository.SearchSort(search, sortKey, ascending);
//            var filteredProjects = GetFilteredProjects(projects, userId);
//            var total = filteredProjects.Count;

//            var filteredProjectPage = GetPage(filteredProjects, page, pageSize);
//            EnrichProjectData(filteredProjectPage, userId);

//            return new ListPage<Project>(filteredProjectPage, total);
//        }

//        public List<Project> ListForUser(string userId, string targetUserId, Permission? permission, string search, int page, int pageSize)
//        {
//            var projectMemberships = _membershipRepository.List(p => p.UserId == targetUserId && p.CommunityEntityType == CommunityEntityType.Project && !p.Deleted);
//            var projectIds = projectMemberships
//              .Select(m => m.CommunityEntityId)
//              .ToList();

//            var projects = _projectRepository.SearchGet(projectIds, search);
//            var filteredProjects = GetFilteredProjects(projects, userId, permission != null ? new List<Permission> { permission.Value } : null, page, pageSize);
//            EnrichProjectData(filteredProjects, userId);
//            EnrichCommunityEntityDataWithContribution(filteredProjects, projectMemberships, targetUserId);

//            return filteredProjects;
//        }

//        public List<Project> ListForPaperExternalId(string userId, string externalId)
//        {
//            var paper = _paperRepository.Get(p => p.ExternalId == externalId && !p.Deleted);
//            var projectIds = paper.FeedIds; //at this point, not all ids in projectIds" will be IDs of projects - other community entity ids and user ids can be present in the mix

//            var projects = _projectRepository.Get(projectIds);
//            var filteredProjects = GetFilteredProjects(projects, userId, null);
//            EnrichProjectData(filteredProjects, userId);

//            return filteredProjects;
//        }

//        public List<Project> ListForCommunity(string userId, string communityId, string search, int page, int pageSize)
//        {
//            var projectIds = _relationRepository.List(r => r.TargetCommunityEntityId == communityId && r.SourceCommunityEntityType == CommunityEntityType.Project && !r.Deleted)
//                 .Select(pn => pn.SourceCommunityEntityId)
//                 .ToList();

//            var projects = _projectRepository.SearchGet(projectIds, search);
//            var filteredProjects = GetFilteredProjects(projects, userId, null, page, pageSize);
//            EnrichProjectData(filteredProjects, userId);

//            return filteredProjects;
//        }

//        public List<Project> ListForNode(string userId, string nodeId, string search, int page, int pageSize)
//        {
//            var projectIds = _relationRepository.List(r => r.TargetCommunityEntityId == nodeId && r.SourceCommunityEntityType == CommunityEntityType.Project && !r.Deleted)
//                 .Select(pn => pn.SourceCommunityEntityId)
//                 .ToList();

//            var projects = _projectRepository.SearchGet(projectIds, search);
//            var filteredProjects = GetFilteredProjects(projects, userId, null, page, pageSize);
//            EnrichProjectData(filteredProjects, userId);

//            return filteredProjects;
//        }

//        public List<Project> ListForOrganization(string userId, string organizationId, string search, int page, int pageSize)
//        {
//            var projectIds = _relationRepository.List(r => r.TargetCommunityEntityId == organizationId && r.SourceCommunityEntityType == CommunityEntityType.Project && !r.Deleted)
//              .Select(pn => pn.SourceCommunityEntityId)
//              .ToList();

//            var projects = _projectRepository.SearchGet(projectIds, search);
//            var filteredProjects = GetFilteredProjects(projects, userId, null, page, pageSize);
//            EnrichProjectData(filteredProjects, userId);

//            return filteredProjects;
//        }

//        public int CountForNode(string currentUserId, string nodeId, string search)
//        {
//            var projectIds = _relationRepository.List(r => r.TargetCommunityEntityId == nodeId && r.SourceCommunityEntityType == CommunityEntityType.Project && !r.Deleted)
//              .Select(pn => pn.SourceCommunityEntityId)
//              .ToList();

//            var projects = _projectRepository.SearchGet(projectIds, search);
//            var filteredProjects = GetFilteredProjects(projects, currentUserId, null);
//            return filteredProjects.Count;
//        }

//        public async Task UpdateAsync(Project project)
//        {
//            await _projectRepository.UpdateAsync(project);
//        }

//        public async Task DeleteAsync(string id)
//        {
//            await DeleteCommunityEntityAsync(id);
//            await _projectRepository.DeleteAsync(id);
//        }
//    }
//}
