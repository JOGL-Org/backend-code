using Jogl.Server.AI;
using Jogl.Server.ConversationCoordinator.DTO;
using Jogl.Server.DB;
using Jogl.Server.Slack;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class SlackOutputService(IInterfaceChannelRepository interfaceChannelRepository, ISlackService slackService, ILogger<ISlackOutputService> logger) : ISlackOutputService
    {
        public async Task<List<MessageResult>> SendMessagesAsync(string workspaceId, string channelId, string conversationId, List<string> messages)
        {
            var channel = interfaceChannelRepository.Get(ic => ic.ExternalId == workspaceId);
            if (channel == null)
            {
                logger.LogWarning("Channel not known: {workspaceId}", workspaceId);
                return new List<MessageResult>();
            }

            if (string.IsNullOrEmpty(channel.Key))
            {
                logger.LogError("Channel not initialized with access key {workspaceId}", workspaceId);
                return new List<MessageResult>();
            }

            var result = new List<MessageResult>();
            foreach (var message in messages)
            {
                var messageId = await slackService.SendMessageAsync(channel.Key, channelId, message, conversationId);
                result.Add(new MessageResult { MessageId = messageId, MessageText = message });
            }

            return result;
        }

        public async Task<string> StartIndicatorAsync(string workspaceId, string channelId, string conversationId)
        {
            var channel = interfaceChannelRepository.Get(ic => ic.ExternalId == workspaceId);
            if (channel == null)
            {
                logger.LogWarning("Channel not known: {workspaceId}", workspaceId);
                return null;
            }

            if (string.IsNullOrEmpty(channel.Key))
            {
                logger.LogError("Channel not initialized with access key {workspaceId}", workspaceId);
                return null;
            }

            return await slackService.SendMessageAsync(channel.Key, channelId, $"Your query is being processed now, your results should be available in a few seconds", conversationId);
        }

        public async Task StopIndicatorAsync(string workspaceId, string channelId, string conversationId, string indicatorId)
        {
            var channel = interfaceChannelRepository.Get(ic => ic.ExternalId == workspaceId);
            if (channel == null)
            {
                logger.LogWarning("Channel not known: {workspaceId}", workspaceId);
                return;
            }

            if (string.IsNullOrEmpty(channel.Key))
            {
                logger.LogError("Channel not initialized with access key {workspaceId}", workspaceId);
                return;
            }

            await slackService.DeleteMessageAsync(channel.Key, channelId, indicatorId);
        }

        public async Task<List<InputItem>> LoadConversationAsync(string workspaceId, string channelId, string conversationId)
        {
            var channel = interfaceChannelRepository.Get(ic => ic.ExternalId == workspaceId);
            if (channel == null)
            {
                logger.LogWarning("Channel not known: {workspaceId}", workspaceId);
                return new List<InputItem>();
            }

            if (string.IsNullOrEmpty(channel.Key))
            {
                logger.LogError("Channel not initialized with access key {workspaceId}", workspaceId);
                return new List<InputItem>();
            }

            var messages = await slackService.GetConversationAsync(channel.Key, channelId, conversationId);
            return messages.Select(m => new InputItem { FromUser = m.FromUser, Text = m.Text }).ToList();
        }
    }
}
