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
                var messageResult = await whatsappService.SendMessageAsync(workspaceId, message);
                result.AddRange(messageResult.Select(r => new MessageResult { MessageId = r.Key, MessageText = r.Value }));
                Thread.Sleep(500);
            }

            return result;
        }

        public async Task<string> StartIndicatorAsync(string workspaceId, string channelId, string conversationId)
        {
            var res = await whatsappService.SendMessageAsync(workspaceId, $"Your query is being processed now, your results should be available in a few seconds");
            return res.Keys.Single();
        }

        public async Task StopIndicatorAsync(string workspaceId, string channelId, string conversationId, string indicatorId)
        {
            //do nothing; whatsapp does not support message deletion via its API
        }

        public async Task<List<InputItem>> LoadConversationAsync(string workspaceId, string channelId, string conversationId)
        {
            var messages = await whatsappService.GetConversationAsync(channelId, conversationId);
            return messages.Skip(1).Select(m => new InputItem { FromUser = m.FromUser, Text = m.Text }).ToList();
        }
    }
}
