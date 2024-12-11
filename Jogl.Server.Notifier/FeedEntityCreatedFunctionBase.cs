using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier
{
    public class FeedEntityCreatedFunctionBase<T> : FeedEntityFunctionBase<T> where T : FeedEntity
    {
        public FeedEntityCreatedFunctionBase(IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(emailRecordRepository, membershipRepository, feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
        }

        public async Task RunAsync(T entity, FeedEntity parentEntity, Func<User, bool> emailFilter, Func<User, bool> pushFilter)
        {
            var creator = _userRepository.Get(entity.CreatedByUserId);
            var users = GetUsersForFeedEntity(entity);
            await SendEmailAsync(users.Where(emailFilter), u => GetEmailPayload(entity, parentEntity, creator,u ), EmailTemplate.ObjectAdded, creator.FirstName);
            await SendPushAsync(users.Where(pushFilter), $"New {_feedEntityService.GetPrintName(entity.FeedType)} in {parentEntity.FeedTitle}", $"{creator.FullName} added {entity.FeedTitle}", _urlService.GetUrl(entity));
            if (!users.Any())
                return;

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
