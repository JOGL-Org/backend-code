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
    public class ConversationReplyCreatedFunction
    {
        private readonly IAgent _aiAgent;
        private readonly IOutputServiceFactory _outputServiceFactory;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly IInterfaceMessageRepository _interfaceMessageRepository;
        private readonly ILogger<ConversationReplyCreatedFunction> _logger;

        public ConversationReplyCreatedFunction(IAgent aiAgent, IOutputServiceFactory outputServiceFactory, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceMessageRepository interfaceMessageRepository, ILogger<ConversationReplyCreatedFunction> logger)
        {
            _aiAgent = aiAgent;
            _outputServiceFactory = outputServiceFactory;
            _interfaceChannelRepository = interfaceChannelRepository;
            _interfaceMessageRepository = interfaceMessageRepository;
            _logger = logger;
        }

        [Function(nameof(ConversationReplyCreatedFunction))]
        public async Task RunInvitesAsync(
            [ServiceBusTrigger(Const.CONVERSATION_REPLY_CREATED, Connection = "ConnectionString", AutoCompleteMessages =true)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var conversationReply = JsonSerializer.Deserialize<ConversationReplyCreated>(message.Body.ToString());
            var channel = _interfaceChannelRepository.Get(ic => ic.ExternalId == conversationReply.WorkspaceId);
            if (channel == null)
            {
                _logger.LogWarning("Channel not known: {0}", conversationReply.WorkspaceId);
                return;
            }

            var rootInterfaceMessage = _interfaceMessageRepository.Get(m => m.ChannelId == conversationReply.ChannelId && m.MessageId == conversationReply.ConversationId);
            if (rootInterfaceMessage?.Context == null)
                return;

            //log incoming message
            await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = conversationReply.MessageId,
                ChannelId = channel.ExternalId,
                ConversationId = conversationReply.ConversationId,
                UserId = conversationReply.UserId,
                Text = conversationReply.Text
            });

            var outputService = _outputServiceFactory.GetService(conversationReply.ConversationSystem);
            var indicatorId = await outputService.StartIndicatorAsync(channel, conversationReply.ChannelId, conversationReply.ConversationId);

            var messages = await outputService.LoadConversationAsync(channel, conversationReply.ChannelId, conversationReply.ConversationId);
            var response = await _aiAgent.GetFollowupResponseAsync([new InputItem { FromUser = true, Text = conversationReply.Text }], rootInterfaceMessage.Context, rootInterfaceMessage.OriginalQuery, conversationReply.ConversationSystem);
            var messageId = await outputService.ProcessReplyAsync(channel, conversationReply.ChannelId, conversationReply.ConversationId, response.Text);
            await outputService.StopIndicatorAsync(channel, conversationReply.ChannelId, conversationReply.ConversationId, indicatorId);

            //log outgoing message
            await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = messageId,
                ChannelId = channel.ExternalId,
                ConversationId = conversationReply.ConversationId,
                Text = response.Text,
                Tag = InterfaceMessage.TAG_SEARCH_USER,
            });
        }
    }
}
