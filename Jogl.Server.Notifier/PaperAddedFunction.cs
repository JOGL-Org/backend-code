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
    public class PaperAddedFunction : CreatedFunctionBase<Paper>
    {
        private readonly ICommunityEntityService _communityEntityService;

        public PaperAddedFunction(ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IMembershipRepository membershipRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<CreatedFunctionBase<Paper>> logger) : base(feedEntityService, membershipRepository, userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
            _communityEntityService = communityEntityService;
        }

        [Function(nameof(PaperAddedFunction))]
        public async Task RunPapersAsync(
            [ServiceBusTrigger("paper-added", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var paper = JsonSerializer.Deserialize<Paper>(message.Body.ToString());
            var feedEntity = _communityEntityService.GetFeedEntity(paper.FeedId);
            var adder = _userRepository.Get(paper.UpdatedByUserId ?? paper.CreatedByUserId);

            await RunAsync(paper, feedEntity, u => u.NotificationSettings?.PaperMemberContainerEmail == true, u => u.NotificationSettings?.PaperMemberContainerJogl == true);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
