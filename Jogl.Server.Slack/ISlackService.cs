using Jogl.Server.Slack.DTO;
using SlackNet;

namespace Jogl.Server.Slack
{
    public interface ISlackService
    {
        Task<string> GetUserChannelIdAsync(string channelAccessToken, string userId);
        Task<string> SendMessageAsync(string channelAccessToken, string channelId, string message, string? threadId = default);
        Task DeleteMessageAsync(string channelAccessToken, string channelId, string id);
        Task<List<User>> ListWorkspaceUsersAsync(string channelAccessToken);
        Task<List<MessageDTO>> GetConversationAsync(string channelAccessToken, string channelId, string threadId, IEnumerable<string>? ignoreIds = default);
        Task<MessageDTO> GetPreviousMessage(string channelAccessToken, string channelId, string messageId);
    }
}