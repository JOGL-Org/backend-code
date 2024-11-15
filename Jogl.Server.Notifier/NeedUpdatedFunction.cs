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
    public class NeedUpdatedFunction : FeedEntityUpdatedFunctionBase<Need>
    {
        public NeedUpdatedFunction(ICommunityEntityService communityEntityService, IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(communityEntityService, emailRecordRepository, membershipRepository, feedEntityService, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
        }

        [Function(nameof(NeedUpdatedFunction))]
        public async Task RunDocumentsAsync(
            [ServiceBusTrigger("need-updated", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var need = JsonSerializer.Deserialize<Need>(message.Body.ToString());
            var parentEntity = _communityEntityService.GetFeedEntity(need.EntityId);

            await RunAsync(need, parentEntity, u => u.NotificationSettings?.DocumentMemberContainerEmail == true, u => u.NotificationSettings?.DocumentMemberContainerJogl == true);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}