using System.Collections.Generic;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.AI;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Notifications;
using Microsoft.Azure.Functions.Worker;
using Microsoft.VisualBasic;

namespace Jogl.Server.Notifier
{
    public class CommentCreatedFunction
    {
        private readonly IFeedIntegrationRepository _feedIntegrationRepository;
        private readonly IContentEntityRepository _contentEntityRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IPaperRepository _paperRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IAIService _aiService;
        private readonly INotificationFacade _notificationFacade;

        public CommentCreatedFunction(IFeedIntegrationRepository feedIntegrationRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IPaperRepository paperRepository, IChannelRepository channelRepository, IAIService aiService, INotificationFacade notificationFacade)
        {
            _feedIntegrationRepository = feedIntegrationRepository;
            _contentEntityRepository = contentEntityRepository;
            _commentRepository = commentRepository;
            _paperRepository = paperRepository;
            _channelRepository = channelRepository;
            _aiService = aiService;
            _notificationFacade = notificationFacade;
        }

        [Function(nameof(CommentCreatedFunction))]
        public async Task RunCommentsAsync(
            [ServiceBusTrigger("comment-created", "agent-prompts", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var comment = JsonSerializer.Deserialize<Comment>(message.Body.ToString());
            var contentEntity = _contentEntityRepository.Get(comment.ContentEntityId);
            var allComments = _commentRepository.List(c => c.ContentEntityId == comment.ContentEntityId && !c.Deleted);

            var agentIntegration = _feedIntegrationRepository.Get(i => i.Type == FeedIntegrationType.JOGLAgentPublication && i.FeedId == comment.FeedId && !i.Deleted);
            if (agentIntegration == null)
            {
                await messageActions.CompleteMessageAsync(message);
                return;
            }

            var mention = comment.Mentions?.SingleOrDefault(m => m.EntityId == agentIntegration.Id.ToString());
            if (mention == null)
            {
                await messageActions.CompleteMessageAsync(message);
                return;
            }

            var channel = _channelRepository.Get(agentIntegration.FeedId);
            var papers = _paperRepository.List(p => p.FeedId == channel.CommunityEntityId);

            var messageHistory = new List<InputItem>();
            messageHistory.Add(new InputItem { FromUser = true, Text = contentEntity.Text });
            messageHistory.AddRange(allComments.Select(c => new InputItem { FromUser = c.ExternalID != agentIntegration.Id.ToString(), Text = c.Text }));
            var response = await _aiService.GetResponseAsync(papers.Select(p => p.Summary), messageHistory);

            var replyComment = new Comment
            {
                Text = response,
                ContentEntityId = comment.ContentEntityId,
                FeedId = comment.FeedId,
                ExternalSourceID = agentIntegration.Id.ToString(),
                CreatedUTC = DateTime.UtcNow,
                Overrides = new CommentOverrides
                {
                    UserName = "AI Agent",
                    UserURL = "/",
                    UserAvatarURL = "/images/discussionApps/ai-logo.svg"
                }
            };

            await _commentRepository.CreateAsync(replyComment);

            //process notifications
            await _notificationFacade.NotifyCreatedAsync(replyComment);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}