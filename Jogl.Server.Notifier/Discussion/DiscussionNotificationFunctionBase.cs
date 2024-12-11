using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.Localization;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.Discussion
{
    public abstract class DiscussionNotificationFunctionBase : NotificationFunctionBase
    {
        protected readonly IFeedEntityService _feedEntityService;

        protected DiscussionNotificationFunctionBase(IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILocalizationService localizationService, ILogger<NotificationFunctionBase> logger) : base(userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, localizationService, logger)
        {
            _feedEntityService = feedEntityService;
        }

        protected abstract string GetOrigin(string language);
        protected abstract string GetOriginAction(string language);

        protected async Task SendMentionEmailAsync(List<User> users, User author, FeedEntity feedEntity, CommunityEntity communityEntity, string text, string contentEntityId)
        {
            var communityEntityEmailData = users
                          .ToDictionary(u => u.Email, u => (object)new
                          {
                              NAME = author.FeedTitle,
                              AVATAR_URL = _urlService.GetImageUrl(author.AvatarId),
                              CONTAINER_TYPE = communityEntity == null ? null : _localizationService.GetString(communityEntity.FeedType),
                              CONTAINER_URL = communityEntity == null ? null : _urlService.GetUrl(communityEntity),
                              CONTAINER_NAME = communityEntity == null ? null : communityEntity.FeedTitle,
                              OBJECT_TYPE = _localizationService.GetString(feedEntity.FeedType, u.Language),
                              OBJECT_URL = _urlService.GetUrl(feedEntity),
                              OBJECT_NAME = feedEntity.FeedTitle,
                              ORIGIN_TYPE = GetOrigin(u.Language),
                              ORIGIN_URL = _urlService.GetContentEntityUrl(contentEntityId),
                              ORIGIN_TEXT = text,
                              CTA_URL = _urlService.GetContentEntityUrl(contentEntityId),
                              ABOUT_VISIBLE = communityEntity != feedEntity,
                              LANGUAGE = u.Language
                          });

            SetEmailProcessed(users);
            await _emailService.SendEmailAsync(communityEntityEmailData, EmailTemplate.MentionCreated, fromName: author.FirstName);
        }

        protected async Task SendMentionPushNotificationsAsync(List<User> users, User author, FeedEntity feedEntity, string contentEntityId)
        {
            SetPushProcessed(users);

            foreach (var grp in users.GroupBy(u => u.Language))
            {
                var lang = grp.Key;
                var userIds = grp.Select(u => u.Id.ToString()).ToList();

                var pushTokens = _pushNotificationTokenRepository
                    .Query(t => userIds.Contains(t.UserId))
                    .ToList();

                await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(),
                       _localizationService.GetString("templates.push.mention.title", lang, GetOrigin(lang), feedEntity.FeedTitle),
                       _localizationService.GetString("templates.push.mention.body", lang, author.FullName, GetOriginAction(lang), feedEntity.FeedTitle),
                       _urlService.GetContentEntityUrl(contentEntityId));
            }
        }

        protected async Task SendPushNotificationsAsync(List<User> users, User author, FeedEntity feedEntity, string contentEntityId)
        {
            SetPushProcessed(users);

            foreach (var grp in users.GroupBy(u => u.Language))
            {
                var lang = grp.Key;
                var userIds = grp.Select(u => u.Id.ToString()).ToList();

                var pushTokens = _pushNotificationTokenRepository
                    .Query(t => userIds.Contains(t.UserId))
                    .ToList();

                await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(),
                       _localizationService.GetString("templates.push.discussion.title", lang, GetOrigin(lang), feedEntity.FeedTitle),
                       _localizationService.GetString("templates.push.discussion.body", lang, author.FullName, GetOriginAction(lang), feedEntity.FeedTitle),
                       _urlService.GetContentEntityUrl(contentEntityId));
            }
        }
    }
}
