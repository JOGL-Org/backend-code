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

namespace Jogl.Server.Notifier.FeedEntities
{
    public class DocumentCreatedFunction : FeedEntityCreatedFunctionBase<Document>
    {
        public DocumentCreatedFunction(IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILocalizationService localizationService, ILogger<NotificationFunctionBase> logger) : base(emailRecordRepository, membershipRepository, feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, localizationService, logger)
        {
        }

        [Function(nameof(DocumentCreatedFunction))]
        public async Task RunDocumentsAsync(
            [ServiceBusTrigger("document-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var doc = JsonSerializer.Deserialize<Document>(message.Body.ToString());
            doc.FeedEntity = _feedEntityService.GetEntity(doc.FeedId);
            await RunAsync(doc, doc.FeedEntity, u => u.NotificationSettings?.DocumentMemberContainerEmail == true, u => u.NotificationSettings?.DocumentMemberContainerJogl == true);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}