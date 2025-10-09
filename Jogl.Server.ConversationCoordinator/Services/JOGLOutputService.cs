using Jogl.Server.AI;
using Jogl.Server.Business;
using Jogl.Server.ConversationCoordinator.DTO;
using Jogl.Server.Data;
using Jogl.Server.Notifications;
using Jogl.Server.Slack;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class JOGLOutputService(IContentService contentService, INotificationFacade notificationFacade, ILogger<IJOGLOutputService> logger) : IJOGLOutputService
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeIndicators = new();

        public async Task<string> StartIndicatorAsync(string workspaceId, string channelId, string conversationId)
        {
            var indicatorId = Guid.NewGuid().ToString();
            var cts = new CancellationTokenSource();

            if (!_activeIndicators.TryAdd(indicatorId, cts))
            {
                cts.Dispose();
                throw new InvalidOperationException("Failed to add indicator to dictionary");
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        await notificationFacade.NotifyTypingAsync(new UserIndicator { User = "JOGL Agent", FeedId = channelId });
                        await Task.Delay(5000);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelled
                }
                finally
                {
                    _activeIndicators.TryRemove(indicatorId, out _);
                    cts.Dispose();
                }
            }, cts.Token);

            return indicatorId;
        }

        public async Task StopIndicatorAsync(string workspaceId, string channelId, string conversationId, string indicatorId)
        {
            if (_activeIndicators.TryRemove(indicatorId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }

            await Task.CompletedTask;
        }

        public async Task<List<InputItem>> LoadConversationAsync(string workspaceId, string channelId, string conversationId)
        {
            var messages = contentService.ListMessageEntities(workspaceId);
            return messages
                .Select(m => new InputItem { FromUser = !string.IsNullOrEmpty(m.CreatedByUserId), Text = m.Text })
                .ToList();
        }

        public async Task<List<MessageResult>> SendMessagesAsync(string workspaceId, string channelId, string conversationId, List<string> messages)
        {
            var res = new List<MessageResult>();
            foreach (var msg in messages)
            {
                var id = await contentService.CreateAsync(new ContentEntity
                {
                    Text = msg,
                    Overrides = new DiscussionItemOverrides
                    {
                        UserName = "JOGL Agent",
                        UserURL = "/",
                        UserAvatarURL = "/images/JOGL_logo.png"
                    },
                    FeedId = workspaceId,
                    Type = ContentEntityType.Message,
                    CreatedUTC = DateTime.UtcNow,
                    Status = ContentEntityStatus.Active,
                });

                res.Add(new MessageResult { MessageId = id, MessageText = msg });
            }

            return res;
        }

        public async Task<List<MessageResult>> SendMessagesAsync(User? user, string workspaceId, string channelId, string conversationId, List<string> messages)
        {
            return await SendMessagesAsync(workspaceId, channelId, conversationId, messages);
        }
    }
}
