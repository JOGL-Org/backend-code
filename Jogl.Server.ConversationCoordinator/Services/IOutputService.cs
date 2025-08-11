using Jogl.Server.AI;
using Jogl.Server.ConversationCoordinator.DTO;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public interface IOutputService
    {
        Task<List<MessageResult>> SendMessagesAsync(string workspaceId, string channelId, string conversationId, List<string> messages);
        //Task<MessageResult> SendMessageAsync(InterfaceChannel channel, string workspaceId, string conversationId, string text);
        Task<string> StartIndicatorAsync(string workspaceId, string channelId, string conversationId);
        Task StopIndicatorAsync(string workspaceId, string channelId, string conversationId, string indicatorId);
        Task<List<InputItem>> LoadConversationAsync(string workspaceId, string channelId, string conversationId);
    }
}
