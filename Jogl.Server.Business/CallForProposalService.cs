using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using Jogl.Server.Email;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Jogl.Server.Business
{
    public class CallForProposalService : BaseService, ICallForProposalService
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public CallForProposalService(IWorkspaceRepository workspaceRepository, IEmailService emailService, INotificationService notificationService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _workspaceRepository = workspaceRepository;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<string> CreateAsync(CallForProposal callForProposal)
        {
            var feed = new Feed()
            {
                CreatedUTC = callForProposal.CreatedUTC,
                CreatedByUserId = callForProposal.CreatedByUserId,
                Type = FeedType.CallForProposal,
            };

            var id = await _feedRepository.CreateAsync(feed);
            callForProposal.Id = ObjectId.Parse(id);
            callForProposal.FeedId = id;

            if (callForProposal.Onboarding == null)
                callForProposal.Onboarding = new OnboardingConfiguration
                {
                    Presentation = new OnboardingPresentation { Items = new List<OnboardingPresentationItem> { } },
                    Questionnaire = new OnboardingQuestionnaire { Items = new List<OnboardingQuestionnaireItem> { } },
                    Rules = new OnboardingRules { Text = string.Empty }
                };

            if (callForProposal.Settings == null)
                callForProposal.Settings = new List<string>();

            if (callForProposal.Tabs == null)
                callForProposal.Tabs = new List<string>();

            if (callForProposal.Template == null)
                callForProposal.Template = new CallForProposalTemplate
                {
                    Questions = new List<CallForProposalTemplateQuestion>(),
                };


            var callForProposalId = await _callForProposalsRepository.CreateAsync(callForProposal);

            //create cfp membership record
            var membership = new Membership
            {
                UserId = callForProposal.CreatedByUserId,
                CreatedByUserId = callForProposal.CreatedByUserId,
                CreatedUTC = callForProposal.CreatedUTC,
                AccessLevel = AccessLevel.Owner,
                CommunityEntityId = callForProposalId,
                CommunityEntityType = CommunityEntityType.CallForProposal,
            };

            await _membershipRepository.CreateAsync(membership);

            //create user feed record
            await _userFeedRecordRepository.SetFeedReadAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);

            //update activity date on parent community
            var community = _workspaceRepository.Get(callForProposal.ParentCommunityEntityId);
            community.LastActivityUTC = callForProposal.UpdatedUTC;
            await _workspaceRepository.UpdateAsync(community);

            return callForProposalId;
        }

        public CallForProposal Get(string callForProposalId, string userId)
        {
            var callForProposal = _callForProposalsRepository.Get(callForProposalId);
            if (callForProposal == null)
                return null;

            EnrichCallForProposalData(new CallForProposal[] { callForProposal }, userId);
            return callForProposal;
        }

        public CallForProposal GetDetail(string callForProposalId, string userId)
        {
            var callForProposal = _callForProposalsRepository.Get(callForProposalId);
            if (callForProposal == null)
                return null;

            EnrichCallForProposalDataDetail(new CallForProposal[] { callForProposal }, userId);
            callForProposal.Path = _feedEntityService.GetPath(callForProposal, userId);

            return callForProposal;
        }

        public List<CallForProposal> Autocomplete(string userId, string search, int page, int pageSize)
        {
            var callForProposals = _callForProposalsRepository.Autocomplete(search);
            var communities = _workspaceRepository.Get(callForProposals.Select(cfp => cfp.ParentCommunityEntityId).Distinct().ToList());
            var sourceRelations = _relationRepository.ListForTargetIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));
            var targetRelations = _relationRepository.ListForSourceIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));

            return GetFilteredCallForProposals(callForProposals, communities, sourceRelations.Union(targetRelations), userId, null, page, pageSize);
        }

        public ListPage<CallForProposal> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var callForProposals = _callForProposalsRepository.SearchSort(search, sortKey, ascending);
            var communities = _workspaceRepository.Get(callForProposals.Select(cfp => cfp.ParentCommunityEntityId).Distinct().ToList());
            var sourceRelations = _relationRepository.ListForTargetIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));
            var targetRelations = _relationRepository.ListForSourceIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));

            var filteredCallsForProposals = GetFilteredCallForProposals(callForProposals, communities, sourceRelations.Union(targetRelations), userId, null);
            var total = filteredCallsForProposals.Count;

            var filteredCFPPage = GetPage(filteredCallsForProposals, page, pageSize);
            EnrichCallForProposalData(filteredCFPPage, userId);

            return new ListPage<CallForProposal>(filteredCFPPage, total);
        }

        //public List<Project> ListForUser(string userId, string targetUserId, Permission? permission, string search, int page, int pageSize)
        //{
        //    var portfolioProjectMemberships = _membershipRepository.List(p => p.UserId == targetUserId && p.CommunityEntityType == CommunityEntityType.Project && !p.Deleted);
        //    var portfolioProjectIds = portfolioProjectMemberships.Select(m => m.CommunityEntityId).ToList();
        //    var portfolioProjects = _projectRepository.Get(portfolioProjectIds);
        //    var portfolioCommunityRelations = _relationRepository.ListForSourceIds(portfolioProjectIds, CommunityEntityType.Workspace); //TODO remove duplicate call
        //    var portfolioNodeRelations = _relationRepository.ListForSourceIds(portfolioProjectIds, CommunityEntityType.Node);//TODO remove duplicate call

        //    var filteredProjects = GetFilteredProjects(portfolioProjects, portfolioCommunityRelations, portfolioNodeRelations, userId, search, permission != null ? new List<Permission> { permission.Value } : null, page, pageSize);
        //    EnrichProjectData(filteredProjects, userId);
        //    EnrichCommunityEntityDataWithContribution(filteredProjects, portfolioProjectMemberships, targetUserId);

        //    return filteredProjects;
        //}

        //public List<Project> ListForPaperExternalId(string userId, string externalId)
        //{
        //    var paper = _paperRepository.Get(p => p.ExternalId == externalId && !p.Deleted);
        //    var projectIds = paper.FeedIds; //at this point, not all ids in projectIds" will be IDs of projects - other community entity ids and user ids can be present in the mix

        //    var projects = _projectRepository.Get(projectIds);
        //    var projectCommunityRelations = _relationRepository.ListForSourceIds(projectIds, CommunityEntityType.Workspace); //TODO remove duplicate call
        //    var projectNodeRelations = _relationRepository.ListForSourceIds(projectIds, CommunityEntityType.Node);//TODO remove duplicate call

        //    var filteredProjects = GetFilteredProjects(projects, projectCommunityRelations, projectNodeRelations, userId, null, null);
        //    EnrichProjectData(filteredProjects, userId);

        //    return filteredProjects;
        //}

        public List<CallForProposal> ListForCommunity(string userId, string communityId, string search, int page, int pageSize)
        {
            var callForProposals = _callForProposalsRepository.SearchList(cfp => cfp.ParentCommunityEntityId == communityId && !cfp.Deleted, search);
            var communities = _workspaceRepository.Get(callForProposals.Select(cfp => cfp.ParentCommunityEntityId).Distinct().ToList());
            var sourceRelations = _relationRepository.ListForTargetIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));
            var targetRelations = _relationRepository.ListForSourceIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));

            var filteredCallForProposals = GetFilteredCallForProposals(callForProposals, communities, sourceRelations.Union(targetRelations), userId, null, page, pageSize);
            EnrichCallForProposalData(filteredCallForProposals, userId);

            return filteredCallForProposals;
        }

        public List<CallForProposal> ListForNode(string userId, string nodeId, string search, int page, int pageSize)
        {
            var communityRelations = _relationRepository.List(r => r.TargetCommunityEntityId == nodeId && r.SourceCommunityEntityType == CommunityEntityType.Workspace && !r.Deleted);
            var communityIds = communityRelations.Select(m => m.SourceCommunityEntityId).ToList();

            var callForProposals = _callForProposalsRepository.SearchList(cfp => communityIds.Contains(cfp.ParentCommunityEntityId) && !cfp.Deleted, search);
            var communities = _workspaceRepository.Get(callForProposals.Select(cfp => cfp.ParentCommunityEntityId).Distinct().ToList());
            var sourceRelations = _relationRepository.ListForTargetIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));
            var targetRelations = _relationRepository.ListForSourceIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));

            var filteredCallForProposals = GetFilteredCallForProposals(callForProposals, communities, sourceRelations.Union(targetRelations), userId, null, page, pageSize);
            EnrichCallForProposalData(filteredCallForProposals, userId);

            return filteredCallForProposals;
        }

        public int CountForNode(string userId, string nodeId, string search)
        {
            var communityRelations = _relationRepository.List(r => r.TargetCommunityEntityId == nodeId && r.SourceCommunityEntityType == CommunityEntityType.Workspace && !r.Deleted);
            var communityIds = communityRelations.Select(m => m.SourceCommunityEntityId).ToList();

            var callForProposals = _callForProposalsRepository.SearchList(cfp => communityIds.Contains(cfp.ParentCommunityEntityId) && !cfp.Deleted, search);
            var communities = _workspaceRepository.Get(callForProposals.Select(cfp => cfp.ParentCommunityEntityId).Distinct().ToList());
            var sourceRelations = _relationRepository.ListForTargetIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));
            var targetRelations = _relationRepository.ListForSourceIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));

            var filteredCallForProposals = GetFilteredCallForProposals(callForProposals, communities, sourceRelations.Union(targetRelations), userId, null);
            return filteredCallForProposals.Count;
        }

        public async Task UpdateAsync(CallForProposal callForProposal)
        {
            var proposals = _proposalRepository.List(p => p.CallForProposalId == callForProposal.Id.ToString() && !p.Deleted);
            var existingCallForProposal = _callForProposalsRepository.Get(callForProposal.Id.ToString());
            if (proposals.Any())
            {
                if (HasTemplateChanged(existingCallForProposal, callForProposal))
                    foreach (var proposal in proposals)
                    {
                        if (proposal.Status != ProposalStatus.Draft)
                        {
                            proposal.Status = ProposalStatus.Draft;
                            await _proposalRepository.UpdateAsync(proposal);
                        }
                    }
            }

            await _callForProposalsRepository.UpdateAsync(callForProposal);
        }

        public bool HasTemplateChanged(CallForProposal existingCFP, CallForProposal updatedCFP)
        {
            if (updatedCFP.Template == null)
                return true;

            if (existingCFP.Template.Questions.Count != updatedCFP.Template.Questions.Count)
                return true;

            if (existingCFP.Template.Questions.Any(q => !updatedCFP.Template.Questions.Any(q2 => q.Key == q2.Key)))
                return true;

            if (updatedCFP.Template.Questions.Any(q2 => !existingCFP.Template.Questions.Any(q => q.Key == q2.Key)))
                return true;

            if (existingCFP.Template.Questions.Any(q => updatedCFP.Template.Questions.Any(q2 => q.Key == q2.Key && HasQuestionChanged(q, q2))))
                return true;

            if (updatedCFP.Template.Questions.Any(q2 => existingCFP.Template.Questions.Any(q => q.Key == q2.Key && HasQuestionChanged(q2, q))))
                return true;

            return false;
        }

        private bool HasQuestionChanged(CallForProposalTemplateQuestion existing, CallForProposalTemplateQuestion updated)
        {
            if (existing.Title != updated.Title)
                return true;

            if (existing.Description != updated.Description)
                return true;

            if (existing.Order != updated.Order)
                return true;

            if (existing.Type != updated.Type)
                return true;

            if (existing.MaxLength != updated.MaxLength)
                return true;

            return false;
        }

        public async Task DeleteAsync(string id)
        {
            await DeleteCommunityEntityAsync(id);
            await _proposalRepository.DeleteAsync(p => p.CallForProposalId == id && !p.Deleted);
            await _callForProposalsRepository.DeleteAsync(id);
        }

        public async Task SendMessageToUsersAsync(string cfpId, List<string> userIds, string subject, string message, string url)
        {
            var cfp = _callForProposalsRepository.Get(cfpId);
            var users = _userRepository.Get(userIds);
            var members = _membershipRepository.List(m => m.CommunityEntityId == cfpId && !m.Deleted);
            var filteredUsers = users.Where(u => members.Any(a => a.UserId == u.Id.ToString()));

            await _emailService.SendEmailAsync(filteredUsers.ToDictionary(u => u.Email, u => (object)new
            {
                first_name = u.FirstName,
                text = message,
                subject = subject,
                cfp_title = cfp.Title,
                cfp_url = url,
                LANGUAGE = u.Language
            }), EmailTemplate.CFPMessage);
        }
    }
}
