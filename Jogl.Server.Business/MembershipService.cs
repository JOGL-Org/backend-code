using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Jogl.Server.Business
{
    public class MembershipService : BaseService, IMembershipService
    {
        private readonly IOnboardingQuestionnaireInstanceRepository _onboardingQuestionnaireInstanceRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly INotificationService _notificationService;

        public MembershipService(IOnboardingQuestionnaireInstanceRepository onboardingQuestionnaireInstanceRepository, IOrganizationRepository organizationRepository, INotificationService notificationService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _onboardingQuestionnaireInstanceRepository = onboardingQuestionnaireInstanceRepository;
            _organizationRepository = organizationRepository;
            _notificationService = notificationService;
        }

        public async Task<string> CreateAsync(Membership membership)
        {
            //create membership
            var id = await _membershipRepository.CreateAsync(membership);

            //create user feed record
            await _userFeedRecordRepository.SetFeedReadAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);

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

            //return
            return id;
        }

        public Membership Get(string entityId, string userId)
        {
            return _membershipRepository.Get(entityId, userId);
        }

        public List<Membership> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, bool loadDetails = false)
        {
            var members = _membershipRepository.List(m => m.CommunityEntityId == entityId && !m.Deleted);
            var memberUserIds = members.Select(m => m.UserId).ToList();
            var users = _userRepository.Get(memberUserIds);

            foreach (var member in members)
            {
                member.User = users.SingleOrDefault(u => u.Id == ObjectId.Parse(member.UserId));
            }

            var memberPage = members
                .Where(m => m.User != null)
                .Where(m => (string.IsNullOrEmpty(search) || (m.User != null && (m.User.FirstName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || m.User.LastName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || m.User.Username.Contains(search, StringComparison.CurrentCultureIgnoreCase))))
                       && !m.Deleted)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!loadDetails)
                return memberPage;

            EnrichUserData(_organizationRepository, memberPage.Select(m => m.User).ToList(), currentUserId);
            return memberPage;
        }

        public List<Membership> ListForEntities(string currentUserId, List<string> entityIds)
        {
            var members = _membershipRepository.List(m => entityIds.Contains(m.CommunityEntityId) && !m.Deleted);
            return members;
        }

        public int CountForEntity(string currentUserId, string entityId, string search)
        {
            var members = _membershipRepository.List(m => m.CommunityEntityId == entityId && !m.Deleted);
            var memberUserIds = members.Select(m => m.UserId).ToList();
            var users = _userRepository.Get(memberUserIds);

            foreach (var member in members)
            {
                member.User = users.Single(u => u.Id == ObjectId.Parse(member.UserId));
            }

            return members.Count(m => string.IsNullOrEmpty(search) || (m.User != null && (m.User.FirstName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || m.User.LastName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || m.User.Username.Contains(search, StringComparison.CurrentCultureIgnoreCase)) && !m.Deleted));
        }

        public async Task UpdateAsync(Membership membership)
        {
            var existingMembership = _membershipRepository.Get(membership.Id.ToString());
            await _membershipRepository.UpdateAsync(membership);

            //process notifications
            if (existingMembership.AccessLevel != membership.AccessLevel)
                await _notificationService.NotifyAccessLevelChangedAsync(membership);
        }

        [Obsolete]
        public async Task DeleteAsync(string id)
        {
            var membership = _membershipRepository.Get(id);
            var answers = _onboardingQuestionnaireInstanceRepository.Get(i => i.UserId == membership.UserId && i.CommunityEntityId == membership.CommunityEntityId && !i.Deleted);
            if (answers != null)
                await _onboardingQuestionnaireInstanceRepository.DeleteAsync(answers.Id.ToString());

            //leave all channels
            foreach (var channel in _channelRepository.List(c => c.CommunityEntityId == membership.CommunityEntityId && !c.Deleted))
            {
                await _membershipRepository.DeleteAsync(m => m.UserId == membership.UserId && m.CommunityEntityId == channel.Id.ToString() && !m.Deleted);
            }

            await _membershipRepository.DeleteAsync(id);
        }

        public async Task DeleteAsync(Membership membership)
        {
            var answers = _onboardingQuestionnaireInstanceRepository.Get(i => i.UserId == membership.UserId && i.CommunityEntityId == membership.CommunityEntityId && !i.Deleted);
            if (answers != null)
                await _onboardingQuestionnaireInstanceRepository.DeleteAsync(answers.Id.ToString());

            //leave all channels
            foreach (var channel in _channelRepository.List(c => c.CommunityEntityId == membership.CommunityEntityId && !c.Deleted))
            {
                await _membershipRepository.DeleteAsync(m => m.UserId == membership.UserId && m.CommunityEntityId == channel.Id.ToString() && !m.Deleted);
            }

            await _membershipRepository.DeleteAsync(membership.Id.ToString());
        }

        public async Task AddMembersAsync(List<Membership> memberships, bool allowAddingOwners = false)
        {
            if (!memberships.Any())
                return;

            var communityEntityId = memberships.Select(m => m.CommunityEntityId).Distinct().Single();

            var existingMemberships = _membershipRepository.List(m => m.CommunityEntityId == communityEntityId && !m.Deleted);
            var newMemberships = memberships.Where(m => !existingMemberships.Any(em => em.UserId == m.UserId) && (allowAddingOwners || m.AccessLevel != AccessLevel.Owner)).ToList();

            if (newMemberships.Any())
                await _membershipRepository.CreateAsync(newMemberships);
        }

        public async Task UpdateMembersAsync(List<Membership> memberships, bool allowAddingOwners = false)
        {
            if (!memberships.Any())
                return;

            var communityEntityId = memberships.Select(m => m.CommunityEntityId).Distinct().Single();

            var existingMemberships = _membershipRepository.List(m => m.CommunityEntityId == communityEntityId && !m.Deleted);
            var updatedMemberships = memberships.Where(m => existingMemberships.Any(em => em.UserId == m.UserId && m.AccessLevel != em.AccessLevel && em.AccessLevel != AccessLevel.Owner) && (allowAddingOwners || m.AccessLevel != AccessLevel.Owner)).ToList();

            if (updatedMemberships.Any())
            {
                foreach (var membership in updatedMemberships)
                {
                    var existingMembership = existingMemberships.Single(m => m.UserId == membership.UserId);
                    membership.Id = existingMembership.Id;
                }

                await _membershipRepository.UpdateAsync(updatedMemberships);
            }

            return;
        }

        public async Task SetMembersAsync(List<Membership> memberships, string communityEntityId, bool allowAddingOwners = false)
        {
            var existingMemberships = _membershipRepository.List(m => m.CommunityEntityId == communityEntityId && !m.Deleted);
            var newMemberships = memberships.Where(m => !existingMemberships.Any(em => em.UserId == m.UserId) && (allowAddingOwners || m.AccessLevel != AccessLevel.Owner)).ToList();
            var updatedMemberships = memberships.Where(m => existingMemberships.Any(em => em.UserId == m.UserId && m.AccessLevel != em.AccessLevel && em.AccessLevel != AccessLevel.Owner) && (allowAddingOwners || m.AccessLevel != AccessLevel.Owner)).ToList();
            var deletedMemberships = existingMemberships.Where(em => !memberships.Any(m => m.UserId == em.UserId && em.AccessLevel != AccessLevel.Owner)).ToList();

            if (newMemberships.Any())
                await _membershipRepository.CreateAsync(newMemberships);

            if (deletedMemberships.Any())
                await _membershipRepository.DeleteAsync(deletedMemberships);

            if (updatedMemberships.Any())
            {
                foreach (var membership in updatedMemberships)
                {
                    var existingMembership = existingMemberships.Single(m => m.UserId == membership.UserId);
                    membership.Id = existingMembership.Id;
                }

                await _membershipRepository.UpdateAsync(updatedMemberships);
            }
        }

        public List<Membership> ListMembers(string entityId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var memberships = _membershipRepository.List(m => m.CommunityEntityId == entityId && !m.Deleted);
            var membershipUserIds = memberships.Select(m => m.UserId).ToList();
            var users = _userRepository.SearchGet(membershipUserIds, search);

            foreach (var membership in memberships)
            {
                membership.User = users.SingleOrDefault(u => u.Id.ToString() == membership.UserId);
            }

            return memberships.Where(u => u.User != null).ToList();
        }

        public List<Membership> ListMembers(string entityId)
        {
            var memberships = _membershipRepository.List(m => m.CommunityEntityId == entityId && !m.Deleted);
            var membershipUserIds = memberships.Select(m => m.UserId).ToList();
            var users = _userRepository.Get(membershipUserIds);

            foreach (var membership in memberships)
            {
                membership.User = users.SingleOrDefault(u => u.Id.ToString() == membership.UserId);
            }

            return memberships.Where(u => u.User != null).ToList();
        }

        public async Task RemoveMembersAsync(List<Membership> memberships)
        {
            if (!memberships.Any())
                return;

            var communityEntityId = memberships.Select(m => m.CommunityEntityId).Distinct().Single();

            var existingMemberships = _membershipRepository.List(m => m.CommunityEntityId == communityEntityId && !m.Deleted);
            var deletedMemberships = existingMemberships.Where(em => memberships.Any(m => m.UserId == em.UserId && em.AccessLevel != AccessLevel.Owner)).ToList();

            if (deletedMemberships.Any())
                await _membershipRepository.DeleteAsync(deletedMemberships);
        }

        public OnboardingQuestionnaireInstance GetOnboardingInstance(string entityId, string userId)
        {
            return _onboardingQuestionnaireInstanceRepository.Get(i => i.UserId == userId && i.CommunityEntityId == entityId);
        }

        public async Task<string> UpsertOnboardingInstanceAsync(OnboardingQuestionnaireInstance instance)
        {
            //TODO remove race condition
            var existingInstance = GetOnboardingInstance(instance.CommunityEntityId, instance.UserId);
            if (existingInstance != null)
            {
                existingInstance.Items = instance.Items;
                await _onboardingQuestionnaireInstanceRepository.UpdateAsync(existingInstance);
                return existingInstance.Id.ToString();
            }
            else
            {
                return await _onboardingQuestionnaireInstanceRepository.CreateAsync(instance);
            }
        }
    }
}