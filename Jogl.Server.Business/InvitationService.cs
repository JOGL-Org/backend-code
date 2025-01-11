using Jogl.Server.Business.DTO;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Notifications;
using MongoDB.Bson;

namespace Jogl.Server.Business
{
    public class InvitationService : BaseService, IInvitationService
    {
        private readonly IOnboardingQuestionnaireInstanceRepository _onboardingQuestionnaireInstanceRepository;
        private readonly IInvitationKeyRepository _invitationKeyRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly INotificationService _notificationService;
        private readonly INotificationFacade _notificationFacade;

        public InvitationService(IOnboardingQuestionnaireInstanceRepository onboardingQuestionnaireInstanceRepository, IInvitationKeyRepository invitationKeyRepository, IWorkspaceRepository workspaceRepository, IOrganizationRepository organizationRepository, INotificationService notificationService, INotificationFacade notificationFacade, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _onboardingQuestionnaireInstanceRepository = onboardingQuestionnaireInstanceRepository;
            _invitationKeyRepository = invitationKeyRepository;
            _workspaceRepository = workspaceRepository;
            _organizationRepository = organizationRepository;
            _notificationService = notificationService;
            _notificationFacade = notificationFacade;
        }

        public async Task<string> CreateAsync(Invitation invitation)
        {
            if (!string.IsNullOrEmpty(invitation.InviteeEmail))
            {
                var inviteeUser = _userRepository.GetForEmail(invitation.InviteeEmail);
                if (inviteeUser != null)
                {
                    invitation.InviteeUserId = inviteeUser.Id.ToString();
                    invitation.InviteeEmail = null;
                    return await CreateWithIdAsync(invitation);
                }

                return await CreateWithEmailAsync(invitation);
            }

            return await CreateWithIdAsync(invitation);
        }

        public async Task<List<OperationResult<string>>> CreateMultipleAsync(IEnumerable<Invitation> invitations, string redirectUrl)
        {
            var res = new List<OperationResult<string>>();
            foreach (var grp in invitations.GroupBy(i => i.Entity))
            {
                var communityEntity = grp.Key;

                var existingInvitations = _invitationRepository.List(i => communityEntity.Id.ToString() == i.CommunityEntityId && i.Status == InvitationStatus.Pending && !i.Deleted);
                var existingMemberships = _membershipRepository.List(m => communityEntity.Id.ToString() == m.CommunityEntityId && !m.Deleted);

                //repoint email invites for email invitations to existing users
                var emails = grp.Where(i => !string.IsNullOrEmpty(i.InviteeEmail)).Select(i => i.InviteeEmail).ToList();
                var emailUsers = _userRepository.List(u => emails.Any(email => u.Email == email && !u.Deleted));
                foreach (var invitation in grp.Where(i => !string.IsNullOrEmpty(i.InviteeEmail)))
                {
                    var user = emailUsers.SingleOrDefault(u => u.Email.Equals(invitation.InviteeEmail, StringComparison.InvariantCultureIgnoreCase));
                    if (user != null)
                    {
                        invitation.InviteeUserId = user.Id.ToString();
                        invitation.InviteeEmail = null;
                    }
                }

                //populate users for user invites
                var ids = grp.Where(i => !string.IsNullOrEmpty(i.InviteeUserId)).Select(i => i.InviteeUserId).ToList();
                var inviteeUsers = _userRepository.Get(ids);
                foreach (var invitation in grp.Where(i => !string.IsNullOrEmpty(i.InviteeUserId)))
                {
                    invitation.User = inviteeUsers.SingleOrDefault(u => u.Id.ToString() == invitation.InviteeUserId);
                }

                //filter out duplicates and empty invites
                var finalizedInvitations = new List<Invitation>();
                foreach (var invitation in grp)
                {
                    //empty invites
                    if (string.IsNullOrEmpty(invitation.InviteeUserId) && string.IsNullOrEmpty(invitation.InviteeEmail))
                    {
                        res.Add(new OperationResult<string> { OriginalPayload = invitation.InviteeUserId ?? invitation.InviteeEmail, Status = Status.Conflict });
                        continue;
                    }

                    //duplicates
                    if (existingInvitations.Any(ei => ei.InviteeUserId == invitation.InviteeUserId && ei.InviteeEmail == invitation.InviteeEmail))
                    {
                        res.Add(new OperationResult<string> { OriginalPayload = invitation.InviteeUserId ?? invitation.InviteeEmail, Status = Status.Conflict });
                        continue;
                    }

                    if (!string.IsNullOrEmpty(invitation.InviteeUserId) && existingMemberships.Any(m => m.UserId == invitation.InviteeUserId))
                    {
                        res.Add(new OperationResult<string> { OriginalPayload = invitation.InviteeUserId ?? invitation.InviteeEmail, Status = Status.Conflict });
                        continue;
                    }

                    if (finalizedInvitations.Any(ei => ei.InviteeUserId == invitation.InviteeUserId && ei.InviteeEmail == invitation.InviteeEmail))
                    {
                        res.Add(new OperationResult<string> { OriginalPayload = invitation.InviteeUserId ?? invitation.InviteeEmail, Status = Status.Conflict });
                        continue;
                    }

                    finalizedInvitations.Add(invitation);
                    res.Add(new OperationResult<string> { OriginalPayload = invitation.InviteeUserId ?? invitation.InviteeEmail, Status = Status.OK });
                }

                await CreateMultipleWithIdAsync(finalizedInvitations.Where(i => !string.IsNullOrEmpty(i.InviteeUserId)).ToList(), redirectUrl);
                await CreateMultipleWithEmailAsync(finalizedInvitations.Where(i => !string.IsNullOrEmpty(i.InviteeEmail)).ToList(), redirectUrl);
            }

            return res;
        }

