using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier
{
    public abstract class MentionNotificationFunctionBase : NotificationFunctionBase
    {
        protected readonly IFeedEntityService _feedEntityService;

        protected abstract string Origin { get; }
        protected abstract string OriginAction { get; }

        protected MentionNotificationFunctionBase(IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
            _feedEntityService = feedEntityService;
        }

        protected async Task SendMentionEmailAsync(IEnumerable<User> users, User author, FeedEntity feedEntity, CommunityEntity communityEntity, string text, string contentEntityId)
        {
            var communityEntityEmailData = users
                          .ToDictionary(u => u.Email, u => (object)new
                          {
                              NAME = author.FeedTitle,
                              AVATAR_URL = _urlService.GetImageUrl(author.AvatarId),
                              CONTAINER_TYPE = communityEntity == null ? null : _feedEntityService.GetPrintName(communityEntity.FeedType),
                              CONTAINER_URL = communityEntity == null ? null : _urlService.GetUrl(communityEntity),
                              CONTAINER_NAME = communityEntity == null ? null : communityEntity.FeedTitle,
                              OBJECT_TYPE = _feedEntityService.GetPrintName(feedEntity.FeedType),
                              OBJECT_URL = _urlService.GetUrl(feedEntity),
                              OBJECT_NAME = feedEntity.FeedTitle,
                              ORIGIN_TYPE = Origin,
                              ORIGIN_URL = _urlService.GetContentEntityUrl(contentEntityId),
                              ORIGIN_TEXT = text,
                              CTA_URL = _urlService.GetContentEntityUrl(contentEntityId),
                              ABOUT_VISIBLE = communityEntity != feedEntity
                          });

            SetEmailProcessed(users);
            await _emailService.SendEmailAsync(communityEntityEmailData, EmailTemplate.MentionCreated, fromName: author.FirstName);
        }

        protected async Task SendMentionPushNotificationsAsync(IEnumerable<User> users, User author, FeedEntity feedEntity, string contentEntityId)
        {
            var userIds = users.Select(u => u.Id.ToString());
            var pushTokens = _pushNotificationTokenRepository.List(t => userIds.Contains(t.UserId) && !t.Deleted);

            SetPushProcessed(users);
            await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(), $"New mention in {feedEntity.FeedTitle}", $"{author.FullName} {OriginAction}", _urlService.GetContentEntityUrl(contentEntityId));
        }

        protected async Task SendPushNotificationsAsync(IEnumerable<User> users, User author, FeedEntity feedEntity, string contentEntityId)
        {
            var userIds = users.Select(u => u.Id.ToString());
            var pushTokens = _pushNotificationTokenRepository.List(t => userIds.Contains(t.UserId) && !t.Deleted);

            SetPushProcessed(users);
            await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(), $"New {Origin} in {feedEntity.FeedTitle}", $"{author.FullName} {OriginAction}", _urlService.GetContentEntityUrl(contentEntityId));
        }
    }
}
