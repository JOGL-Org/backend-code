using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier
{
    public class CommentCreatedFunction : MentionNotificationFunctionBase
    {
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IContentEntityRepository _contentEntityRepository;
        private readonly IUserContentEntityRecordRepository _userContentEntityRecordRepository;

        protected override string Origin => "post";

        protected override string OriginAction => "posted";

        public CommentCreatedFunction(ICommunityEntityService communityEntityService, IMembershipRepository membershipRepository, IContentEntityRepository contentEntityRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
            _communityEntityService = communityEntityService;
            _membershipRepository = membershipRepository;
            _contentEntityRepository = contentEntityRepository;
            _userContentEntityRecordRepository = userContentEntityRecordRepository;
        }

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
                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true), author, channel.CommunityEntity, channel.CommunityEntity, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true), author, channel.CommunityEntity, comment.ContentEntityId);
                        break;
                    case FeedType.Event:
                        var ev = (Event)feedEntity;
                        ev.CommunityEntity = _communityEntityService.Get(ev.CommunityEntityId);
                        if (ev.CommunityEntity == null)
                            break;

                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true), author, feedEntity, ev.CommunityEntity, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true), author, feedEntity, comment.ContentEntityId);
                        break;
                    case FeedType.Need:
                        var need = (Need)feedEntity;
                        need.CommunityEntity = _communityEntityService.Get(need.EntityId);
                        if (need.CommunityEntity == null)
                            break;

                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true), author, feedEntity, need.CommunityEntity, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true), author, feedEntity, comment.ContentEntityId);
                        break;
                    case FeedType.Document:
                        var doc = (Document)feedEntity;
                        doc.FeedEntity = _communityEntityService.Get(doc.FeedId);

                        if (doc.FeedEntity as CommunityEntity == null)
                            break;

                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true), author, feedEntity, doc.FeedEntity as CommunityEntity, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true), author, feedEntity, comment.ContentEntityId);
                        break;
                    case FeedType.Paper:
                        var paper = (Paper)feedEntity;

                        await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true), author, feedEntity, null, comment.Text, comment.ContentEntityId);
                        await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true), author, feedEntity, comment.ContentEntityId);
                        break;
                }
            }

            if (comment.CreatedByUserId != contentEntity.CreatedByUserId)
            {
                //notify content entity creator
                var contentEntityAuthor = _userRepository.Get(contentEntity.CreatedByUserId);
                var users = new List<User> { contentEntityAuthor };

                await SendCommentEmailAsync(users.Where(u => u.NotificationSettings?.ThreadActivityEmail == true).Where(u => !IsEmailProcessed(u)), author, feedEntity, comment, comment.ContentEntityId);
                await SendPushNotificationsAsync(users.Where(u => u.NotificationSettings?.ThreadActivityJogl == true).Where(u => !IsPushProcessed(u)), author, feedEntity, comment.ContentEntityId);
            }

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
                              CONTAINER_TYPE = _feedEntityService.GetPrintName(feedEntity.FeedType),
                              CONTAINER_URL = _urlService.GetUrl(feedEntity),
                              CONTAINER_NAME = feedEntity.FeedTitle,
                              OBJECT_TYPE = _feedEntityService.GetPrintName(feedEntity.FeedType),
                              OBJECT_URL = _urlService.GetUrl(feedEntity),
                              OBJECT_NAME = feedEntity.FeedTitle,
                              COMMENT_URL = _urlService.GetContentEntityUrl(contentEntityId),
                              //COMMENT_DATE = comment.CreatedUTC.ToString(),
                              COMMENT_TEXT = comment.Text,
                              CTA_URL = _urlService.GetContentEntityUrl(contentEntityId),
                          });

            SetEmailProcessed(users);

            await _emailService.SendEmailAsync(communityEntityEmailData, EmailTemplate.CommentAdded, fromName: author.FirstName);
        }
    }
}
