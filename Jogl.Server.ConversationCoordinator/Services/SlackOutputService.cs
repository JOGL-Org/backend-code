using Jogl.Server.AI;
using Jogl.Server.Data;
using Jogl.Server.Slack;
using Microsoft.Extensions.Logging;
using SlackNet;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class SlackOutputService(ISlackService slackService, ILogger<ISlackOutputService> logger) : ISlackOutputService
    {
        public async Task<string> ProcessReplyAsync(InterfaceChannel channel, string workspaceId, string conversationId, string text)
        {
            return await slackService.SendMessageAsync(channel.Key, workspaceId, text, conversationId);
        }

        public async Task<string> StartIndicatorAsync(InterfaceChannel channel, string workspaceId, string conversationId)
        {
            if (string.IsNullOrEmpty(channel.Key))
                logger.LogError("Channel not initialized with access key {externalId}", channel.ExternalId);

            return await slackService.SendMessageAsync(channel.Key, workspaceId, $"Your query is being processed now, your results should be available in a few seconds", conversationId);
        }

        public async Task StopIndicatorAsync(InterfaceChannel channel, string workspaceId, string conversationId, string indicatorId)
        {
            if (string.IsNullOrEmpty(channel.Key))
                logger.LogError("Channel not initialized with access key {externalId}", channel.ExternalId);

            await slackService.DeleteMessageAsync(channel.Key, workspaceId, indicatorId);
        }

        public async Task<List<InputItem>> LoadConversationAsync(InterfaceChannel channel, string workspaceId, string conversationId)
        {
            if (string.IsNullOrEmpty(channel.Key))
                logger.LogError("Channel not initialized with access key {externalId}", channel.ExternalId);

            var messages = await slackService.GetConversationAsync(channel.Key, workspaceId, conversationId);
            return messages.Select(m => new InputItem { FromUser = m.FromUser, Text = m.Text }).ToList();
        }
    }
}
