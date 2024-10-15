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
    public class NeedCreatedFunction : CreatedFunctionBase<Need>
    {
        private readonly ICommunityEntityService _communityEntityService;

        public NeedCreatedFunction(ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IMembershipRepository membershipRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<CreatedFunctionBase<Need>> logger) : base(feedEntityService, membershipRepository, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
            _communityEntityService = communityEntityService;
        }

        [Function(nameof(NeedCreatedFunction))]
        public async Task RunNeedsAsync(
            [ServiceBusTrigger("need-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var need = JsonSerializer.Deserialize<Need>(message.Body.ToString());
            need.CommunityEntity = _communityEntityService.Get(need.EntityId);
            need.CreatedBy = _userRepository.Get(need.CreatedByUserId);

            await RunAsync(need, need.CommunityEntity, u => u.NotificationSettings?.NeedMemberContainerEmail == true, u => u.NotificationSettings?.NeedMemberContainerJogl == true);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
