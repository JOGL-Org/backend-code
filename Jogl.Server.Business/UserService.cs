using Jogl.Server.Auth;
using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.URL;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Jogl.Server.Business
{
    public class UserService : BaseService, IUserService
    {
        private readonly IUserVerificationCodeRepository _verificationCodeRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IWaitlistRecordRepository _waitlistRecordRepository;
        private readonly IPushNotificationTokenRepository _pushNotificationTokenRepository;
        private readonly IUserConnectionRepository _userConnectionRepository;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IUrlService _urlService;

        public UserService(IUserVerificationCodeRepository verificationCodeRepository, IAuthService authService, ISkillRepository skillRepository, IWaitlistRecordRepository waitlistRecordRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IUserConnectionRepository userConnectionRepository, ICommunityEntityService communityEntityService, INotificationService notificationService, IEmailService emailService, IUrlService urlService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _verificationCodeRepository = verificationCodeRepository;
            _authService = authService;
            _skillRepository = skillRepository;
            _waitlistRecordRepository = waitlistRecordRepository;
            _pushNotificationTokenRepository = pushNotificationTokenRepository;
            _userConnectionRepository = userConnectionRepository;
            _communityEntityService = communityEntityService;
            _notificationService = notificationService;
            _emailService = emailService;
            _urlService = urlService;
        }

        public async Task<string> CreateAsync(User user, string password = "")
        {
            var feed = new Feed()
            {
                CreatedUTC = user.CreatedUTC,
                CreatedByUserId = user.CreatedByUserId,
                Type = FeedType.User
            };

            var id = await _feedRepository.CreateAsync(feed);
            user.Id = ObjectId.Parse(id);

            if (!string.IsNullOrEmpty(password))
            {
                byte[]? salt;
                var hash = _authService.HashPasword(password, out salt);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            user.Onboarding = true;
            user.ContactMe = true;
            user.NotificationSettings = new UserNotificationSettings
            {
                ContainerInvitationEmail = true,
                ContainerInvitationJogl = true,
                DocumentMemberContainerEmail = true,
                DocumentMemberContainerJogl = true,
                EventInvitationJogl = true,
                EventMemberContainerEmail = true,
                EventMemberContainerJogl = true,
                MentionEmail = true,
                MentionJogl = true,
                NeedMemberContainerEmail = true,
                NeedMemberContainerJogl = true,
                PaperMemberContainerEmail = true,
                PaperMemberContainerJogl = true,
                PostAttendingEventEmail = true,
                PostAttendingEventJogl = true,
                PostAuthoredEventEmail = true,
                PostAuthoredEventJogl = true,
                PostAuthoredObjectEmail = true,
                PostAuthoredObjectJogl = true,
                PostMemberContainerEmail = true,
                PostMemberContainerJogl = true,
                ThreadActivityEmail = true,
                ThreadActivityJogl = true,
            };

            if (string.IsNullOrEmpty(user.Username))
                user.Username = GetUniqueUsername(user.FirstName, user.LastName);

            await _userRepository.CreateAsync(user);

            //check for pending invitations
            var invites = _invitationRepository.List(i => i.InviteeEmail == user.Email
                                                          && i.Status == InvitationStatus.Pending
                                                          && !i.Deleted);

            foreach (var invitation in invites)
            {
                //if they exist, assign the invite to them
                invitation.InviteeEmail = null;
                invitation.InviteeUserId = id;
                invitation.UpdatedUTC = user.CreatedUTC;
                invitation.UpdatedByUserId = id;
                await _invitationRepository.UpdateAsync(invitation);

                var inviterUser = _userRepository.Get(invitation.CreatedByUserId);

                //raise a notification for them
                await _notificationService.NotifyInviteCreatedAsync(invitation, inviterUser);
            }

            //check for pending event invitations
            var eventInvites = _eventAttendanceRepository.List(ea => ea.UserEmail == user.Email && !ea.Deleted);
            foreach (var eventInvitation in eventInvites)
            {
                //if they exist, assign the invite to them
                eventInvitation.UserEmail = null;
                eventInvitation.UserId = id;
                eventInvitation.UpdatedUTC = user.CreatedUTC;
                eventInvitation.UpdatedByUserId = id;
                await _eventAttendanceRepository.UpdateUserAsync(eventInvitation);

                var inviterUser = _userRepository.Get(eventInvitation.CreatedByUserId);
                var ev = _eventRepository.Get(eventInvitation.EventId);
                var communityEntity = _communityEntityService.Get(ev.CommunityEntityId);

                //raise a notification for them
                await _notificationService.NotifyEventInviteCreatedAsync(ev, communityEntity, user, new[] { eventInvitation });
            }

            return id;
        }

        public User Get(string userId)
        {
            return _userRepository.Get(userId);
        }

        public List<User> List()
        {
            return _userRepository.Query().ToList();
        }

        public User GetDetail(string userId, string currentUserId)
        {
            var user = _userRepository.Get(userId);
            if (user == null)
                return null;

            EnrichUserData(new User[] { user }, currentUserId);
            EnrichUserDataWithConnectionStatus(new User[] { user }, currentUserId);
            return user;
        }

        public User GetForEmail(string email, bool includeDeleted = false)
        {
            return _userRepository.Get(u => u.Email == email, includeDeleted);
        }

        public User GetForWallet(string wallet, bool includeDeleted = false)
        {
            return _userRepository.Get(u => u.Wallets.Any(w => w.Address == wallet), includeDeleted);
        }

        public User GetForUsername(string username, bool includeDeleted = false)
        {
            return _userRepository.Get(u => u.Username == username, includeDeleted);
        }

        public ListPage<User> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var users = _userRepository
                .Query(search)
                .Filter(u => u.Status == UserStatus.Verified)
                .Sort(sortKey, sortAscending)
                .ToList();
            var total = users.Count;

            var userPage = GetPage(users, page, pageSize);
            EnrichUserData(userPage, userId);

            return new ListPage<User>(userPage, total);
        }

        public long Count(string currentUserId, string search)
        {
            return _userRepository
                 .Query(search)
                 .Filter(u => u.Status == UserStatus.Verified)
                 .Count();
        }

        public List<User> ListForEntity(string userId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var memberships = _membershipRepository.Query(m => m.CommunityEntityId == entityId).ToList();
            var userIds = memberships.Select(m => m.UserId).Distinct().ToList();
            var users = _userRepository
                .QueryWithMembershipData(search, new List<string> { entityId })
                .Filter(u => userIds.Contains(u.Id.ToString()))
                .Filter(u => u.Status == UserStatus.Verified)
                .Sort(sortKey, sortAscending)
                .ToList();

            var userPage = GetPage(users, page, pageSize);
            EnrichUserData(userPage, userId);
            //EnrichUserDataWithCommonSpaces(users, nodeId, userId);

            return userPage;
        }

        public ListPage<User> ListForNode(string userId, string nodeId, List<string> communityEntityIds, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var memberships = _membershipRepository.List(m => entityIds.Contains(m.CommunityEntityId) && !m.Deleted);
            var userIds = memberships.Select(m => m.UserId).Distinct().ToList();
            var users = _userRepository
                .QueryWithMembershipData(search, entityIds)
                .Filter(u => userIds.Contains(u.Id.ToString()))
                .Filter(u => u.Status == UserStatus.Verified)
                .Sort(sortKey, sortAscending)
                .ToList();

            //var docs = _documentRepository.Query(search)
            //    .Filter(d => userIds.Contains(d.FeedId))
            //    .ToList();

            //var documentUserIds = docs.Select(u => u.CreatedByUserId).Distinct().ToList();
            //var documentUsers = _userRepository.Get(documentUserIds);

            //var papers = _paperRepository.Query(search)
            //    .Filter(p => userIds.Contains(p.FeedId))
            //    .ToList();

            //var paperUserIds = papers.Select(p => p.CreatedByUserId).Distinct().ToList();
            //var paperUsers = _userRepository.Get(paperUserIds);

            //var allUsers = users.Concat(documentUsers).Concat(paperUsers).DistinctBy(u => u.Id).ToList();

            var userPage = GetPage(/*allUsers*/users, page, pageSize);
            EnrichUserData(userPage, userId);
            EnrichUserDataWithCommonSpaces(users, nodeId, userId);

            return new ListPage<User>(userPage, users.Count);
        }

        public long CountForNode(string userId, string nodeId, string search)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);

            var memberships = _membershipRepository.List(m => entityIds.Contains(m.CommunityEntityId) && !m.Deleted);
            var userIds = memberships.Select(m => m.UserId).Distinct().ToList();

            return _userRepository
               .Query(search)
               .Filter(u => userIds.Contains(u.Id.ToString()))
               .Count();
        }

        public List<User> ListEcosystem(string currentUserId, string entityId, string search, int page, int pageSize)
        {
            var relations = _relationRepository.List(r => (r.SourceCommunityEntityId == entityId || r.TargetCommunityEntityId == entityId) && r.TargetCommunityEntityType != CommunityEntityType.Organization && !r.Deleted);
            var sourceIds = relations.Select(r => r.SourceCommunityEntityId).ToList();
            var targetIds = relations.Select(r => r.TargetCommunityEntityId).ToList();
            var communityEntityIds = sourceIds.Concat(targetIds).Concat(new List<string> { entityId }).Distinct().ToList();

            var memberships = _membershipRepository.List(m => communityEntityIds.Contains(m.CommunityEntityId) && !m.Deleted);
            var userIds = memberships.Select(m => m.UserId).Except(new string[] { currentUserId }).Distinct().ToList();
            var users = _userRepository
                 .Query(search)
                 .Filter(u => userIds.Contains(u.Id.ToString()))
                 .Filter(u => u.Status == UserStatus.Verified)
                 .ToList();

            var userPage = GetPage(users, page, pageSize);

            EnrichUserData(userPage, currentUserId);
            return userPage;
        }

        public List<User> AutocompleteForEntity(string entityId, string search, int page, int pageSize)
        {
            var feed = _feedRepository.Get(entityId);
            if (feed == null)
                return new List<User>();

            switch (feed.Type)
            {
                case FeedType.Project:
                case FeedType.Workspace:
                case FeedType.Node:
                case FeedType.Organization:
                case FeedType.CallForProposal:
                case FeedType.Channel:
                    var memberships = _membershipRepository.List(m => m.CommunityEntityId == entityId && !m.Deleted);
                    var communityEntityUserIds = memberships.Select(m => m.UserId).ToList();
                    return _userRepository.QueryAutocomplete(search)
                        .Filter(u => communityEntityUserIds.Contains(u.Id.ToString()))
                        .ToList();
                case FeedType.Event:
                    var eventAttendances = _eventAttendanceRepository.List(ea => ea.EventId == entityId && !string.IsNullOrEmpty(ea.UserId) && !ea.Deleted);
                    var eventUserIds = eventAttendances.Select(ea => ea.UserId).ToList();
                    return _userRepository.QueryAutocomplete(search)
                        .Filter(u => eventUserIds.Contains(u.Id.ToString()))
                        .ToList();
                case FeedType.User:
                    return new List<User>() { _userRepository.Get(entityId) };
                case FeedType.Need:
                    var need = _needRepository.Get(entityId);
                    if (need == null)
                        return new List<User>();

                    return AutocompleteForEntity(need.FeedEntityId, search, page, pageSize);
                case FeedType.Document:
                    var doc = _documentRepository.Get(entityId);
                    if (doc == null)
                        return new List<User>();

                    return AutocompleteForEntity(doc.FeedEntityId, search, page, pageSize);
                case FeedType.Paper:
                    var pap = _paperRepository.Get(entityId);
                    if (pap == null)
                        return new List<User>();

                    return AutocompleteForEntity(pap.FeedEntityId, search, page, pageSize);
                default:
                    throw new Exception($"Cannot return users for feed type {feed.Type}");
            }
        }

        public List<User> Autocomplete(string search, int page, int pageSize)
        {
            return _userRepository
                  .QueryAutocomplete(search)
                  .Filter(u => u.Status == UserStatus.Verified)
                  .Page(page, pageSize)
                  .ToList();
        }

        public async Task UpdateAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task UpdateOnboardingStatusAsync(User user)
        {
            user.Onboarding = true;
            await _userRepository.SetOnboardingStatusAsync(user);
        }

        public async Task SetActiveAsync(User user)
        {
            await _userRepository.SetStatusAsync(user.Id.ToString(), UserStatus.Verified);
        }

        public async Task SetArchivedAsync(User user)
        {
            await _userRepository.SetStatusAsync(user.Id.ToString(), UserStatus.Archived);
        }

        public async Task DeleteAsync(User user)
        {
            var id = user.Id.ToString();

            //delete feed
            await DeleteFeedAsync(id);

            await _membershipRepository.DeleteAsync(m => m.UserId == id && !m.Deleted);
            await _invitationRepository.DeleteAsync(i => i.InviteeUserId == id && !i.Deleted);
            await _pushNotificationTokenRepository.DeleteAsync(t => t.UserId == id && !t.Deleted);
            await _userFeedRecordRepository.DeleteAsync(ufr => ufr.UserId == id && !ufr.Deleted);
            await _userContentEntityRecordRepository.DeleteAsync(ucer => ucer.UserId == id && !ucer.Deleted);
            await _verificationCodeRepository.DeleteAsync(vc => vc.UserId == id && !vc.Deleted);
            await _mentionRepository.DeleteAsync(m => m.EntityId == id && !m.Deleted);
            await _followingRepository.DeleteAsync(f => f.UserIdFrom == id && !f.Deleted);
            await _followingRepository.DeleteAsync(f => f.UserIdTo == id && !f.Deleted);
            await _userRepository.DeleteAsync(id);
        }

        public async Task UndeleteAsync(User user)
        {
            var id = user.Id.ToString();

            //delete feed
            await UndeleteFeedAsync(id);

            await _membershipRepository.UndeleteAsync(m => m.UserId == id && !m.Deleted);
            await _invitationRepository.UndeleteAsync(i => i.InviteeUserId == id && !i.Deleted);
            await _pushNotificationTokenRepository.UndeleteAsync(t => t.UserId == id && !t.Deleted);
            await _userFeedRecordRepository.UndeleteAsync(ufr => ufr.UserId == id && !ufr.Deleted);
            await _userContentEntityRecordRepository.UndeleteAsync(ucer => ucer.UserId == id && !ucer.Deleted);
            await _verificationCodeRepository.UndeleteAsync(vc => vc.UserId == id && !vc.Deleted);
            await _mentionRepository.UndeleteAsync(m => m.EntityId == id && !m.Deleted);
            await _followingRepository.UndeleteAsync(f => f.UserIdFrom == id && !f.Deleted);
            await _followingRepository.UndeleteAsync(f => f.UserIdTo == id && !f.Deleted);
            await _userRepository.UndeleteAsync(id);
        }

        public async Task ResetPasswordAsync(string email, string url)
        {
            var code = GenerateCode();
            var user = _userRepository.Get(u => u.Email == email);
            if (user == null)
                return;

            await _emailService.SendEmailAsync(email, EmailTemplate.PasswordReset, new
            {
                first_name = user.FirstName,
                url = url + $"?email={email}&token={code}",
                LANGUAGE = user.Language
            });
            await _verificationCodeRepository.CreateAsync(new UserVerificationCode
            {
                Action = VerificationAction.PasswordReset,
                Code = code,
                CreatedByUserId = user.Id.ToString(),
                CreatedUTC = DateTime.UtcNow,
                UserId = user.Id.ToString(),
                ValidUntilUTC = DateTime.UtcNow.AddHours(1)
            });
        }

        public async Task<bool> ResetPasswordConfirmAsync(string email, string code, string newPassword)
        {
            var user = _userRepository.GetForEmail(email);
            if (user == null)
                return false;

            var verificationCode = _verificationCodeRepository.GetForCode(code);
            if (verificationCode == null)
                return false;

            if (verificationCode.UserId != user.Id.ToString())
                return false;

            if (verificationCode.Action != VerificationAction.PasswordReset)
                return false;

            if (verificationCode.ValidUntilUTC < DateTime.UtcNow)
                return false;

            await SetPasswordAsync(user.Id.ToString(), newPassword);
            await _verificationCodeRepository.DeleteAsync(verificationCode.Id.ToString());
            return true;
        }

        public async Task StartOneTimeLoginAsync(string email)
        {
            var code = await GetOnetimeLoginCodeAsync(email);
            var redirectUrl = _urlService.GetOneTimeLoginLink(email);

            await _emailService.SendEmailAsync(email, EmailTemplate.Login, new
            {
                code = code,
                url = redirectUrl
            });
        }

        public async Task<string> GetOnetimeLoginCodeAsync(string email)
        {
            var code = GenerateCode(6, true);

            await _verificationCodeRepository.CreateAsync(new UserVerificationCode
            {
                Action = VerificationAction.OneTimeLogin,
                Code = code,
                CreatedUTC = DateTime.UtcNow,
                UserEmail = email,
                ValidUntilUTC = DateTime.UtcNow.AddDays(7)
            });

            return code;
        }

        public async Task<bool> VerifyOneTimeLoginAsync(string email, string code)
        {
            var verificationCode = _verificationCodeRepository.GetForCode(code);
            if (verificationCode == null)
                return false;

            if (verificationCode.UserEmail != email)
                return false;

            if (verificationCode.Action != VerificationAction.OneTimeLogin)
                return false;

            if (verificationCode.ValidUntilUTC < DateTime.UtcNow)
                return false;

            await _verificationCodeRepository.DeleteAsync(verificationCode.Id.ToString());
            return true;
        }

        public async Task SetPasswordAsync(string userId, string password)
        {
            byte[]? salt;
            var hash = _authService.HashPasword(password, out salt);
            await _userRepository.SetPasswordAsync(userId, hash, salt);
        }

        private string GenerateCode(int size = 16, bool digitsOnly = false)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            if (digitsOnly)
                chars = "1234567890".ToCharArray();

            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }

        public async Task SendMessageAsync(string userIdFrom, string userIdTo, string appUrl, string subject, string text)
        {
            var userFrom = _userRepository.Get(userIdFrom);
            var userTo = _userRepository.Get(userIdTo);

            await _emailService.SendEmailAsync(userTo.Email, EmailTemplate.Message, new
            {
                to_first_name = userTo.FirstName,
                from_name = userFrom.FirstName + " " + userFrom.LastName,
                from_url = appUrl + $"user/{userIdFrom}",
                subject = subject,
                text = text,
                LANGUAGE = userTo.Language
            }, userFrom.Email);
        }

        public UserFollowing GetFollowing(string userIdFrom, string userIdTo)
        {
            return _followingRepository.GetFollowing(userIdFrom, userIdTo);
        }

        public async Task<string> CreateFollowingAsync(UserFollowing following)
        {
            //create following
            var id = await _followingRepository.CreateAsync(following);

            //process notifications
            await _notificationService.NotifyUserFollowedAsync(following);

            //return
            return id;
        }

        public async Task DeleteFollowingAsync(string followingId)
        {
            await _followingRepository.DeleteAsync(followingId);
        }

        public List<User> GetFollowed(string followerId, string currentUserId, string search, int page, int pageSize, bool loadDetails = false)
        {
            var userIds = _followingRepository.List(f => f.UserIdFrom == followerId && !f.Deleted).Select(f => f.UserIdTo).ToList();
            var users = _userRepository.SearchGet(userIds, search);
            var userPage = GetPage(users, page, pageSize);

            if (!loadDetails)
                return userPage;

            EnrichUserData(userPage, currentUserId);
            return userPage;
        }

        public List<User> GetFollowers(string followedId, string currentUserId, string search, int page, int pageSize, bool loadDetails = false)
        {
            var userIds = _followingRepository.List(f => f.UserIdTo == followedId && !f.Deleted).Select(f => f.UserIdFrom).ToList();
            var users = _userRepository.SearchGet(userIds, search);
            var userPage = GetPage(users, page, pageSize);

            if (!loadDetails)
                return userPage;

            EnrichUserData(userPage, currentUserId);
            return userPage;
        }

        public async Task CreateSkillAsync(TextValue skill)
        {
            await _skillRepository.CreateAsync(skill);
        }

        public List<TextValue> GetSkills(string search, int page, int pageSize)
        {
            return _skillRepository.List(s => string.IsNullOrEmpty(search) || s.Value.StartsWith(search), page, pageSize);
        }

        public TextValue GetSkill(string value)
        {
            return _skillRepository.Get(s => s.Value == value);
        }

        private string GetUniqueUsername(string firstName, string lastName)
        {
            var rootUsername = ToAlphaNum(firstName?.Trim() + lastName?.Trim());
            var users = _userRepository.List(u => u.Username.StartsWith(rootUsername));

            int counter = 1;
            var username = rootUsername;
            var clashingUser = users.FirstOrDefault(u => u.Username == username);
            while (clashingUser != null)
            {
                username = rootUsername + counter++;
                clashingUser = users.FirstOrDefault(u => u.Username == username);
            }

            return username;
        }

        public async Task CreateWaitlistRecordAsync(WaitlistRecord record)
        {
            await _waitlistRecordRepository.CreateAsync(record);
        }

        public async Task SendContactEmailAsync(UserContact contact)
        {
            await _emailService.SendEmailAsync("hello@jogl.io", EmailTemplate.ContactDemo, new
            {
                first_name = contact.FirstName,
                last_name = contact.LastName,
                country = contact.Country,
                organization = contact.Organization,
                organization_size = contact.OrganizationSize,
                phone = contact.Phone,
                email = contact.EmailAddress,
                message = contact.Message,
                reason = contact.Reason
            }, contact.EmailAddress);
        }

        public async Task UpsertPushNotificationTokenAsync(string token, string userId)
        {
            await _pushNotificationTokenRepository.UpsertTokenAsync(userId, token, DateTime.UtcNow);
        }

        public List<CommunityEntity> ListCommunityEntitiesForNodeUsers(string currentUserId, string nodeId, string search)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            var memberships = _membershipRepository.Query(m => m.UserId == currentUserId && entityIds.Contains(m.CommunityEntityId)).ToList();
            var currentUserEntityIds = memberships.Select(m => m.CommunityEntityId);

            return _communityEntityService.List(currentUserEntityIds);
        }

        private string ToAlphaNum(string username)
        {
            string[] accentPatterns =
            {
                "[\u00C0-\u00C6]", "[\u00E0-\u00E6]", // A, a
                "[\u00C8-\u00CB]", "[\u00E8-\u00EB]", // E, e
                "[\u00CC-\u00CF]", "[\u00EC-\u00EF]", // I, i
                "[\u00D2-\u00D8]", "[\u00F2-\u00F8]", // O, o
                "[\u00D9-\u00DC]", "[\u00F9-\u00FC]", // U, u
                "[\u00D1]", "[\u00F1]",             // N, n
                "[\u00C7]", "[\u00E7]"              // C, c
            };

            string[] noAccent = { "A", "a", "E", "e", "I", "i", "O", "o", "U", "u", "N", "n", "C", "c" };

            for (int i = 0; i < accentPatterns.Length; i++)
            {
                username = Regex.Replace(username, "[+=\\-*_/\\\\'\"`()[\\]{} &#|^@%µ$£¤§~:;,.?!<>]", "")
                            .Replace(accentPatterns[i], noAccent[i]);
            }

            return username;
        }

        protected void EnrichUserDataWithConnectionStatus(IEnumerable<User> users, string currentUserId)
        {
            var userIds = users.Select(u => u.Id.ToString());
            var userConnections = _userConnectionRepository.List(uc => (uc.FromUserId == currentUserId && userIds.Contains(uc.ToUserId)) || uc.ToUserId == currentUserId && userIds.Contains(uc.FromUserId));
            foreach (var user in users)
            {
                var userConnection = userConnections.SingleOrDefault(c => c.FromUserId == user.Id.ToString() || c.ToUserId == user.Id.ToJson());
                user.UserConnectionStatus = userConnection?.Status;
            }
        }
    }
}