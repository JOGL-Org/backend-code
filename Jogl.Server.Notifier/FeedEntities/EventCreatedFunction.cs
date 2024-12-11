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
    public class EventCreatedFunction : FeedEntityCreatedFunctionBase<Event>
    {
        public EventCreatedFunction(IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILocalizationService localizationService, ILogger<NotificationFunctionBase> logger) : base(emailRecordRepository, membershipRepository, feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, localizationService, logger)
        {
        }

        [Function(nameof(EventCreatedFunction))]
        public async Task RunDocumentsAsync(
            [ServiceBusTrigger("event-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var ev = JsonSerializer.Deserialize<Event>(message.Body.ToString());
            ev.FeedEntity = _feedEntityService.GetEntity(ev.CommunityEntityId);
            await RunAsync(ev, ev.FeedEntity, u => u.NotificationSettings?.EventMemberContainerEmail == true, u => u.NotificationSettings?.EventMemberContainerJogl == true);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}