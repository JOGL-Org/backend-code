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
    public class ConversationCreatedFunction
    {
        private readonly IAgent _aiAgent;
        private readonly IOutputServiceFactory _outputServiceFactory;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly IInterfaceMessageRepository _interfaceMessageRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConversationCreatedFunction> _logger;

        public ConversationCreatedFunction(IAgent aiAgent, IOutputServiceFactory outputServiceFactory, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceMessageRepository interfaceMessageRepository, IConfiguration configuration, ILogger<ConversationCreatedFunction> logger)
        {
            _aiAgent = aiAgent;
            _outputServiceFactory = outputServiceFactory;
            _interfaceChannelRepository = interfaceChannelRepository;
            _interfaceMessageRepository = interfaceMessageRepository;
            _configuration = configuration;
            _logger = logger;
        }

        [Function(nameof(ConversationCreatedFunction))]
        public async Task RunInvitesAsync(
            [ServiceBusTrigger(Const.CONVERSATION_CREATED, Connection = "ConnectionString", AutoCompleteMessages = true)]
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
            var indicatorId = await outputService.StartIndicatorAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId);

            var response = await _aiAgent.GetInitialResponseAsync([new InputItem { FromUser = true, Text = conversation.Text }], channel?.NodeId, conversation.ConversationSystem);
            var messageResultData = await outputService.SendMessagesAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, response.Text);
            await outputService.StopIndicatorAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, indicatorId);

            ////log outgoing message
            //await _interfaceMessageRepository.CreateAsync(messageResultData.Select(r => new InterfaceMessage
            //{
            //    CreatedUTC = DateTime.UtcNow,
            //    MessageId = r.MessageId,
            //    ChannelId = conversation.WorkspaceId,
            //    ConversationId = conversation.ConversationId,
            //    Text = r.MessageText,
            //    Tag = InterfaceMessage.TAG_SEARCH_USER,
            //}).ToList());

            var mirrorConversationId = await MirrorConversationAsync(conversation.Text);
            await MirrorRepliesAsync(mirrorConversationId, response.Text);

            //store context in root message
            rootInterfaceMessage.MessageMirrorId = mirrorConversationId;
            rootInterfaceMessage.Tag = InterfaceMessage.TAG_SEARCH_USER;
            rootInterfaceMessage.Context = response.Context;
            rootInterfaceMessage.OriginalQuery = response.OriginalQuery;

            await _interfaceMessageRepository.UpdateAsync(rootInterfaceMessage);
        }

        private async Task MirrorRepliesAsync(string mirrorConversationId, List<string> text)
        {
            var outputService = _outputServiceFactory.GetService(Const.TYPE_SLACK);
            var ids = await outputService.SendMessagesAsync(_configuration["Slack:Mirror:WorkspaceID"], _configuration["Slack:Mirror:ChannelID"], mirrorConversationId, text);
        }

        private async Task<string> MirrorConversationAsync(string text)
        {
            var outputService = _outputServiceFactory.GetService(Const.TYPE_SLACK);
            var ids = await outputService.SendMessagesAsync(_configuration["Slack:Mirror:WorkspaceID"], _configuration["Slack:Mirror:ChannelID"], null, [text]);
            return ids.Single().MessageId;
        }
    }
}
