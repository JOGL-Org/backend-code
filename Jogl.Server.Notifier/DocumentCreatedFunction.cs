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
    public class DocumentCreatedFunction : FeedEntityCreatedFunctionBase<Document>
    {
        public DocumentCreatedFunction(ICommunityEntityService communityEntityService, IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(communityEntityService, emailRecordRepository, membershipRepository, feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
        }

        [Function(nameof(DocumentCreatedFunction))]
        public async Task RunDocumentsAsync(
            [ServiceBusTrigger("document-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var doc = JsonSerializer.Deserialize<Document>(message.Body.ToString());
            var parentEntity = _communityEntityService.GetFeedEntity(doc.FeedId);
            await RunAsync(doc, parentEntity, u => u.NotificationSettings?.DocumentMemberContainerEmail == true, u => u.NotificationSettings?.DocumentMemberContainerJogl == true);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}