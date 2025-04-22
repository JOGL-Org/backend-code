using SlackNet;
using Microsoft.Extensions.Logging;
using Jogl.Server.Slack.DTO;

namespace Jogl.Server.Slack
{
    public class SlackService : ISlackService
    {
        private readonly ISlackApiClient _slackApiClient;
        private readonly ILogger<SlackService> _logger;

        public SlackService(ISlackApiClient slackApiClient, ILogger<SlackService> logger)
        {
            _slackApiClient = slackApiClient;
            _logger = logger;
        }

        public async Task<User> GetUserInfoAsync(string channelAccessToken, string userId)
        {
            var client = _slackApiClient.WithAccessToken(channelAccessToken);

            return await client.Users.Info(userId);
        }

        public async Task<string> GetUserChannelIdAsync(string channelAccessToken, string userId)
        {
            var client = _slackApiClient.WithAccessToken(channelAccessToken);

            return await client.Conversations.Open([userId]);
        }

        public async Task<string> SendMessageAsync(string channelAccessToken, string channelId, string message, string? threadId = default)
        {
            var client = _slackApiClient.WithAccessToken(channelAccessToken);
            var res = await client.Chat.PostMessage(new SlackNet.WebApi.Message
            {
                Channel = channelId,
                Text = message,
                ThreadTs = threadId
            });

            return res.Ts;
        }

        public async Task DeleteMessageAsync(string channelAccessToken, string channelId, string id)
        {
            var client = _slackApiClient.WithAccessToken(channelAccessToken);

            await client.Chat.Delete(id, channelId, true);
        }

        public async Task<List<User>> ListWorkspaceUsersAsync(string channelAccessToken)
        {
            var client = _slackApiClient.WithAccessToken(channelAccessToken);

            var cursor = string.Empty;
            var users = new List<User>();
            while (true)
            {
                var page = await client.Users.List(cursor: cursor, limit: 100);
                users.AddRange(page.Members);
                cursor = page.ResponseMetadata.NextCursor;
                if (string.IsNullOrEmpty(cursor))
                    break;
            }

            return users
                .Where(u => !u.IsBot && u.Id != "USLACKBOT")
                .ToList();
        }

        public async Task<List<MessageDTO>> GetConversationAsync(string channelAccessToken, string channelId, string threadId, IEnumerable<string>? ignoreIds = default)
        {
            var client = _slackApiClient.WithAccessToken(channelAccessToken);

            var history = await client.Conversations.Replies(channelId, threadId, limit: 10);
            return history.Messages.Where(m => ignoreIds == null || !ignoreIds.Contains(m.Ts)).Select(m => new MessageDTO(m.Ts, string.IsNullOrEmpty(m.BotId), m.Text)).ToList();
        }

        public async Task<MessageDTO> GetPreviousMessage(string channelAccessToken, string channelId, string messageId)
        {
            var client = _slackApiClient.WithAccessToken(channelAccessToken);

            var history = await client.Conversations.History(channelId, limit: 1, latestTs: messageId, inclusive: false);
            return history.Messages.Select(m => new MessageDTO(m.Ts, string.IsNullOrEmpty(m.BotId), m.Text)).FirstOrDefault();
        }
    }
}