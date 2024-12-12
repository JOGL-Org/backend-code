using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.Localization;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.Discussion
{
    public class CommentCreatedFunction : DiscussionNotificationFunctionBase
    {
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IContentEntityRepository _contentEntityRepository;
        private readonly IUserContentEntityRecordRepository _userContentEntityRecordRepository;

        public CommentCreatedFunction(ICommunityEntityService communityEntityService, IMembershipRepository membershipRepository, IContentEntityRepository contentEntityRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILocalizationService localizationService, ILogger<NotificationFunctionBase> logger) : base(feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, localizationService, logger)
        {
            _communityEntityService = communityEntityService;
            _membershipRepository = membershipRepository;
            _contentEntityRepository = contentEntityRepository;
            _userContentEntityRecordRepository = userContentEntityRecordRepository;
        }

        protected override string GetOrigin(string language) => _localizationService.GetString("reply", language);
        protected override string GetOriginAction(string language) => _localizationService.GetString("replied", language);

        [Function(nameof(CommentCreatedFunction))]
        public async Task RunCommentsAsync(
            [ServiceBusTrigger("comment-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var comment = JsonSerializer.Deserialize<Comment>(message.Body.ToString());
            var contentEntity = _contentEntityRepository.Get(comment.ContentEntityId);
            var feedEntity = _feedEntityService.GetEntity(contentEntity.FeedId);
            var author = _userRepository.Get(comment.CreatedByUserId);

            //send notifications to mentioned users
            var mentions = comment.Mentions.Where(m => m.EntityType == FeedType.User).ToList();
            var mentionUsers = _userRepository.Get(mentions.Select(m => m.EntityId).ToList());
            if (mentionUsers.Any())
            {
                switch (feedEntity.FeedType)
                {
                    case FeedType.Channel:
                        var channel = (Channel)feedEntity;
                        channel.CommunityEntity = _communityEntityService.Get(channel.CommunityEntityId);
                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true).ToList(), author, channel.CommunityEntity, channel.CommunityEntity, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true).ToList(), author, channel.CommunityEntity, comment.ContentEntityId);
                        break;
                    case FeedType.Event:
                        var ev = (Event)feedEntity;
                        ev.CommunityEntity = _communityEntityService.Get(ev.CommunityEntityId);
                        if (ev.CommunityEntity == null)
                            break;

                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true).ToList(), author, feedEntity, ev.CommunityEntity, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true).ToList(), author, feedEntity, comment.ContentEntityId);
                        break;
                    case FeedType.Need:
                        var need = (Need)feedEntity;
                        need.CommunityEntity = _communityEntityService.Get(need.EntityId);
                        if (need.CommunityEntity == null)
                            break;

                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true).ToList(), author, feedEntity, need.CommunityEntity, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true).ToList(), author, feedEntity, comment.ContentEntityId);
                        break;
                    case FeedType.Document:
                        var doc = (Document)feedEntity;
                        doc.FeedEntity = _communityEntityService.Get(doc.FeedId);

                        if (doc.FeedEntity as CommunityEntity == null)
                            break;

                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true).ToList(), author, feedEntity, doc.FeedEntity as CommunityEntity, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true).ToList(), author, feedEntity, comment.ContentEntityId);
                        break;
                    case FeedType.Paper:
                        var paper = (Paper)feedEntity;

                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true).ToList(), author, feedEntity, null, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true).ToList(), author, feedEntity, comment.ContentEntityId);
                        break;
                }
            }

            //notify users following the post
            var userIds = _userContentEntityRecordRepository
                .Query(ucer => ucer.ContentEntityId == contentEntity.Id.ToString() && ucer.FollowedUTC.HasValue)
                .ToList(ucer => ucer.UserId);

            var users = _userRepository.Get(userIds);
            await SendCommentEmailAsync(users.Where(u => u.NotificationSettings?.ThreadActivityEmail == true).Where(u => !IsEmailProcessed(u)).ToList(), author, feedEntity, comment, comment.ContentEntityId);
            await SendPushNotificationsAsync(users.Where(u => u.NotificationSettings?.ThreadActivityJogl == true).Where(u => !IsPushProcessed(u)).ToList(), author, feedEntity, comment.ContentEntityId);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }

        private async Task SendCommentEmailAsync(IEnumerable<User> users, User author, FeedEntity feedEntity, Comment comment, string contentEntityId)
        {
            var communityEntityEmailData = users
                          .ToDictionary(u => u.Email, u => (object)new
                          {
                              NAME = author.FeedTitle,
                              AVATAR_URL = _urlService.GetImageUrl(author.AvatarId),
                              REPLY_COUNT = "A new reply",
                              ACTIVITY_TYPE = "your post",
                              CONTAINER_TYPE = _localizationService.GetString(feedEntity.FeedType, u.Language),
                              CONTAINER_URL = _urlService.GetUrl(feedEntity),
                              CONTAINER_NAME = feedEntity.FeedTitle,
                              OBJECT_TYPE = _localizationService.GetString(feedEntity.FeedType, u.Language),
                              OBJECT_URL = _urlService.GetUrl(feedEntity),
                              OBJECT_NAME = feedEntity.FeedTitle,
                              COMMENT_URL = _urlService.GetContentEntityUrl(contentEntityId),
                              //COMMENT_DATE = comment.CreatedUTC.ToString(),
                              COMMENT_TEXT = comment.Text,
                              CTA_URL = _urlService.GetContentEntityUrl(contentEntityId),
                              LANGUAGE = u.Language
                          });

            SetEmailProcessed(users);

            await _emailService.SendEmailAsync(communityEntityEmailData, EmailTemplate.CommentAdded, fromName: author.FirstName);
        }
    }
}
