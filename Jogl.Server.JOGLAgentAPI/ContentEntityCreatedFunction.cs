using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Conversation.Data;
using Jogl.Server.ConversationCoordinator;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.Discussion
{
    public class ContentEntityCreatedFunction
    {
        private readonly IServiceBusProxy _serviceBusProxy;
        private readonly IChannelRepository _channelRepository;
        private readonly ILogger<CommentCreatedFunction> _logger;

        public ContentEntityCreatedFunction(IServiceBusProxy serviceBusProxy, IChannelRepository channelRepository, ILogger<CommentCreatedFunction> logger)
        {
            _serviceBusProxy = serviceBusProxy;
            _channelRepository = channelRepository;
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


            await _serviceBusProxy.SendAsync(new Message
            {
                ConversationSystem = Const.TYPE_JOGL,
                WorkspaceId = contentEntity.FeedId,
                ChannelId = contentEntity.FeedId,
                ConversationId = contentEntity.Id.ToString(),
                Text = contentEntity.Text,
                UserId = contentEntity.CreatedByUserId,
            }, "conversation-created");
        }
    }
}
