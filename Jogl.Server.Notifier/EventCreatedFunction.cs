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
using SharpCompress.Writers;

namespace Jogl.Server.Notifier
{
    public class EventCreatedFunction : CreatedFunctionBase<Event>
    {
        private readonly ICommunityEntityService _communityEntityService;

        public EventCreatedFunction(ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IMembershipRepository membershipRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<CreatedFunctionBase<Event>> logger) : base(feedEntityService, membershipRepository, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
            _communityEntityService = communityEntityService;
        }

        [Function(nameof(EventCreatedFunction))]
        public async Task RunEventAsync(
            [ServiceBusTrigger("event-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var ev = JsonSerializer.Deserialize<Event>(message.Body.ToString());
            if (ev.Visibility == EventVisibility.Private)
                return;

            var feedEntity = _communityEntityService.GetFeedEntity(ev.CommunityEntityId);
            
            await RunAsync(ev, feedEntity, u => u.NotificationSettings?.EventMemberContainerEmail == true, u => u.NotificationSettings?.EventMemberContainerJogl == true);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
