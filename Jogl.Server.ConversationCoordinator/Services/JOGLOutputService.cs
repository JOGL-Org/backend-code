using Jogl.Server.AI;
using Jogl.Server.Business;
using Jogl.Server.ConversationCoordinator.DTO;
using Jogl.Server.Data;
using Jogl.Server.Slack;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class JOGLOutputService(IContentService contentService, ILogger<IJOGLOutputService> logger) : IJOGLOutputService
    {
        public async Task<string> StartIndicatorAsync(string workspaceId, string channelId, string conversationId)
        {
            return null;
        }

        public async Task StopIndicatorAsync(string workspaceId, string channelId, string conversationId, string indicatorId)
        {

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
