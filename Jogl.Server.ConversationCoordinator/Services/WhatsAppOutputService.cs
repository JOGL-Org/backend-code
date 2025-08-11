using Jogl.Server.AI;
using Jogl.Server.ConversationCoordinator.DTO;
using Jogl.Server.WhatsApp;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class WhatsAppOutputService(IWhatsAppService whatsappService, ILogger<IWhatsAppOutputService> logger) : IWhatsAppOutputService
    {
        public async Task<List<MessageResult>> SendMessagesAsync(string workspaceId, string channelId, string conversationId, List<string> messages)
        {
            var result = new List<MessageResult>();
            foreach (var message in messages)
            {
                var messageId = await whatsappService.SendMessageAsync(workspaceId, message);
                result.Add(new MessageResult { MessageId = messageId, MessageText = message });
            }

            return result;
        }

        public async Task<string> StartIndicatorAsync( string workspaceId, string channelId, string conversationId)
        {
            return await whatsappService.SendMessageAsync(workspaceId, $"Your query is being processed now, your results should be available in a few seconds");
        }

        public async Task StopIndicatorAsync(string workspaceId, string channelId, string conversationId, string indicatorId)
        {
            //do nothing; whatsapp does not support message deletion via its API
        }

        public async Task<List<InputItem>> LoadConversationAsync(string workspaceId, string channelId, string conversationId)
        {
            //TODO
            return new List<InputItem>();
        }
    }
}
