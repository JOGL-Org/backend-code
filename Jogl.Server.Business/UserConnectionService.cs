using Jogl.Server.Data;
using Jogl.Server.DB;
using MongoDB.Driver.Linq;

namespace Jogl.Server.Business
{
    public class UserConnectionService : BaseService, IUserConnectionService
    {
        private readonly IUserConnectionRepository _userConnectionRepository;
        public UserConnectionService(IUserConnectionRepository userConnectionRepository, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _userConnectionRepository = userConnectionRepository;
        }

        public async Task AcceptInvitationAsync(UserConnection connection)
        {
            connection.Status = UserConnectionStatus.Accepted;
            await _userConnectionRepository.UpdateAsync(connection);
        }

        public async Task<string> InviteAsync(UserConnection connection)
        {
            return await _userConnectionRepository.CreateAsync(connection);
        }

        public List<User> ListConnectedUsers(string userId)
        {
            var userConnections = _userConnectionRepository
                .Query(u => u.Status == UserConnectionStatus.Accepted)
                .ToList();

            var userIds = userConnections.Select(uc => uc.FromUserId).Concat(userConnections.Select(uc => uc.ToUserId))
                .Where(id => userId != id)
                .Distinct()
                .ToList();

            return _userRepository.Get(userIds);
        }

        public UserConnection Get(string userId1, string userId2)
        {
            return _userConnectionRepository.Get(uc => (uc.FromUserId == userId1 && uc.ToUserId == userId2) || (uc.FromUserId == userId2 && uc.ToUserId == userId1));
        }

        public async Task RejectInvitationAsync(UserConnection connection)
        {
            connection.Status = UserConnectionStatus.Rejected;
            await _userConnectionRepository.UpdateAsync(connection);
        }
    }
}