        private async Task<List<OperationResult<string>>> CreateMultipleWithIdAsync(List<Invitation> invitations, string redirectUrl)
        {
            if (invitations == null || invitations.Count() == 0)
                return new List<OperationResult<string>>();

            //create
            await _invitationRepository.CreateAsync(invitations);

            //process notifications
            EnrichEntitiesWithCreatorData(invitations);

            var invites = invitations.Where(i => i.Type == InvitationType.Invitation).ToList();
            await _notificationService.NotifyInvitesCreatedAsync(invites);
            await _notificationFacade.NotifyInvitedAsync(invites);

            var requests = invitations.Where(i => i.Type == InvitationType.Request).ToList();
            await _notificationService.NotifyRequestsCreatedAsync(requests);

            //return
            return invitations
                .Select(i => new OperationResult<string> { Id = i.Id.ToString(), OriginalPayload = i.User.Id.ToString(), Status = Status.OK })
                .ToList();
        }

        private async Task<List<OperationResult<string>>> CreateMultipleWithEmailAsync(List<Invitation> invitations, string redirectUrl)
        {
            if (invitations == null || invitations.Count() == 0)
                return new List<OperationResult<string>>();

            //create
            await _invitationRepository.CreateAsync(invitations);

            //process notifications
            await _notificationFacade.NotifyInvitedAsync(invitations);

            //return
            return invitations
                .Select(i => new OperationResult<string> { Id = i.Id.ToString(), OriginalPayload = i.InviteeEmail, Status = Status.OK })
                .ToList();
        }

        private async Task<string> CreateWithIdAsync(Invitation invitation)
        {
            //create entity
            var id = await _invitationRepository.CreateAsync(invitation);

            //process emails
            var inviterUser = _userRepository.Get(invitation.CreatedByUserId);

            //process notifications
            switch (invitation.Type)
            {
                case InvitationType.Invitation:
                    await _notificationService.NotifyInviteCreatedAsync(invitation, inviterUser);
                    await _notificationFacade.NotifyInvitedAsync(invitation);
                    break;
                case InvitationType.Request:
                    await _notificationService.NotifyRequestCreatedAsync(invitation);
                    break;
            }

            //return
            return id;
        }

