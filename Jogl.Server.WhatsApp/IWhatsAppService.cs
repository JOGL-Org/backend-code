using Jogl.Server.WhatsApp.DTO;

namespace Jogl.Server.WhatsApp
{
    public interface IWhatsAppService
    {
        Task<string> SendMessageAsync(string number, string message);
        Task<string> GetMessageAsync(string number, string messageId);
        Task<List<MessageDTO>> GetConversationAsync(string number, string firstMessageId, IEnumerable<string>? ignoreIds = default);
    }
}
