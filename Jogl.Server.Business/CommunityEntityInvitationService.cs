using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using MongoDB.Bson;

namespace Jogl.Server.Business
{
    public class CommunityEntityInvitationService : BaseService, ICommunityEntityInvitationService
    {
        private readonly ICommunityEntityInvitationRepository _communityEntityInvitationRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly INodeRepository _nodeRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public CommunityEntityInvitationService(ICommunityEntityInvitationRepository communityEntityInvitationRepository  , IWorkspaceRepository workspaceRepository, INodeRepository nodeRepository, IOrganizationRepository organizationRepository, IEmailService emailService, INotificationService notificationService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _communityEntityInvitationRepository = communityEntityInvitationRepository;
            _workspaceRepository = workspaceRepository;
            _nodeRepository = nodeRepository;
            _organizationRepository = organizationRepository;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<string> CreateAsync(CommunityEntityInvitation invitation, string redirectUrl)
        {
            //create entity
            var id = await _communityEntityInvitationRepository.CreateAsync(invitation);

            //process notifications
            await _notificationService.NotifyCommunityEntityInviteCreatedAsync(invitation);

            //return
            return id;
        }

        public CommunityEntityInvitation Get(string invitationId)
        {
            var id = ObjectId.Parse(invitationId);
            return _communityEntityInvitationRepository.Get(i => i.Id == id && i.Status == InvitationStatus.Pending && !i.Deleted);
        }

        public CommunityEntityInvitation GetForSourceAndTarget(string sourceEntityId, string targetEntityId)
        {
            return _communityEntityInvitationRepository.Get(i => i.SourceCommunityEntityId == sourceEntityId && i.TargetCommunityEntityId == targetEntityId && i.Status == InvitationStatus.Pending && !i.Deleted);
        }

        public List<CommunityEntityInvitation> ListForTarget(string currentUserId, string entityId, string search, int page, int pageSize)
        {
            var invitations = _communityEntityInvitationRepository.List(i => i.TargetCommunityEntityId == entityId && i.Status == InvitationStatus.Pending && !i.Deleted);
            EnrichCommunityEntityInvitationDataSource(invitations);

            var invitationPage = invitations
                .Where(i => (string.IsNullOrEmpty(search) || i.SourceCommunityEntity.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) || i.SourceCommunityEntity.ShortTitle.Contains(search, StringComparison.CurrentCultureIgnoreCase) || i.SourceCommunityEntity.ShortDescription.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                       && !i.Deleted)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return invitationPage;
        }

        public List<CommunityEntityInvitation> ListForSource(string currentUserId, string entityId, string search, int page, int pageSize)
        {
            var invitations = _communityEntityInvitationRepository.List(i => i.SourceCommunityEntityId == entityId && i.Status == InvitationStatus.Pending && !i.Deleted);
            EnrichCommunityEntityInvitationDataTarget(invitations);

            var invitationPage = invitations
                .Where(i => (string.IsNullOrEmpty(search) || i.TargetCommunityEntity.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) || i.TargetCommunityEntity.ShortTitle.Contains(search, StringComparison.CurrentCultureIgnoreCase) || i.TargetCommunityEntity.ShortDescription.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                       && !i.Deleted)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return invitationPage;
        }

        public async Task AcceptAsync(CommunityEntityInvitation invitation)
        {
            //update invitation status
            invitation.Status = InvitationStatus.Accepted;
            await _communityEntityInvitationRepository.UpdateAsync(invitation);

            //initialize entity
            var source = GetSource(invitation);
            var target = GetTarget(invitation);

            //create entity
            var relation = new Relation
            {
                SourceCommunityEntityType = source.Item1,
                SourceCommunityEntityId = source.Item2,
                TargetCommunityEntityType = target.Item1,
                TargetCommunityEntityId = target.Item2,
                CreatedByUserId = invitation.CreatedByUserId,
                CreatedUTC = invitation.CreatedUTC,
            };

            await _relationRepository.CreateAsync(relation);

            //process notifications
            await _notificationService.NotifyCommunityEntityJoinedAsync(relation);
            await _notificationService.NotifyCommunityEntityInviteCreatedWithdrawAsync(invitation);
        }

        private Tuple<CommunityEntityType, string> GetSource(CommunityEntityInvitation invitation)
        {
            if (invitation.TargetCommunityEntityType < invitation.SourceCommunityEntityType)
                return new Tuple<CommunityEntityType, string>(invitation.TargetCommunityEntityType, invitation.TargetCommunityEntityId);

            return new Tuple<CommunityEntityType, string>(invitation.SourceCommunityEntityType, invitation.SourceCommunityEntityId);
        }

        private Tuple<CommunityEntityType, string> GetTarget(CommunityEntityInvitation invitation)
        {
            if (invitation.TargetCommunityEntityType < invitation.SourceCommunityEntityType)
                return new Tuple<CommunityEntityType, string>(invitation.SourceCommunityEntityType, invitation.SourceCommunityEntityId);

            return new Tuple<CommunityEntityType, string>(invitation.TargetCommunityEntityType, invitation.TargetCommunityEntityId);
        }

        public async Task RejectAsync(CommunityEntityInvitation invitation)
        {
            invitation.Status = InvitationStatus.Rejected;
            await _communityEntityInvitationRepository.UpdateAsync(invitation);

            //process notifications
            await _notificationService.NotifyCommunityEntityInviteCreatedWithdrawAsync(invitation);
        }

        //private CommunityEntity Get(string id, CommunityEntityType type)
        //{
        //    switch (type)
        //    {

        //        case CommunityEntityType.Project:
        //            return _projectRepository.Get(id);
        //        case CommunityEntityType.Workspace:
        //            return _workspaceRepository.Get(id);
        //        case CommunityEntityType.Node:
        //            return _nodeRepository.Get(id);
        //        default:
        //            throw new Exception($"Cannot get entity; unknown type {type}");
        //    }
        //}

        private List<CommunityEntity> Get(Dictionary<string, CommunityEntityType> entityDictionary)
        {
            var communityIds = entityDictionary.Keys.Where(k => entityDictionary[k] == CommunityEntityType.Workspace).ToList();
            var nodeIds = entityDictionary.Keys.Where(k => entityDictionary[k] == CommunityEntityType.Node).ToList();
            var organizationIds = entityDictionary.Keys.Where(k => entityDictionary[k] == CommunityEntityType.Organization).ToList();

            var res = new List<CommunityEntity>();
            res.AddRange(_workspaceRepository.Get(communityIds));
            res.AddRange(_nodeRepository.Get(nodeIds));
            res.AddRange(_organizationRepository.Get(organizationIds));

            return res;
        }

        protected void EnrichCommunityEntityInvitationDataSource(List<CommunityEntityInvitation> communityEntityInvitations)
        {
            var invitationSourceDictionary = communityEntityInvitations.ToDictionary(i => i.SourceCommunityEntityId, i => i.SourceCommunityEntityType);
            var communityEntities = Get(invitationSourceDictionary);

            foreach (var invitation in communityEntityInvitations)
            {
                invitation.SourceCommunityEntity = communityEntities.SingleOrDefault(e => e.Id.ToString() == invitation.SourceCommunityEntityId);
            }
        }

        protected void EnrichCommunityEntityInvitationDataTarget(List<CommunityEntityInvitation> communityEntityInvitations)
        {
            var invitationTargetDictionary = communityEntityInvitations.ToDictionary(i => i.TargetCommunityEntityId, i => i.TargetCommunityEntityType);
            var communityEntities = Get(invitationTargetDictionary);

            foreach (var invitation in communityEntityInvitations)
            {
                invitation.TargetCommunityEntity = communityEntities.SingleOrDefault(e => e.Id.ToString() == invitation.TargetCommunityEntityId);
            }
        }
    }
}