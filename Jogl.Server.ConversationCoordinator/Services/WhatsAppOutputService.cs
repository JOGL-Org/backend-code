using Jogl.Server.AI;
using Jogl.Server.Data;
using Jogl.Server.Slack;
using Jogl.Server.WhatsApp;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class WhatsAppOutputService(IWhatsAppService whatsappService, ILogger<IWhatsAppOutputService> logger) : IWhatsAppOutputService
    {
        public async Task<string> ProcessReplyAsync(InterfaceChannel channel, string workspaceId, string conversationId, string text)
        {
            return await whatsappService.SendMessageAsync(workspaceId, text);
        }

        public async Task<string> StartIndicatorAsync(InterfaceChannel channel, string workspaceId, string conversationId)
        {
            return await whatsappService.SendMessageAsync(workspaceId, $"Your query is being processed now, your results should be available in a few seconds");
        }

        public async Task StopIndicatorAsync(InterfaceChannel channel, string workspaceId, string conversationId, string indicatorId)
        {
            //do nothing; whatsapp does not support message deletion via its API
        }

        public async Task<List<InputItem>> LoadConversationAsync(InterfaceChannel channel, string workspaceId, string conversationId)
        {
            //TODO
            return new List<InputItem>();
        }
    }
}
