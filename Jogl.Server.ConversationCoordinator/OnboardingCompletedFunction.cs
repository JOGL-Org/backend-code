using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Conversation.Data;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Slack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator
{
    public class OnboardingCompletedFunction
    {
        private readonly ISlackService _slackService;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly IInterfaceUserRepository _interfaceUserRepository;
        private readonly IInterfaceMessageRepository _interfaceMessageRepository;
        private readonly ILogger<OnboardingCompletedFunction> _logger;

        public OnboardingCompletedFunction(ISlackService slackService, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceUserRepository interfaceUserRepository, IInterfaceMessageRepository interfaceMessageRepository, ILogger<OnboardingCompletedFunction> logger)
        {
            _slackService = slackService;
            _interfaceChannelRepository = interfaceChannelRepository;
            _interfaceUserRepository = interfaceUserRepository;
            _interfaceMessageRepository = interfaceMessageRepository;
            _logger = logger;
        }

        [Function(nameof(OnboardingCompletedFunction))]
        public async Task RunInvitesAsync(
            [ServiceBusTrigger(Const.ONBOARDING_COMPLETED, Connection = "ConnectionString", AutoCompleteMessages =true)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var user = JsonSerializer.Deserialize<Data.User>(message.Body.ToString());
            var interfaceUser = _interfaceUserRepository.Get(u => u.UserId == user.Id.ToString());
            if (interfaceUser == null)
            {
                _logger.LogDebug("No interface user for {0}", user.Id);
                return;
            }

            var interfaceChannel = _interfaceChannelRepository.Get(interfaceUser.ChannelId);
            if (string.IsNullOrEmpty(interfaceChannel.ExternalId))
            {
                _logger.LogWarning("Channel not initialized with access key {0}", interfaceChannel.ExternalId);
                return;
            }

            var channelId = await _slackService.GetUserChannelIdAsync(interfaceChannel.Key, interfaceUser.ExternalId);
            var messageId = await _slackService.SendMessageAsync(interfaceChannel.Key, channelId, string.Format(Messages.Onboarding_Complete, user.FirstName);
            await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                CreatedByUserId = user.Id.ToString(),
                ExternalId = messageId,
                Tag = InterfaceMessage.TAG_ONBOARDING,
            });

            _logger.LogDebug("Sent onboarding-complete message to {0}", interfaceUser.ExternalId);
        }
    }
}
