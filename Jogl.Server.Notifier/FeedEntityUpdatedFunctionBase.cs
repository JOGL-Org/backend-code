using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier
{
    public class FeedEntityUpdatedFunctionBase<T> : FeedEntityFunctionBase<T> where T : FeedEntity
    {
        public FeedEntityUpdatedFunctionBase(ICommunityEntityService communityEntityService, IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(communityEntityService, emailRecordRepository, membershipRepository, feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
        }

        public async Task RunAsync(T entity, FeedEntity parentEntity, Func<User, bool> emailFilter, Func<User, bool> pushFilter)
        {
            var updater = _userRepository.Get( entity.UpdatedByUserId);
            var emailRecords = _emailRecordRepository.List(er => er.ObjectId == entity.Id.ToString() && er.Type == EmailRecordType.Share && !er.Deleted);
            var users = GetUsersForFeedEntity(entity).Where(u => !emailRecords.Any(er => er.UserId == u.Id.ToString()));
            if (!users.Any())
                return;

            await SendEmailAsync(users.Where(emailFilter), u => GetEmailPayload(entity, parentEntity, updater), EmailTemplate.ObjectShared, updater.FirstName);
            await SendPushAsync(users.Where(pushFilter), $"New {_feedEntityService.GetPrintName(entity.FeedType)} in {parentEntity.FeedTitle}", $"{updater.FullName} shared {entity.FeedTitle} with you", _urlService.GetUrl(entity));

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
