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
    public class FeedEntityUpdatedFunctionBase<T> : FeedEntityFunctionBase<T> where T : FeedEntity
    {
        public FeedEntityUpdatedFunctionBase(IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILocalizationService localizationService, ILogger<NotificationFunctionBase> logger) : base(emailRecordRepository, membershipRepository, feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, localizationService, logger)
        {
        }

        public async Task RunAsync(T entity, FeedEntity parentEntity, Func<User, bool> emailFilter, Func<User, bool> pushFilter)
        {
            var updater = _userRepository.Get(entity.UpdatedByUserId);
            var emailRecords = _emailRecordRepository.List(er => er.ObjectId == entity.Id.ToString() && er.Type == EmailRecordType.Share && !er.Deleted);
            var users = GetUsersForFeedEntity(entity).Where(u => !emailRecords.Any(er => er.UserId == u.Id.ToString()));
            if (!users.Any())
                return;

            await SendEmailAsync(users.Where(emailFilter), u => GetEmailPayload(entity, parentEntity, updater, u), EmailTemplate.ObjectShared, updater.FirstName);

            foreach (var grp in users.Where(pushFilter).GroupBy(u => u.Language))
            {
                var lang = grp.Key;
                var userIds = grp.Select(u => u.Id.ToString()).ToList();

                var pushTokens = _pushNotificationTokenRepository
                    .Query(t => userIds.Contains(t.UserId))
                    .ToList();

                await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(),
                       _localizationService.GetString("templates.push.feedEntityShare.title", lang, _localizationService.GetString(entity.FeedType, lang)),
                       _localizationService.GetString("templates.push.feedEntityShare.body", lang, updater.FullName, entity.FeedTitle),
                       _urlService.GetUrl(entity));
            }

            await _emailRecordRepository.CreateAsync(users.Select(u => new EmailRecord
            {
                CreatedByUserId = entity.UpdatedByUserId,
                Type = EmailRecordType.Share,
                ObjectId = entity.Id.ToString(),
                CreatedUTC = DateTime.UtcNow,
                UserId = u.Id.ToString()
            }).ToList());
        }
    }
}
