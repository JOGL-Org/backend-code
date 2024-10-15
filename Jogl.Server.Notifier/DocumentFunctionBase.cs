using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier
{
    public abstract class DocumentFunctionBase : CreatedFunctionBase<Document>
    {
        protected readonly ICommunityEntityService _communityEntityService;
        protected readonly IEmailRecordRepository _emailRecordRepository;

        protected DocumentFunctionBase(ICommunityEntityService communityEntityService, IEmailRecordRepository emailRecordRepository, IFeedEntityService feedEntityService, IMembershipRepository membershipRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(feedEntityService, membershipRepository, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
            _communityEntityService = communityEntityService;
            _emailRecordRepository = emailRecordRepository;
        }

        protected List<User> GetUsersForJoglDocNotification(Document doc)
        {
            var ids = new List<string>();
            if (doc.UserVisibility != null)
                ids.AddRange(doc.UserVisibility.Select(uv => uv.UserId));

            if (doc.CommunityEntityVisibility != null)
            {
                var memberships = _membershipRepository.List(m => doc.CommunityEntityVisibility.Select(cev => cev.CommunityEntityId).Contains(m.CommunityEntityId) && !m.Deleted);
                ids.AddRange(memberships.Select(u => u.UserId));
            }

            ids.Remove(doc.UpdatedByUserId ?? doc.CreatedByUserId);

            return _userRepository.Get(ids);
        }
    }
}
