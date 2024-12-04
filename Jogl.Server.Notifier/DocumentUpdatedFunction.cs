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
    public class DocumentUpdatedFunction : FeedEntityUpdatedFunctionBase<Document>
    {
        public DocumentUpdatedFunction(IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(emailRecordRepository, membershipRepository, feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
        }

        [Function(nameof(DocumentUpdatedFunction))]
        public async Task RunDocumentsAsync(
            [ServiceBusTrigger("document-updated", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var doc = JsonSerializer.Deserialize<Document>(message.Body.ToString());
            doc.FeedEntity = _feedEntityService.GetEntity(doc.FeedId);

            switch (doc.Type)
            {
                case DocumentType.JoglDoc:
                    await RunAsync(doc, doc.FeedEntity, u => u.NotificationSettings?.DocumentMemberContainerEmail == true, u => u.NotificationSettings?.DocumentMemberContainerJogl == true);
                    break;
            }

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}