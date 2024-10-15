using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using MongoDB.Bson;

namespace Jogl.Server.Business
{
    public class OrganizationService : BaseService, IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly INotificationService _notificationService;

        public OrganizationService(IOrganizationRepository organizationRepository, INotificationService notificationService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _organizationRepository = organizationRepository;
            _notificationService = notificationService;
        }

        public async Task<string> CreateAsync(Organization organization)
        {
            var feed = new Feed()
            {
                CreatedUTC = organization.CreatedUTC,
                CreatedByUserId = organization.CreatedByUserId,
                Type = FeedType.Organization
            };

            var id = await _feedRepository.CreateAsync(feed);
            organization.Id = ObjectId.Parse(id);
            organization.FeedId = id;

            if (organization.Onboarding == null)
                organization.Onboarding = new OnboardingConfiguration
                {
                    Presentation = new OnboardingPresentation { Items = new List<OnboardingPresentationItem> { } },
                    Questionnaire = new OnboardingQuestionnaire { Items = new List<OnboardingQuestionnaireItem> { } },
                    Rules = new OnboardingRules { Text = string.Empty }
                };

            if (organization.Settings == null)
                organization.Settings = new List<string>();

            if (organization.Tabs == null)
                organization.Tabs = new List<string>();

            var organizationId = await _organizationRepository.CreateAsync(organization);

            //create organization membership record
            var membership = new Membership
            {
                UserId = organization.CreatedByUserId,
                CreatedByUserId = organization.CreatedByUserId,
                CreatedUTC = organization.CreatedUTC,
                AccessLevel = AccessLevel.Owner,
                CommunityEntityId = organizationId,
                CommunityEntityType = CommunityEntityType.Organization,
            };

            await _membershipRepository.CreateAsync(membership);

            //create user feed record
            await _userFeedRecordRepository.SetFeedReadAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);

            return organizationId;
        }

        public Organization Get(string organizationId, string userId)
        {
            var organization = _organizationRepository.Get(organizationId);
            if (organization == null)
                return null;

            EnrichOrganizationData(new Organization[] { organization }, userId);
            return organization;
        }

        public Organization GetDetail(string organizationId, string userId)
        {
            var organization = _organizationRepository.Get(organizationId);
            if (organization == null)
                return null;

            EnrichOrganizationDataDetail(new Organization[] { organization }, userId);
            organization.Path = _feedEntityService.GetPath(organization, userId);

            return organization;
        }

        public List<Organization> Autocomplete(string userId, string search, int page, int pageSize)
        {
            var organizations = _organizationRepository.Search(search);
            return GetFilteredOrganizations(organizations, userId, null, page, pageSize);
        }

        public ListPage<Organization> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var organizations = _organizationRepository.SearchSort(search, sortKey, ascending);
            var filteredOrganizations = GetFilteredOrganizations(organizations, userId);
            var total = filteredOrganizations.Count;

            var filteredOrganizationPage = GetPage(filteredOrganizations, page, pageSize);
            EnrichOrganizationData(filteredOrganizationPage, userId);

            return new ListPage<Organization>(filteredOrganizationPage, total);
        }

        public long Count(string currentUserId, string search)
        {
            var organizations = _organizationRepository.Search(search);
            var filteredOrganizations = GetFilteredOrganizations(organizations, currentUserId);

            return filteredOrganizations.Count;
        }

        public List<Organization> ListForUser(string userId, string targetUserId, Permission? permission, string search, int page, int pageSize)
        {
            var ecosystemOrganizationMemberships = _membershipRepository.List(p => p.UserId == targetUserId && p.CommunityEntityType == CommunityEntityType.Organization && !p.Deleted);
            var ecosystemOrganizationIds = ecosystemOrganizationMemberships.Select(m => m.CommunityEntityId).ToList();

            var organizations = _organizationRepository.SearchGet(ecosystemOrganizationIds, search);
            var filteredOrganizations = GetFilteredOrganizations(organizations, userId, permission != null ? new List<Permission> { permission.Value } : null, page, pageSize);
            EnrichOrganizationData(organizations, userId);

            return filteredOrganizations;
        }

        public List<Organization> ListForProject(string userId, string projectId, string search, int page, int pageSize)
        {
            var organizationIds = _relationRepository.List(r => r.SourceCommunityEntityId == projectId && r.TargetCommunityEntityType == CommunityEntityType.Organization && !r.Deleted)
                .Select(r => r.TargetCommunityEntityId)
                .ToList();

            var organizations = _organizationRepository.SearchGet(organizationIds, search);
            var filteredOrganizations = GetFilteredOrganizations(organizations, userId, null, page, pageSize);
            EnrichOrganizationData(filteredOrganizations, userId);

            return filteredOrganizations;
        }

        public List<Organization> ListForCommunity(string userId, string communityId, string search, int page, int pageSize)
        {
            var organizationIds = _relationRepository.List(r => r.SourceCommunityEntityId == communityId && r.TargetCommunityEntityType == CommunityEntityType.Organization && !r.Deleted)
               .Select(r => r.TargetCommunityEntityId)
               .ToList();

            var organizations = _organizationRepository.SearchGet(organizationIds, search);
            var filteredOrganizations = GetFilteredOrganizations(organizations, userId, null, page, pageSize);
            EnrichOrganizationData(filteredOrganizations, userId);

            return filteredOrganizations;
        }

        public List<Organization> ListForNode(string userId, string nodeId, string search, int page, int pageSize)
        {
            var organizationIds = _relationRepository.List(r => r.SourceCommunityEntityId == nodeId && r.TargetCommunityEntityType == CommunityEntityType.Organization && !r.Deleted)
               .Select(r => r.TargetCommunityEntityId)
               .ToList();

            var organizations = _organizationRepository.SearchGet(organizationIds, search);
            var filteredOrganizations = GetFilteredOrganizations(organizations, userId, null, page, pageSize);
            EnrichOrganizationData(filteredOrganizations, userId);

            return filteredOrganizations;
        }

        public int CountForNode(string userId, string nodeId, string search)
        {
            var organizationIds = _relationRepository.List(r => r.SourceCommunityEntityId == nodeId && r.TargetCommunityEntityType == CommunityEntityType.Organization && !r.Deleted)
              .Select(r => r.TargetCommunityEntityId)
              .ToList();

            var organizations = _organizationRepository.SearchGet(organizationIds, search);
            var filteredOrganizations = GetFilteredOrganizations(organizations, userId, null);
            return filteredOrganizations.Count;
        }

        public async Task UpdateAsync(Organization organization)
        {
            await _organizationRepository.UpdateAsync(organization);
        }

        public async Task DeleteAsync(string id)
        {
            await DeleteCommunityEntityAsync(id);
            await _organizationRepository.DeleteAsync(id);
        }
    }
}