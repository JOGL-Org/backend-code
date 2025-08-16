using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.AI;
using Jogl.Server.AI.Agent;
using Jogl.Server.Conversation.Data;
using Jogl.Server.ConversationCoordinator.Services;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator
{
    public class ConversationReplyCreatedFunction : BaseFunction
    {
        private readonly IAgent _aiAgent;
        private readonly IInterfaceMessageRepository _interfaceMessageRepository;
        private readonly ILogger<ConversationReplyCreatedFunction> _logger;

        public ConversationReplyCreatedFunction(IAgent aiAgent, IOutputServiceFactory outputServiceFactory, IInterfaceMessageRepository interfaceMessageRepository, IConfiguration configuration, ILogger<ConversationReplyCreatedFunction> logger) : base(outputServiceFactory, configuration)
        {
            _aiAgent = aiAgent;
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
            var rootInterfaceMessage = _interfaceMessageRepository.Get(m => m.ChannelId == conversationReply.ChannelId && m.MessageId == conversationReply.ConversationId);
            if (rootInterfaceMessage?.Context == null)
                return;

            await MirrorRepliesAsync(rootInterfaceMessage.MessageMirrorId, [conversationReply.Text]);

            //log incoming message
            await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = conversationReply.MessageId,
                ChannelId = conversationReply.WorkspaceId,
                ConversationId = conversationReply.ConversationId,
                UserId = conversationReply.UserId,
                Text = conversationReply.Text
            });

            var outputService = _outputServiceFactory.GetService(conversationReply.ConversationSystem);
            var indicatorId = await outputService.StartIndicatorAsync(conversationReply.WorkspaceId, conversationReply.ChannelId, conversationReply.ConversationId);

            //var messages = await outputService.LoadConversationAsync(conversationReply.WorkspaceId, conversationReply.ChannelId, conversationReply.ConversationId);
            var response = await _aiAgent.GetFollowupResponseAsync([new InputItem { FromUser = true, Text = conversationReply.Text }], rootInterfaceMessage.Context, rootInterfaceMessage.OriginalQuery, conversationReply.ConversationSystem);
            var messageResultData = await outputService.SendMessagesAsync(conversationReply.WorkspaceId, conversationReply.ChannelId, conversationReply.ConversationId, response.Text);
            await outputService.StopIndicatorAsync(conversationReply.WorkspaceId, conversationReply.ChannelId, conversationReply.ConversationId, indicatorId);

            await MirrorRepliesAsync(rootInterfaceMessage.MessageMirrorId, response.Text);

            //log outgoing message
            await _interfaceMessageRepository.CreateAsync(messageResultData.Select(r => new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = r.MessageId,
                ChannelId = conversationReply.ChannelId,
                ConversationId = conversationReply.ConversationId,
                Text = r.MessageText,
                Tag = InterfaceMessage.TAG_SEARCH_USER,
            }).ToList());
        }
    }
}