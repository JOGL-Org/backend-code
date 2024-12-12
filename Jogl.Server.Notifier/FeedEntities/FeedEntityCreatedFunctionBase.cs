using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.Localization;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.FeedEntities
{
    public class FeedEntityCreatedFunctionBase<T> : FeedEntityFunctionBase<T> where T : FeedEntity
    {
        public FeedEntityCreatedFunctionBase(IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILocalizationService localizationService, ILogger<NotificationFunctionBase> logger) : base(emailRecordRepository, membershipRepository, feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, localizationService, logger)
        {
        }

        public async Task RunAsync(T entity, FeedEntity parentEntity, Func<User, bool> emailFilter, Func<User, bool> pushFilter)
        {
            var creator = _userRepository.Get(entity.CreatedByUserId);
            var users = GetUsersForFeedEntity(entity);
            await SendEmailAsync(users.Where(emailFilter), u => GetEmailPayload(entity, parentEntity, creator, u), EmailTemplate.ObjectAdded, creator.FirstName);
            if (!users.Any())
                return;

            foreach (var grp in users.Where(pushFilter).GroupBy(u => u.Language))
            {
                var lang = grp.Key;
                var userIds = grp.Select(u => u.Id.ToString()).ToList();

                var pushTokens = _pushNotificationTokenRepository
                    .Query(t => userIds.Contains(t.UserId))
                    .ToList();

                await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(),
                       _localizationService.GetString("templates.push.feedEntityCreate.title", lang, _localizationService.GetString(entity.FeedType, lang)),
                       _localizationService.GetString("templates.push.feedEntityCreate.body", lang, creator.FullName, parentEntity.FeedTitle),
                       _urlService.GetUrl(entity));
            }

            await _emailRecordRepository.CreateAsync(users.Select(u => new EmailRecord
            {
                CreatedByUserId = entity.CreatedByUserId,
                Type = EmailRecordType.Share,
                ObjectId = entity.Id.ToString(),
                CreatedUTC = DateTime.UtcNow,
                UserId = u.Id.ToString()
            }).ToList());
        }
    }
}
