using Jogl.Server.Data;
using Jogl.Server.DB;
using System.Reflection.Emit;

namespace Jogl.Server.Business
{
    public class ProposalService : IProposalService
    {
        private readonly IProposalRepository _proposalRepository;
        private readonly ICallForProposalRepository _callForProposalRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserFeedRecordRepository _userFeedRecordRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly INotificationService _notificationService;
        const string LABEL_APPLICANT = "applicant";
        const string LABEL_LEAD_APPLICANT = "lead_applicant";

        public ProposalService(IProposalRepository proposalRepository, ICallForProposalRepository callForProposalRepository, IWorkspaceRepository workspaceRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository,IUserRepository userRepository, IUserFeedRecordRepository userFeedRecordRepository, IDocumentRepository documentRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, INotificationService notificationService)
        {
            _proposalRepository = proposalRepository;
            _callForProposalRepository = callForProposalRepository;
            _workspaceRepository = workspaceRepository;
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
            _userFeedRecordRepository = userFeedRecordRepository;
            _documentRepository = documentRepository;
            _notificationService = notificationService;
        }

        public async Task<string> CreateAsync(Proposal proposal)
        {
            var cfp = _callForProposalRepository.Get(proposal.CallForProposalId);
            if (!cfp.SubmissionsFrom.HasValue || cfp.SubmissionsFrom > DateTime.UtcNow)
                throw new Exception("Call for proposal is not yet open for submissions");

            //populate authors
            proposal.UserIds = _membershipRepository.List(m => m.CommunityEntityId == proposal.SourceCommunityEntityId && !m.Deleted)
                .Select(m => m.UserId)
                .ToList();

            if (proposal.Answers == null)
                proposal.Answers = new List<ProposalAnswer>();

            //create entity
            var id = await _proposalRepository.CreateAsync(proposal);

            //process notifications
            //await _notificationService.NotifyResourceCreatedAsync(resource);

            //return
            return id;
        }

        public Proposal Get(string currentUserId, string proposalId)
        {
            var proposal = _proposalRepository.Get(p => p.Id.ToString() == proposalId && !p.Deleted);
            if (proposal == null)
                return null;

            EnrichProposalData(new List<Proposal> { proposal }, true, true, true);
            return proposal;
        }

        public Proposal GetForProjectAndCFP(string projectId, string callForProposalId)
        {
            return _proposalRepository.Get(p => p.SourceCommunityEntityId == projectId && p.CallForProposalId == callForProposalId && !p.Deleted);
        }

        public List<Proposal> ListForProject(string projectId)
        {
            var list = _proposalRepository.List(p => p.SourceCommunityEntityId == projectId && !p.Deleted);
            EnrichProposalData(list, false, true, false);

            return list
                .Where(p => p.CallForProposal != null)
                .ToList();
        }

        public List<Proposal> ListForUser(string userId)
        {
            var list = _proposalRepository.List(p => p.UserIds.Contains(userId) && !p.Deleted);
            EnrichProposalData(list, true, true, true);

            return list
                .Where(p => p.CallForProposal != null)
                .ToList();
        }

        public List<Proposal> ListForCFP(string currentUserId, string callForProposalsId)
        {
            var list = _proposalRepository.List(p => p.CallForProposalId == callForProposalsId && p.UserIds.Contains(currentUserId) && !p.Deleted);
            EnrichProposalData(list, true, false, false);

            return list
                .Where(p => p.SourceCommunityEntity != null)
                .ToList();
        }

        public List<Proposal> ListForCFPAdmin(string currentUserId, string callForProposalsId)
        {
            var list = _proposalRepository.List(p => p.CallForProposalId == callForProposalsId && (p.Status != ProposalStatus.Draft || p.UserIds.Contains(currentUserId)) && !p.Deleted);
            EnrichProposalData(list, true, false, false);

            return list
                .Where(p => p.SourceCommunityEntity != null)
                .ToList();
        }

        public async Task UpdateAsync(Proposal proposal)
        {
            var existingProposal = _proposalRepository.Get(proposal.Id.ToString());
            if (existingProposal.Status == ProposalStatus.Submitted && HasDataChanged(existingProposal, proposal))
                throw new Exception("Cannot change proposal answers");

            await _proposalRepository.UpdateAsync(proposal);
        }

        private bool HasDataChanged(Proposal existing, Proposal updated)
        {
            if (existing.Answers == null)
                return true;

            if (updated.Answers == null)
                return true;

            if (existing.Answers.Count != updated.Answers.Count)
                return true;

            if (existing.Answers.Any(a => !updated.Answers.Any(a2 => a.QuestionKey == a2.QuestionKey)))
                return true;

            if (updated.Answers.Any(a2 => !existing.Answers.Any(a => a.QuestionKey == a2.QuestionKey)))
                return true;

            if (existing.Answers.Any(a => updated.Answers.Any(a2 => a.QuestionKey == a2.QuestionKey && HasAnswerChanged(a, a2))))
                return true;

            if (updated.Answers.Any(a2 => existing.Answers.Any(a => a.QuestionKey == a2.QuestionKey && HasAnswerChanged(a2, a))))
                return true;

            return false;
        }

