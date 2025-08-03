using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Conversation.Data;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator
{
    public class CommentCreatedFunction
    {
        private readonly IServiceBusProxy _serviceBusProxy;
        private readonly IChannelRepository _channelRepository;
        private readonly ILogger<CommentCreatedFunction> _logger;

        public CommentCreatedFunction(IServiceBusProxy serviceBusProxy, IChannelRepository channelRepository, ILogger<CommentCreatedFunction> logger)
        {
            _serviceBusProxy = serviceBusProxy;
            _channelRepository = channelRepository;
            _logger = logger;
        }

        [Function(nameof(CommentCreatedFunction))]
        public async Task RunCommentsAsync(
            [ServiceBusTrigger("comment-created", "conversations", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var comment = JsonSerializer.Deserialize<Comment>(message.Body.ToString());
            if (string.IsNullOrEmpty(comment.CreatedByUserId))
                return;

            var channel = _channelRepository.Get(comment.FeedId);
            if (channel?.Key != "USER_SEARCH")
                return;

            await _serviceBusProxy.SendAsync(new ConversationReplyCreated
            {
                ConversationSystem = Const.TYPE_JOGL,
                WorkspaceId = comment.FeedId,
                ChannelId = comment.FeedId,
                ConversationId = comment.ContentEntityId,
                MessageId = comment.Id.ToString(),
                Text = comment.Text,
                UserId = comment.CreatedByUserId,
            }, "conversation-reply-created");
        }
    }
}
