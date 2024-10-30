using Jogl.Server.Auth;
using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using Jogl.Server.Email;
using MongoDB.Bson;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Jogl.Server.Business
{
    public class UserService : BaseService, IUserService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserVerificationCodeRepository _verificationCodeRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IWaitlistRecordRepository _waitlistRecordRepository;
        private readonly IPushNotificationTokenRepository _pushNotificationTokenRepository;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public UserService(IOrganizationRepository organizationRepository, IUserVerificationCodeRepository verificationCodeRepository, IAuthService authService, ISkillRepository skillRepository, IWaitlistRecordRepository waitlistRecordRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, ICommunityEntityService communityEntityService, INotificationService notificationService, IEmailService emailService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _organizationRepository = organizationRepository;
            _verificationCodeRepository = verificationCodeRepository;
            _authService = authService;
            _skillRepository = skillRepository;
            _waitlistRecordRepository = waitlistRecordRepository;
            _pushNotificationTokenRepository = pushNotificationTokenRepository;
            _communityEntityService = communityEntityService;
            _notificationService = notificationService;
            _emailService = emailService;
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
            user.FeedId = id;

            if (!string.IsNullOrEmpty(password))
            {
                byte[]? salt;
                var hash = _authService.HashPasword(password, out salt);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

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

        public User GetDetail(string userId, string currentUserId)
        {
            var user = _userRepository.Get(userId);
            if (user == null)
                return null;

            EnrichUserData(_organizationRepository, new User[] { user }, currentUserId);
            return user;
        }

        public User GetForEmail(string email)
        {
            return _userRepository.Get(u => u.Email == email);
        }

        public User GetForWallet(string wallet)
        {
            return _userRepository.Get(u => u.Wallets.Any(w => w.Address == wallet));
        }

        public User GetForUsername(string username)
        {
            return _userRepository.Get(u => u.Username == username);
        }

        public ListPage<User> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            // .Where(u => !u.Deleted && u.Status == UserStatus.Verified)
            var users = _userRepository.SearchSort(search, sortKey, sortAscending);
            var total = users.Count;

            var userPage = GetPage(users, page, pageSize);
            EnrichUserData(_organizationRepository, userPage, userId);

            return new ListPage<User>(userPage, total);
        }

        public long Count(string currentUserId, string search)
        {
            return _userRepository.SearchCount(search);
        }

        public ListPage<User> ListForNode(string userId, string nodeId, List<string> communityEntityIds, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var memberships = _membershipRepository.List(m => entityIds.Contains(m.CommunityEntityId) && !m.Deleted);
            var userIds = memberships.Select(m => m.UserId).Distinct().ToList();
            var users = _userRepository.SearchGetSort(userIds, sortKey, sortAscending, search);

            var userPage = GetPage(users, page, pageSize);
            EnrichUserData(_organizationRepository, userPage, userId);

            return new ListPage<User>(userPage, users.Count);
        }

        public long CountForNode(string userId, string nodeId, string search)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);

            var memberships = _membershipRepository.List(m => entityIds.Contains(m.CommunityEntityId) && !m.Deleted);
            var userIds = memberships.Select(m => m.UserId).Distinct().ToList();
            var users = _userRepository.SearchGet(userIds, search);

            return users.Count;
        }

        public List<User> ListEcosystem(string currentUserId, string entityId, string search, int page, int pageSize)
        {
            var relations = _relationRepository.List(r => (r.SourceCommunityEntityId == entityId || r.TargetCommunityEntityId == entityId) && r.TargetCommunityEntityType != CommunityEntityType.Organization && !r.Deleted);
            var sourceIds = relations.Select(r => r.SourceCommunityEntityId).ToList();
            var targetIds = relations.Select(r => r.TargetCommunityEntityId).ToList();
            var communityEntityIds = sourceIds.Concat(targetIds).Concat(new List<string> { entityId }).Distinct().ToList();

            var memberships = _membershipRepository.List(m => communityEntityIds.Contains(m.CommunityEntityId) && !m.Deleted);
            var userIds = memberships.Select(m => m.UserId).Except(new string[] { currentUserId }).Distinct().ToList();
            var users = _userRepository.SearchGet(userIds, search);
            var userPage = GetPage(users, page, pageSize);

            EnrichUserData(_organizationRepository, userPage, currentUserId);
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
                    return _userRepository.AutocompleteGet(communityEntityUserIds, search);
                case FeedType.Event:
                    var eventAttendances = _eventAttendanceRepository.List(ea => ea.EventId == entityId && !string.IsNullOrEmpty(ea.UserId) && !ea.Deleted);
                    var eventUserIds = eventAttendances.Select(ea => ea.UserId).ToList();
                    return _userRepository.AutocompleteGet(eventUserIds, search);
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
                    return _userRepository.Autocomplete(search);
                default:
                    throw new Exception($"Cannot return users for feed type {feed.Type}");
            }
        }

        public List<User> Autocomplete(string search, int page, int pageSize)
        {
            return _userRepository.Autocomplete(search, page, pageSize);
        }

        public async Task UpdateAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteAsync(string id)
        {
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

        public async Task ResetPasswordAsync(string email, string url)
        {
            var code = GenerateCode();
            var user = _userRepository.GetForEmail(email);
            if (user == null)
                return;

            await _emailService.SendEmailAsync(email, EmailTemplate.PasswordReset, new
            {
                first_name = user.FirstName,
                url = url + $"?email={email}&token={code}",
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

        public async Task OneTimeLoginAsync(string email, string url)
        {
            var code = GenerateCode();

            await _emailService.SendEmailAsync(email, EmailTemplate.OneTimeLogin, new
            {
                url = url + $"?email={email}&code={code}",
            });

            await _verificationCodeRepository.CreateAsync(new UserVerificationCode
            {
                Action = VerificationAction.OneTimeLogin,
                Code = code,
                CreatedUTC = DateTime.UtcNow,
                UserEmail = email,
                ValidUntilUTC = DateTime.UtcNow.AddHours(1)
            });
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

        private string GenerateCode(int size = 16)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
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

            EnrichUserData(_organizationRepository, userPage, currentUserId);
            return userPage;
        }

        public List<User> GetFollowers(string followedId, string currentUserId, string search, int page, int pageSize, bool loadDetails = false)
        {
            var userIds = _followingRepository.List(f => f.UserIdTo == followedId && !f.Deleted).Select(f => f.UserIdFrom).ToList();
            var users = _userRepository.SearchGet(userIds, search);
            var userPage = GetPage(users, page, pageSize);

            if (!loadDetails)
                return userPage;

            EnrichUserData(_organizationRepository, userPage, currentUserId);
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

        public string GetUniqueUsername(string firstName, string lastName)
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

        public List<CommunityEntity> ListCommunityEntitiesForNodeUsers(string nodeId, string search, int page, int pageSize)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);

            var memberships = _membershipRepository.List(m => entityIds.Contains(m.CommunityEntityId) && !m.Deleted);

            var communityEntityIds = memberships.Select(m => m.CommunityEntityId).Distinct().ToList();
            return _communityEntityService.List(communityEntityIds);
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
    }
}