        private async Task<string> CreateWithEmailAsync(Invitation invitation)
        {
            await _notificationFacade.NotifyInvitedAsync(invitation);
            return await _invitationRepository.CreateAsync(invitation);
        }

        public Invitation Get(string invitationId)
        {
            var id = ObjectId.Parse(invitationId);
            return _invitationRepository.Get(i => i.Id == id && i.Status == InvitationStatus.Pending);
        }

        public Invitation Get(string invitationId, string userId)
        {
            return _invitationRepository.Get(i => i.InviteeUserId == userId && i.CommunityEntityId == invitationId && i.Status == InvitationStatus.Pending && !i.Deleted);
        }

        public Invitation GetForUserAndEntity(string userId, string entityId)
        {
            return _invitationRepository.Get(i => i.InviteeUserId == userId && i.CommunityEntityId == entityId && i.Status == InvitationStatus.Pending && !i.Deleted);
        }

        public Invitation GetForEmailAndEntity(string email, string entityId)
        {
            //check if user exists
            var user = _userRepository.GetForEmail(email);
            if (user != null)
            {
                //if they do, read invitation for user id instead
                return GetForUserAndEntity(user.Id.ToString(), entityId);
            }

            return _invitationRepository.Get(i => i.InviteeEmail == email && i.CommunityEntityId == entityId && i.Status == InvitationStatus.Pending && !i.Deleted);
        }

        public List<Invitation> List(string userId)
        {
            return _invitationRepository.List((i) => i.InviteeUserId == userId && !i.Deleted && i.Status == InvitationStatus.Pending, 1, 1000);
        }

        public List<Invitation> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, bool loadDetails = false)
        {
            var invitations = _invitationRepository.List(i => i.CommunityEntityId == entityId && !i.Deleted && i.Status == InvitationStatus.Pending);
            var invitationUserIds = invitations.Select(m => m.InviteeUserId).Where(id => !string.IsNullOrEmpty(id)).ToList();
            var users = _userRepository.Get(invitationUserIds);

            foreach (var member in invitations)
            {
                member.User = users.SingleOrDefault(u => u.Id.ToString() == member.InviteeUserId);
            }

            var invitationPage = invitations
                .Where(m => (string.IsNullOrEmpty(search) || (m.User != null && m.User.FirstName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || m.User.LastName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || m.User.Username.Contains(search, StringComparison.CurrentCultureIgnoreCase)))
                       && !m.Deleted)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!loadDetails)
                return invitationPage;

