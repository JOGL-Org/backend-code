using Jogl.Server.Data;
using Jogl.Server.DB;

namespace Jogl.Server.Business
{
    public class AccessService : BaseService, IAccessService
    {
        public AccessService(IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
        }

        public string GetUserAccessLevel(Membership membership, Invitation invitation = null)
        {
            if (membership == null)
            {
                if (invitation != null)
                    return "pending";
                else
                    return "visitor";
            }

            return membership.AccessLevel.ToString().ToLower();
        }

        public JoiningRestrictionLevel? GetUserJoiningRestrictionLevel(Membership membership, string currentUserId, CommunityEntity entity)
        {
            if (membership != null)
                return null;

            if (entity.JoiningRestrictionLevel != JoiningRestrictionLevel.Custom)
                return entity.JoiningRestrictionLevel;

            var memberships = _membershipRepository.List(m => m.UserId == currentUserId && !m.Deleted);
            var membershipSettings = entity.JoiningRestrictionLevelCustomSettings?.Where(s => memberships.Any(m => m.CommunityEntityId == s.CommunityEntityId)).ToList();
            if (membershipSettings?.Any() != true)
                return null;

            if (membershipSettings.Any(s => s.Level == JoiningRestrictionLevel.Open))
                return JoiningRestrictionLevel.Open;

            if (membershipSettings.Any(s => s.Level == JoiningRestrictionLevel.Request))
                return JoiningRestrictionLevel.Request;

            if (membershipSettings.Any(s => s.Level == JoiningRestrictionLevel.Invite))
                return JoiningRestrictionLevel.Invite;

            return null;
        }

        public bool IsContentEntityActionAllowed(CommunityEntity entity, Membership membership, ContentEntity contentEntity, Action action)
        {
            switch (action)
            {
                case Action.Contribute:
                    if (membership == null)
                        return false;

                    return membership.AccessLevel == AccessLevel.Owner
                        || membership.AccessLevel == AccessLevel.Admin;

                case Action.Delete:
                    if (membership == null)
                        return false;

                    if (membership.UserId == contentEntity.CreatedByUserId)
                        return true;

                    return (membership.AccessLevel == AccessLevel.Owner
                        || membership.AccessLevel == AccessLevel.Admin);
                default:
                    throw new Exception($"Cannot determine whether action {action} is allowed on content entity");
            }
        }
    }
}