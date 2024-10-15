using Jogl.Server.Data;
using Jogl.Server.DB;

namespace Jogl.Server.Business
{
    public class CommunityEntityMembershipService : BaseService, ICommunityEntityMembershipService
    {
        private readonly INotificationService _notificationService;

        public CommunityEntityMembershipService(INotificationService notificationService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _notificationService = notificationService;
        }

        public Relation GetForSourceAndTarget(string sourceEntityId, string targetEntityId)
        {
            return _relationRepository.Get(r => (r.SourceCommunityEntityId == sourceEntityId && r.TargetCommunityEntityId == targetEntityId)
            || (r.SourceCommunityEntityId == targetEntityId && r.TargetCommunityEntityId == sourceEntityId));
        }

        public async Task DeleteAsync(string id)
        {
            await _relationRepository.DeleteAsync(id);
        }

        public async Task<string> CreateAsync(Relation relation)
        {
            //initialize entity
            var source = GetSource(relation);
            var target = GetTarget(relation);

            //create entity
            relation.SourceCommunityEntityType = source.Item1;
            relation.SourceCommunityEntityId = source.Item2;
            relation.TargetCommunityEntityType = target.Item1;
            relation.TargetCommunityEntityId = target.Item2;

            var id = await _relationRepository.CreateAsync(relation);

            //process notifications
            await _notificationService.NotifyCommunityEntityJoinedAsync(relation);

            //return
            return id;
        }

        private Tuple<CommunityEntityType, string> GetSource(Relation relation)
        {
            if (relation.SourceCommunityEntityType == CommunityEntityType.CallForProposal && relation.TargetCommunityEntityType == CommunityEntityType.Project)
                return new Tuple<CommunityEntityType, string>(relation.TargetCommunityEntityType, relation.TargetCommunityEntityId);
            if (relation.SourceCommunityEntityType == CommunityEntityType.Workspace && relation.TargetCommunityEntityType == CommunityEntityType.CallForProposal)
                return new Tuple<CommunityEntityType, string>(relation.TargetCommunityEntityType, relation.TargetCommunityEntityId);

            if (relation.TargetCommunityEntityType < relation.SourceCommunityEntityType)
                return new Tuple<CommunityEntityType, string>(relation.TargetCommunityEntityType, relation.TargetCommunityEntityId);

            return new Tuple<CommunityEntityType, string>(relation.SourceCommunityEntityType, relation.SourceCommunityEntityId);
        }

        private Tuple<CommunityEntityType, string> GetTarget(Relation relation)
        {
            if (relation.SourceCommunityEntityType == CommunityEntityType.CallForProposal && relation.TargetCommunityEntityType == CommunityEntityType.Project)
                return new Tuple<CommunityEntityType, string>(relation.SourceCommunityEntityType, relation.SourceCommunityEntityId);
            if (relation.SourceCommunityEntityType == CommunityEntityType.Workspace && relation.TargetCommunityEntityType == CommunityEntityType.CallForProposal)
                return new Tuple<CommunityEntityType, string>(relation.SourceCommunityEntityType, relation.SourceCommunityEntityId);

            if (relation.TargetCommunityEntityType < relation.SourceCommunityEntityType)
                return new Tuple<CommunityEntityType, string>(relation.SourceCommunityEntityType, relation.SourceCommunityEntityId);

            return new Tuple<CommunityEntityType, string>(relation.TargetCommunityEntityType, relation.TargetCommunityEntityId);
        }
    }
}