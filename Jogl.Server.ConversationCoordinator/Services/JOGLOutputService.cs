using Jogl.Server.AI;
using Jogl.Server.Data;
using Jogl.Server.Slack;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class JOGLOutputService(ISlackService slackService, ILogger<IJOGLOutputService> logger) : IJOGLOutputService
    {
        public async Task<string> ProcessReplyAsync(InterfaceChannel channel, string workspaceId, string conversationId, string text)
        {
            return null;
        }

        public async Task<string> StartIndicatorAsync(InterfaceChannel channel, string workspaceId, string conversationId)
        {
            return null;
        }

        public async Task StopIndicatorAsync(InterfaceChannel channel, string workspaceId, string conversationId, string indicatorId)
        {

        }

        public async Task<List<InputItem>> LoadConversationAsync(InterfaceChannel channel, string workspaceId, string conversationId)
        {
            return new List<InputItem>();
        }
    }
}
