using Jogl.Server.AI;
using Jogl.Server.ConversationCoordinator.DTO;
using Jogl.Server.Data;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public interface IOutputService
    {
        Task<List<MessageResult>> SendMessagesAsync(InterfaceChannel channel, string workspaceId, string conversationId, List<string> messages);
        //Task<MessageResult> SendMessagesAsync(InterfaceChannel channel, string workspaceId, string conversationId, string text);
        Task<string> StartIndicatorAsync(InterfaceChannel channel, string workspaceId, string conversationId);
        Task StopIndicatorAsync(InterfaceChannel channel, string workspaceId, string conversationId, string indicatorId);
        Task<List<InputItem>> LoadConversationAsync(InterfaceChannel channel, string workspaceId, string conversationId);
    }
}
