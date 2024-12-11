using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier
{
    public abstract class CreatedFunctionBase<T> : NotificationFunctionBase where T : FeedEntity
    {
        protected readonly IFeedEntityService _feedEntityService;
        protected readonly IMembershipRepository _membershipRepository;

        protected CreatedFunctionBase(IFeedEntityService feedEntityService, IMembershipRepository membershipRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
            _feedEntityService = feedEntityService;
            _membershipRepository = membershipRepository;
        }

        public async Task RunAsync(T entity, FeedEntity feedEntity, Func<User, bool> emailUserFilter, Func<User, bool> pushUserFilter)
        {
            var memberships = _membershipRepository.List(m => m.CommunityEntityId == feedEntity.Id.ToString() && m.UserId != (entity.UpdatedByUserId ?? entity.CreatedByUserId) && !m.Deleted);
            var users = _userRepository.Get(memberships.Select(m => m.UserId).ToList());
            var creator = _userRepository.Get(entity.UpdatedByUserId ?? entity.CreatedByUserId);

            await SendEmailAsync(users.Where(emailUserFilter), u => GetEmailPayload(entity, feedEntity, creator, u), EmailTemplate.ObjectAdded, creator.FirstName);
            await SendPushAsync(users.Where(pushUserFilter), $"New {_feedEntityService.GetPrintName(entity.FeedType)} in {feedEntity.FeedTitle}", $"{creator.FullName} added {entity.FeedTitle}", _urlService.GetUrl(entity));
        }

        protected object GetEmailPayload(T entity, FeedEntity feedEntity, User creator, User recipient)
        {
            return new
            {
                NAME = creator.FeedTitle,
                ENTITY_TYPE = _feedEntityService.GetPrintName(entity.FeedType),
                ENTITY_URL = _urlService.GetUrl(entity),
                ENTITY_NAME = entity.FeedTitle,
                CONTAINER_TYPE = _feedEntityService.GetPrintName(feedEntity.FeedType),
                CONTAINER_URL = _urlService.GetUrl(feedEntity),
                CONTAINER_NAME = feedEntity.FeedTitle,
                CTA_URL = _urlService.GetUrl(entity),
                LANGUAGE = recipient.Language
            };
        }
    }
}