        private bool HasAnswerChanged(ProposalAnswer existing, ProposalAnswer updated)
        {
            if (updated.Answer.Any(a => !existing.Answer.Contains(a)))
                return true;

            if (existing.Answer.Any(a => !updated.Answer.Contains(a)))
                return true;

            if (existing.AnswerDocumentId != updated.AnswerDocumentId)
                return true;

            return false;
        }

        public async Task JoinMembersToCFPAsync(Proposal proposal)
        {
            var existingMemberships = _membershipRepository.List(m => m.CommunityEntityId == proposal.CallForProposalId && !m.Deleted);

            //create or update membership records on cfp
            foreach (var userId in proposal.UserIds)
            {
                var newLabel = userId == proposal.CreatedByUserId ? LABEL_LEAD_APPLICANT : LABEL_APPLICANT;
                var membership = existingMemberships.FirstOrDefault(m => m.UserId == userId);
                if (membership != null)
                {
                    //see if current membership needs to be updated
                    if (membership.Labels == null)
                        membership.Labels = new List<string>();

                    if (!membership.Labels.Contains(newLabel))
                    {
                        membership.Labels.Add(newLabel);
                        await _membershipRepository.UpdateAsync(membership);
                    }
                }
                else
                {
                    //create new membership
                    membership = new Membership
                    {
                        UserId = userId,
                        CreatedByUserId = proposal.UpdatedByUserId,
                        CreatedUTC = proposal.UpdatedUTC.Value,
                        AccessLevel = AccessLevel.Member,
                        CommunityEntityId = proposal.CallForProposalId,
                        CommunityEntityType = CommunityEntityType.CallForProposal,
                        Labels = new List<string> { newLabel }
                    };

                    await _membershipRepository.CreateAsync(membership);

                    //create user feed record
                    await _userFeedRecordRepository.SetFeedReadAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);
                }
            }
        }

        public async Task DeleteAsync(string id)
        {
            await _proposalRepository.DeleteAsync(id);
        }

        private void EnrichProposalData(IEnumerable<Proposal> proposals, bool loadProjects = true, bool loadCommunities = true, bool loadDocuments = true)
        {
            var cfps = _callForProposalRepository.Get(proposals.Select(p => p.CallForProposalId).ToList());
            foreach (var propoposal in proposals)
            {
                var cfp = cfps.SingleOrDefault(c => propoposal.CallForProposalId == c.Id.ToString());
                if (cfp == null)
                    continue;

                propoposal.Answers = propoposal.Answers.OrderBy(a => cfp.Template?.Questions?.FirstOrDefault(q => q.Key == a.QuestionKey)?.Order ?? 0).ToList();
            }

            if (loadProjects)
            {
                var projects = _workspaceRepository.Get(proposals.Select(p => p.SourceCommunityEntityId).ToList());
                foreach (var proposal in proposals)
                {
                    proposal.SourceCommunityEntity = projects.FirstOrDefault(p => p.Id.ToString() == proposal.SourceCommunityEntityId);
                }
            }

            if (loadCommunities)
            {
                var communities = _workspaceRepository.Get(cfps.Select(cfp => cfp.ParentCommunityEntityId).ToList());
                foreach (var proposal in proposals)
                {
                    proposal.CallForProposal = cfps.FirstOrDefault(cfp => cfp.Id.ToString() == proposal.CallForProposalId);
                    proposal.ParentCommunityEntity = communities.FirstOrDefault(c => c.Id.ToString() == proposal.CallForProposal?.ParentCommunityEntityId);
                }
            }

            if (loadDocuments)
            {
                var proposalIds = proposals.Select(p => p.Id.ToString()).ToList();
                var documents = _documentRepository.List(d => proposalIds.Contains(d.ProposalId) && !d.Deleted);
                foreach (var proposal in proposals)
                {
                    foreach (var answer in proposal.Answers)
                    {
                        answer.AnswerDocument = documents.FirstOrDefault(d => d.Id.ToString() == answer.AnswerDocumentId);
                    }
                }
            }

            var users = _userRepository.Get(proposals.SelectMany(p => p.UserIds).Distinct().ToList());
            foreach (var proposal in proposals)
            {
                proposal.Users = users.Where(u => proposal.UserIds.Contains(u.Id.ToString())).ToList();
            }
        }
    }
}