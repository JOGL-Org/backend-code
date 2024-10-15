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
    public class DocumentCreatedFunction : DocumentFunctionBase
    {
        public DocumentCreatedFunction(ICommunityEntityService communityEntityService, IEmailRecordRepository emailRecordRepository, IFeedEntityService feedEntityService, IMembershipRepository membershipRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(communityEntityService, emailRecordRepository, feedEntityService, membershipRepository, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
        }

        [Function(nameof(DocumentCreatedFunction))]
        public async Task RunDocumentsAsync(
            [ServiceBusTrigger("document-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var doc = JsonSerializer.Deserialize<Document>(message.Body.ToString());
            doc.FeedEntity = _communityEntityService.GetFeedEntity(doc.FeedId);
            doc.CreatedBy = _userRepository.Get(doc.CreatedByUserId);

            switch (doc.Type)
            {
                case DocumentType.JoglDoc:
                    var users = GetUsersForJoglDocNotification(doc);
                    await SendEmailAsync(users.Where(u => u.NotificationSettings?.DocumentMemberContainerEmail == true), u => GetEmailPayload(doc, doc.FeedEntity, doc.CreatedBy), EmailTemplate.ObjectAdded, doc.CreatedBy.FirstName);
                    await SendPushAsync(users.Where(u => u.NotificationSettings?.DocumentMemberContainerJogl == true), $"New {_feedEntityService.GetPrintName(FeedType.Document)} in {doc.FeedEntity.FeedTitle}", $"{doc.CreatedBy.FullName} added {doc.FeedTitle}", _urlService.GetUrl(doc));

                    await _emailRecordRepository.CreateAsync(users.Select(u => new EmailRecord
                    {
                        CreatedByUserId = doc.CreatedByUserId,
                        Type = EmailRecordType.Share,
                        ObjectId = doc.Id.ToString(),
                        CreatedUTC = DateTime.UtcNow,
                        UserId = u.Id.ToString()
                    }).ToList());
                    break;
                default:
                    await RunAsync(doc, doc.FeedEntity, u => u.NotificationSettings?.DocumentMemberContainerEmail == true, u => u.NotificationSettings?.DocumentMemberContainerJogl == true);
                    break;
            }

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