            EnrichUserData(invitationPage.Where(i => i.User != null).Select(i => i.User).ToList(), currentUserId);
            return invitationPage;
        }

        public async Task AcceptAsync(Invitation invitation)
        {
            //update invitation status
            invitation.Status = InvitationStatus.Accepted;
            await _invitationRepository.UpdateAsync(invitation);

            //create project membership record
            var membership = new Membership
            {
                UserId = invitation.InviteeUserId,
                CreatedByUserId = invitation.CreatedByUserId,
                CreatedUTC = invitation.CreatedUTC,
                AccessLevel = invitation.AccessLevel,
                CommunityEntityId = invitation.CommunityEntityId,
                CommunityEntityType = invitation.CommunityEntityType,
            };

            await _membershipRepository.CreateAsync(membership);

            //create user feed record
            await _userFeedRecordRepository.SetFeedReadAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);

            //process notifications
            switch (invitation.Type)
            {
                case InvitationType.Request:
                    await _notificationService.NotifyRequestAcceptedAsync(invitation);
                    await _notificationService.NotifyRequestCreatedWithdrawAsync(invitation);
                    break;
                case InvitationType.Invitation:
                    await _notificationService.NotifyInviteWithdrawAsync(invitation);
                    break;
            }

            //auto-join channels
            foreach (var channel in _channelRepository.List(c => c.CommunityEntityId == membership.CommunityEntityId && c.AutoJoin && !c.Deleted))
            {
                await _membershipRepository.CreateAsync(new Membership
                {
                    AccessLevel = AccessLevel.Member,
                    CommunityEntityId = channel.Id.ToString(),
                    CommunityEntityType = CommunityEntityType.Channel,
                    CreatedUTC = membership.CreatedUTC,
                    CreatedByUserId = membership.CreatedByUserId,
                    UserId = membership.UserId,
                });
            }

            await _notificationService.NotifyMemberJoinedAsync(membership);
        }

        public async Task RejectAsync(Invitation invitation)
        {
            invitation.Status = InvitationStatus.Rejected;
            await _invitationRepository.UpdateAsync(invitation);

            //process notifications
            switch (invitation.Type)
            {
                case InvitationType.Request:
                    await _notificationService.NotifyRequestCreatedWithdrawAsync(invitation);
                    break;
                case InvitationType.Invitation:
                    await _notificationService.NotifyInviteWithdrawAsync(invitation);
                    break;
            }

            var answers = _onboardingQuestionnaireInstanceRepository.Get(i => i.UserId == invitation.InviteeUserId && i.CommunityEntityId == invitation.CommunityEntityId && !i.Deleted);
            if (answers != null)
                await _onboardingQuestionnaireInstanceRepository.DeleteAsync(answers.Id.ToString());
        }

        public async Task ResendAsync(Invitation invitation)
        {
            await _invitationRepository.UpdateAsync(invitation);

            //process notifications
            switch (invitation.Type)
            {
                case InvitationType.Request:
                    throw new Exception("Cannot resend an invitation of type request");
                case InvitationType.Invitation:
                    await _notificationFacade.NotifyInvitedAsync(invitation);
                    break;
            }
        }

        public List<Invitation> ListForUser(string userId, InvitationType? type = null)
        {
            var invitations = _invitationRepository.List(i =>
                i.InviteeUserId == userId
            && !i.Deleted && i.Status == InvitationStatus.Pending
            && (i.Type == InvitationType.Invitation || type == null));

            var invitationProjectIds = invitations.Where(i => i.CommunityEntityType == CommunityEntityType.Project).Select(m => m.CommunityEntityId).ToList();
            var invitationCommunityIds = invitations.Where(i => i.CommunityEntityType == CommunityEntityType.Workspace).Select(m => m.CommunityEntityId).ToList();
            var communities = _workspaceRepository.Get(invitationCommunityIds);

            foreach (var invite in invitations)
            {
                switch (invite.CommunityEntityType)
                {
                    case CommunityEntityType.Workspace:
                        invite.Entity = communities.Single(e => e.Id == ObjectId.Parse(invite.CommunityEntityId));
                        break;
                }
            }

            return invitations;
        }

        private string GetEntityType(CommunityEntityType type)
        {
            switch (type)
            {
                case CommunityEntityType.Node: return "Hub";
                default: return type.ToString();
            }
        }

        public InvitationKey GetInvitationKey(string entityId, string key)
        {
            return _invitationKeyRepository.Get(ik => ik.InviteKey == key && ik.CommunityEntityId == entityId);
        }

        public async Task<string> GetInvitationKeyForEntityAsync(string entityId)
        {
            var invitationKey = _invitationKeyRepository.Get(ik => ik.CommunityEntityId == entityId);
            if (invitationKey == null)
            {
                var feed = _feedRepository.Get(entityId);
                if (feed == null)
                    return null;

                invitationKey = new InvitationKey
                {
                    CommunityEntityId = entityId,
                    InviteKey = ObjectId.GenerateNewId().ToString(),
                    CommunityEntityType = _feedEntityService.GetType(feed.Type)
                };

               await _invitationKeyRepository.CreateAsync(invitationKey);
            }

            return invitationKey.InviteKey;
        }
    }
}