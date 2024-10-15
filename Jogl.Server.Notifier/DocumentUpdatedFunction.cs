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
    public class DocumentUpdatedFunction : DocumentFunctionBase
    {
        public DocumentUpdatedFunction(ICommunityEntityService communityEntityService, IEmailRecordRepository emailRecordRepository, IFeedEntityService feedEntityService, IMembershipRepository membershipRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(communityEntityService, emailRecordRepository, feedEntityService, membershipRepository, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
        }

        [Function(nameof(DocumentUpdatedFunction))]
        public async Task RunDocumentsAsync(
            [ServiceBusTrigger("document-updated", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var doc = JsonSerializer.Deserialize<Document>(message.Body.ToString());
            doc.FeedEntity = _communityEntityService.GetFeedEntity(doc.FeedId);
            var updater = _userRepository.Get(doc.UpdatedByUserId ?? doc.CreatedByUserId);

            switch (doc.Type)
            {
                case DocumentType.JoglDoc:
                    var emailRecords = _emailRecordRepository.List(er => er.ObjectId == doc.Id.ToString() && er.Type == EmailRecordType.Share && !er.Deleted);
                    var users = GetUsersForJoglDocNotification(doc).Where(u => !emailRecords.Any(er => er.UserId == u.Id.ToString()));

                    await SendEmailAsync(users.Where(u => u.NotificationSettings?.DocumentMemberContainerEmail == true), u => GetEmailPayload(doc, doc.FeedEntity, updater), EmailTemplate.ObjectShared, updater.FirstName);
                    await SendPushAsync(users.Where(u => u.NotificationSettings?.DocumentMemberContainerJogl == true), $"New {_feedEntityService.GetPrintName(FeedType.Document)} in {doc.FeedEntity.FeedTitle}", $"{updater.FullName} shared {doc.FeedTitle} with you", _urlService.GetUrl(doc));

                    await _emailRecordRepository.CreateAsync(users.Select(u => new EmailRecord
                    {
                        CreatedByUserId = doc.UpdatedByUserId,
                        Type = EmailRecordType.Share,
                        ObjectId = doc.Id.ToString(),
                        CreatedUTC = DateTime.UtcNow,
                        UserId = u.Id.ToString()
                    }).ToList());

                    break;
                default:
                    break;
            }

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
