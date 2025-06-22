using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.AI;
using Jogl.Server.AI.Agent;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator
{
    public class CommentCreatedFunction
    {
        private readonly IContentService _contentService;
        private readonly IAgent _agent;
        private readonly IInterfaceMessageRepository _interfaceMessageRepository;
        private readonly ILogger<CommentCreatedFunction> _logger;

        public CommentCreatedFunction(IContentService contentService, IAgent agent, IInterfaceMessageRepository interfaceMessageRepository, ILogger<CommentCreatedFunction> logger)
        {
            _contentService = contentService;
            _agent = agent;
            _interfaceMessageRepository = interfaceMessageRepository;
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

            var rootInterfaceMessage = _interfaceMessageRepository.Get(m => m.ChannelId == comment.FeedId && m.MessageId == comment.ContentEntityId);
            if (rootInterfaceMessage?.Context == null)
                return;

            //log incoming message
            await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = comment.Id.ToString(),
                ChannelId = comment.FeedId,
                ConversationId = comment.ContentEntityId,
                UserId = comment.CreatedByUserId,
                Text = comment.Text,
            });

            var originalPost = _contentService.Get(comment.ContentEntityId);
            var comments = _contentService.ListComments(comment.ContentEntityId, comment.CreatedByUserId, 1, int.MaxValue, Data.Util.SortKey.CreatedDate, true);
            var messages = new List<InputItem>();
            messages.Add(new InputItem { FromUser = true, Text = originalPost.Text });
            messages.AddRange(comments.Items.Select(c => new InputItem { FromUser = !string.IsNullOrEmpty(c.CreatedByUserId), Text = c.Text }));

            var followup = await _agent.GetFollowupResponseAsync(messages, rootInterfaceMessage.Context, "JOGL");
            var replyId = await _contentService.CreateCommentAsync(new Comment
            {
                ContentEntityId = comment.ContentEntityId,
                CreatedUTC = DateTime.UtcNow,
                Text = followup.Text,
                FeedId = comment.FeedId,
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
                ChannelId = comment.FeedId,
                ConversationId = comment.ContentEntityId,
                Text = followup.Text
            });
        }
    }
}
