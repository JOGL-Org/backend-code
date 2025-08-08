using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.AI;
using Jogl.Server.AI.Agent;
using Jogl.Server.Conversation.Data;
using Jogl.Server.ConversationCoordinator.Services;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator
{
    public class ConversationCreatedFunction
    {
        private readonly IAgent _aiAgent;
        private readonly IOutputServiceFactory _outputServiceFactory;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly IInterfaceMessageRepository _interfaceMessageRepository;
        private readonly ILogger<ConversationCreatedFunction> _logger;

        public ConversationCreatedFunction(IAgent aiAgent, IOutputServiceFactory outputServiceFactory, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceMessageRepository interfaceMessageRepository, ILogger<ConversationCreatedFunction> logger)
        {
            _aiAgent = aiAgent;
            _outputServiceFactory = outputServiceFactory;
            _interfaceChannelRepository = interfaceChannelRepository;
            _interfaceMessageRepository = interfaceMessageRepository;
            _logger = logger;
        }

        [Function(nameof(ConversationCreatedFunction))]
        public async Task RunInvitesAsync(
            [ServiceBusTrigger(Const.CONVERSATION_CREATED, Connection = "ConnectionString", AutoCompleteMessages =true)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var conversation = JsonSerializer.Deserialize<ConversationCreated>(message.Body.ToString());
            var channel = _interfaceChannelRepository.Get(ic => ic.ExternalId == conversation.WorkspaceId);

            //log incoming message
            var rootInterfaceMessage = new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = conversation.ConversationId,
                ChannelId = conversation.ChannelId,
                ConversationId = conversation.ConversationId,
                UserId = conversation.UserId,
                Text = conversation.Text,
            };

            await _interfaceMessageRepository.CreateAsync(rootInterfaceMessage);

            var outputService = _outputServiceFactory.GetService(conversation.ConversationSystem);
            var indicatorId = await outputService.StartIndicatorAsync(channel, conversation.ChannelId, conversation.ConversationId);

            var response = await _aiAgent.GetInitialResponseAsync([new InputItem { FromUser = true, Text = conversation.Text }], channel?.NodeId, conversation.ConversationSystem);
            var messageId = await outputService.ProcessReplyAsync(channel, conversation.ChannelId, conversation.ConversationId, response.Text);
            await outputService.StopIndicatorAsync(channel, conversation.ChannelId, conversation.ConversationId, indicatorId);

            //log outgoing message
            await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = messageId,
                ChannelId = conversation.WorkspaceId,
                ConversationId = conversation.ConversationId,
                Text = response.Text,
                Tag = InterfaceMessage.TAG_SEARCH_USER,
            });

            //store context in root message
            rootInterfaceMessage.Tag = InterfaceMessage.TAG_SEARCH_USER;
            rootInterfaceMessage.Context = response.Context;
            rootInterfaceMessage.OriginalQuery = response.OriginalQuery;
            await _interfaceMessageRepository.UpdateAsync(rootInterfaceMessage);
        }
    }
}
