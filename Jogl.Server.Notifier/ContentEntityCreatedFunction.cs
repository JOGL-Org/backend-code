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
    public class ContentEntityCreatedFunction : MentionNotificationFunctionBase
    {
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IEventAttendanceRepository _eventAttendanceRepository;
        private readonly IUserFeedRecordRepository _userFeedRecordRepository;

        public ContentEntityCreatedFunction(ICommunityEntityService communityEntityService, IMembershipRepository membershipRepository, IEventAttendanceRepository eventAttendanceRepository, IUserFeedRecordRepository userFeedRecordRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
            _communityEntityService = communityEntityService;
            _membershipRepository = membershipRepository;
            _eventAttendanceRepository = eventAttendanceRepository;
            _userFeedRecordRepository = userFeedRecordRepository;
        }

        protected override string Origin => "post";

        protected override string OriginAction => "posted";

        [Function(nameof(ContentEntityCreatedFunction))]
        public async Task RunPostsAsync(
            [ServiceBusTrigger("content-entity-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var contentEntity = JsonSerializer.Deserialize<ContentEntity>(message.Body.ToString());
            var feedEntity = _feedEntityService.GetEntity(contentEntity.FeedId);
            var author = _userRepository.Get(contentEntity.CreatedByUserId);

            var mentions = contentEntity.Mentions.Where(m => m.EntityType == FeedType.User).ToList();
            var mentionUsers = _userRepository.Get(mentions.Select(m => m.EntityId).ToList());

            switch (feedEntity.FeedType)
            {
                case FeedType.Channel:
                    //send notifications to communityEntity members
                    var channel = (Channel)feedEntity;
                    channel.CommunityEntity = _communityEntityService.Get(channel.CommunityEntityId);
                    var memberships = _membershipRepository.List(m => m.CommunityEntityId == feedEntity.Id.ToString() && m.UserId != contentEntity.CreatedByUserId && !m.Deleted);
                    var users = _userRepository.Get(memberships.Select(m => m.UserId).ToList());

                    //send notifications to mentioned users
                    await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true).Where(u => !IsEmailProcessed(u)), author, channel.CommunityEntity, channel.CommunityEntity, contentEntity.Text, contentEntity.Id.ToString());
                    await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true).Where(u => !IsPushProcessed(u)), author, channel.CommunityEntity, contentEntity.Id.ToString());

                    await SendContainerPostEmailAsync(users.Where(u => u.NotificationSettings?.PostMemberContainerEmail == true).Where(u => !IsEmailProcessed(u)), author, channel.CommunityEntity, contentEntity);
                    await SendPushNotificationsAsync(users.Where(u => u.NotificationSettings?.PostMemberContainerJogl == true).Where(u => !IsPushProcessed(u)), author, channel.CommunityEntity, contentEntity.Id.ToString());
                    break;
                case FeedType.Event:
                    var ev = (Event)feedEntity;
                    var eventCommunityEntity = _communityEntityService.Get(ev.CommunityEntityId);

                    //send notifications to mentioned users
                    await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true).Where(u => !IsEmailProcessed(u)), author, feedEntity, eventCommunityEntity, contentEntity.Text, contentEntity.Id.ToString());
                    await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true).Where(u => !IsPushProcessed(u)), author, feedEntity, contentEntity.Id.ToString());

                    //send notifications to users attending the event
                    var attendances = _eventAttendanceRepository.List(a => a.EventId == feedEntity.Id.ToString() && a.Status == AttendanceStatus.Yes && !string.IsNullOrEmpty(a.UserId) && a.UserId != contentEntity.CreatedByUserId && !a.Deleted);
                    var eventUsers = _userRepository.Get(attendances.Select(a => a.UserId).ToList());
                    await SendObjectPostEmailAsync(eventUsers.Where(u => u.NotificationSettings?.PostAttendingEventEmail == true).Where(u => !IsEmailProcessed(u)), author, feedEntity, eventCommunityEntity, contentEntity);
                    await SendPushNotificationsAsync(eventUsers.Where(u => u.NotificationSettings?.PostAttendingEventJogl == true).Where(u => !IsPushProcessed(u)), author, feedEntity, contentEntity.Id.ToString());

                    //send notifications to the user who created the event
                    var eventCreator = _userRepository.Get(feedEntity.CreatedByUserId);
                    if (eventCreator == null || ev.CreatedByUserId == contentEntity.CreatedByUserId)
                        break;

                    await SendObjectPostEmailAsync(new List<User> { eventCreator }.Where(u => u.NotificationSettings?.PostAuthoredEventEmail == true).Where(u => !IsEmailProcessed(u)), author, feedEntity, eventCommunityEntity, contentEntity);
                    await SendPushNotificationsAsync(new List<User> { eventCreator }.Where(u => u.NotificationSettings?.PostAuthoredEventJogl == true).Where(u => !IsPushProcessed(u)), author, feedEntity, contentEntity.Id.ToString());
                    break;
                case FeedType.Need:
                    var need = (Need)feedEntity;
                    need.CommunityEntity = _communityEntityService.Get(need.EntityId);
                    if (need.CommunityEntity == null)
                        break;

                    //send notifications to mentioned users
                    await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true), author, feedEntity, need.CommunityEntity, contentEntity.Text, contentEntity.Id.ToString());
                    await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true), author, feedEntity, contentEntity.Id.ToString());

                    //send notifications to the user who created the need
                    var needCreator = _userRepository.Get(feedEntity.CreatedByUserId);
                    if (needCreator == null || need.CreatedByUserId == contentEntity.CreatedByUserId)
                        break;

                    await SendObjectPostEmailAsync(new List<User> { needCreator }.Where(u => u.NotificationSettings?.PostAuthoredObjectEmail == true).Where(u => !IsEmailProcessed(u)), author, feedEntity, need.CommunityEntity, contentEntity);
                    await SendPushNotificationsAsync(new List<User> { needCreator }.Where(u => u.NotificationSettings?.PostAuthoredObjectJogl == true).Where(u => !IsPushProcessed(u)), author, feedEntity, contentEntity.Id.ToString());
                    break;
                case FeedType.Document:
                    var doc = (Document)feedEntity;
                    var docCommunityEntity = _communityEntityService.Get(doc.FeedId);
                    if (docCommunityEntity == null)
                        break;

                    //send notifications to mentioned users
                    await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true), author, feedEntity, docCommunityEntity, contentEntity.Text, contentEntity.Id.ToString());
                    await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true), author, feedEntity, contentEntity.Id.ToString());

                    //send notifications to the user who created the doc
                    var docCreator = _userRepository.Get(feedEntity.CreatedByUserId);
                    if (docCreator == null || doc.CreatedByUserId == contentEntity.CreatedByUserId)
                        break;

                    await SendObjectPostEmailAsync(new List<User> { docCreator }.Where(u => u.NotificationSettings?.PostAuthoredObjectEmail == true).Where(u => !IsEmailProcessed(u)), author, feedEntity, docCommunityEntity, contentEntity);
                    await SendPushNotificationsAsync(new List<User> { docCreator }.Where(u => u.NotificationSettings?.PostAuthoredObjectJogl == true).Where(u => !IsPushProcessed(u)), author, feedEntity, contentEntity.Id.ToString());
                    break;
                case FeedType.Paper:
                    //send notifications to mentioned users
                    await SendMentionEmailAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionEmail == true), author, feedEntity, null, contentEntity.Text, contentEntity.Id.ToString());
                    await SendMentionPushNotificationsAsync(mentionUsers.Where(u => u.NotificationSettings?.MentionJogl == true), author, feedEntity, contentEntity.Id.ToString());
                    break;
                default:
                    throw new Exception($"Cannot process post for feed type {feedEntity.FeedType}");
            }

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }

        private async Task SendContainerPostEmailAsync(IEnumerable<User> users, User author, CommunityEntity communityEntity, ContentEntity contentEntity)
        {
            var communityEntityEmailData = users
                          .ToDictionary(u => u.Email, u => (object)new
                          {
                              NAME = author.FeedTitle,
                              AVATAR_URL = _urlService.GetImageUrl(author.AvatarId),
                              CONTAINER_TYPE = _feedEntityService.GetPrintName(communityEntity.FeedType),
                              CONTAINER_URL = _urlService.GetUrl(communityEntity),
                              CONTAINER_NAME = communityEntity.FeedTitle,
                              CONTENT_ENTITY_URL = _urlService.GetContentEntityUrl(contentEntity.Id.ToString()),
                              //CONTENT_ENTITY_DATE = contentEntity.CreatedUTC.ToString(),
                              CONTENT_ENTITY_TEXT = contentEntity.Text,
                              CTA_URL = _urlService.GetContentEntityUrl(contentEntity.Id.ToString()),
                          });

            SetEmailProcessed(users);
            await _emailService.SendEmailAsync(communityEntityEmailData, EmailTemplate.ContentEntityAddedInContainer, fromName: author.FirstName);
        }


        private async Task SendObjectPostEmailAsync(IEnumerable<User> users, User author, FeedEntity feedEntity, CommunityEntity communityEntity, ContentEntity contentEntity)
        {
            var communityEntityEmailData = users
                          .ToDictionary(u => u.Email, u => (object)new
                          {
                              NAME = author.FeedTitle,
                              AVATAR_URL = _urlService.GetImageUrl(author.AvatarId),
                              CONTAINER_TYPE = _feedEntityService.GetPrintName(communityEntity.FeedType),
                              CONTAINER_URL = _urlService.GetUrl(communityEntity),
                              CONTAINER_NAME = communityEntity.FeedTitle,
                              OBJECT_TYPE = _feedEntityService.GetPrintName(feedEntity.FeedType),
                              OBJECT_URL = _urlService.GetUrl(feedEntity),
                              OBJECT_NAME = feedEntity.FeedTitle,
                              CONTENT_ENTITY_URL = _urlService.GetContentEntityUrl(contentEntity.Id.ToString()),
                              //CONTENT_ENTITY_DATE = contentEntity.CreatedUTC.ToString(),
                              CONTENT_ENTITY_TEXT = contentEntity.Text,
                              CTA_URL = _urlService.GetContentEntityUrl(contentEntity.Id.ToString()),
                          });

            SetEmailProcessed(users);
            await _emailService.SendEmailAsync(communityEntityEmailData, EmailTemplate.ContentEntityAddedInObject, fromName: author.FirstName);
        }
    }
}
