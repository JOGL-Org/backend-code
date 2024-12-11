using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.Localization;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier
{
    public abstract class NotificationFunctionBase
    {
        protected readonly IUserRepository _userRepository;
        protected readonly IPushNotificationTokenRepository _pushNotificationTokenRepository;
        protected readonly IEmailService _emailService;
        protected readonly IPushNotificationService _pushNotificationService;
        protected readonly IUrlService _urlService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ILogger<NotificationFunctionBase> _logger;

        private List<string> processedUserIdsEmail = new List<string>();
        private List<string> processedUserIdsPush = new List<string>();

        public NotificationFunctionBase(IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILocalizationService localizationService, ILogger<NotificationFunctionBase> logger)
        {
            _userRepository = userRepository;
            _pushNotificationTokenRepository = pushNotificationTokenRepository;
            _emailService = emailService;
            _pushNotificationService = pushNotificationService;
            _urlService = urlService;
            _localizationService = localizationService;
            _logger = logger;
        }

        protected void SetEmailProcessed(IEnumerable<User> users)
        {
            processedUserIdsEmail.AddRange(users.Select(u => u.Id.ToString()));
        }

        protected bool IsEmailProcessed(User user)
        {
            return processedUserIdsEmail.Contains(user.Id.ToString());
        }

        protected void SetPushProcessed(IEnumerable<User> users)
        {
            processedUserIdsPush.AddRange(users.Select(u => u.Id.ToString()));
        }

        protected bool IsPushProcessed(User user)
        {
            return processedUserIdsPush.Contains(user.Id.ToString());
        }

        protected async Task SendEmailAsync(IEnumerable<User> users, Func<User, object> payloadCreation, EmailTemplate template, string creatorName)
        {
            if (!users.Any())
                return;

            var emailData = users
                .ToDictionary(u => u.Email, u => payloadCreation(u));

            await _emailService.SendEmailAsync(emailData, template, fromName: creatorName);
        }
    }
}
