using Jogl.Server.AI;
using Jogl.Server.ConversationCoordinator.DTO;
using Jogl.Server.Data;
using Jogl.Server.Slack;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class JOGLOutputService(ISlackService slackService, ILogger<IJOGLOutputService> logger) : IJOGLOutputService
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
            return new List<InputItem>();
        }

        public async Task<List<MessageResult>> SendMessagesAsync(string workspaceId, string channelId, string conversationId, List<string> messages)
        {
            return new List<MessageResult>();
        }

        public async Task<List<MessageResult>> SendMessagesAsync(User? user, string workspaceId, string channelId, string conversationId, List<string> messages)
        {
            return await SendMessagesAsync(workspaceId, channelId, conversationId, messages);
        }
    }
}
