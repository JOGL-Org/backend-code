using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.AI;
using Jogl.Server.AI.Agent;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.Discussion
{
    public class ContentEntityCreatedFunction
    {
        private readonly IContentService _contentService;
        private readonly IAgent _agent;
        private readonly IChannelRepository _channelRepository;
        private readonly IInterfaceMessageRepository _interfaceMessageRepository;
        private readonly ILogger<ContentEntityCreatedFunction> _logger;

        public ContentEntityCreatedFunction(IContentService contentService, IAgent agent, IChannelRepository channelRepository, IInterfaceMessageRepository interfaceMessageRepository, ILogger<ContentEntityCreatedFunction> logger)
        {
            _contentService = contentService;
            _agent = agent;
            _channelRepository = channelRepository;
            _interfaceMessageRepository = interfaceMessageRepository;
            _logger = logger;
        }

        [Function(nameof(ContentEntityCreatedFunction))]
        public async Task RunCommentsAsync(
             [ServiceBusTrigger("content-entity-created", "conversations", Connection = "ConnectionString")]
             ServiceBusReceivedMessage message,
             ServiceBusMessageActions messageActions)
        {
            var contentEntity = JsonSerializer.Deserialize<ContentEntity>(message.Body.ToString());
            if (string.IsNullOrEmpty(contentEntity.CreatedByUserId))
                return;

            var channel = _channelRepository.Get(contentEntity.FeedId);
            if (channel?.Key != "USER_SEARCH")
                return;

            //log incoming message
            var rootInterfaceMessage = new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = contentEntity.Id.ToString(),
                ChannelId = contentEntity.FeedId,
                ConversationId = contentEntity.Id.ToString(),
                UserId = contentEntity.CreatedByUserId,
                Text = contentEntity.Text,
            };

            await _interfaceMessageRepository.CreateAsync(rootInterfaceMessage);

            var response = await _agent.GetInitialResponseAsync([new InputItem { FromUser = true, Text = contentEntity.Text }], new Dictionary<string, string>(), contentEntity.NodeId, "JOGL");
            var replyId = await _contentService.CreateCommentAsync(new Comment
            {
                ContentEntityId = contentEntity.Id.ToString(),
                CreatedUTC = DateTime.UtcNow,
                Text = response.Text,
                FeedId = contentEntity.FeedId,
                Overrides = new DiscussionItemOverrides
                {
                    UserName = "Search Agent",
                    UserURL = "/",
                    UserAvatarURL = "/images/discussionApps/ai-logo.svg"
                },
            });

            //log outgoing message
            await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = replyId,
                ChannelId = contentEntity.FeedId,
                ConversationId = contentEntity.Id.ToString(),
                Text = response.Text,
                Tag = InterfaceMessage.TAG_SEARCH_USER,
            });

            //store context in root message
            rootInterfaceMessage.Context = response.Context;
            await _interfaceMessageRepository.UpdateAsync(rootInterfaceMessage);
        }
    }
}
