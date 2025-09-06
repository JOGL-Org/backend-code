using Jogl.Server.InfoBIP.DTO;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Jogl.Server.InfoBIP
{
    public class InfoBIPConversationService(IConfiguration configuration) : IInfoBIPConversationService
    {
        public async Task<string> SendWhatsappMessageAsync(string from, string to, string message)
        {
            var client = new RestClient("https://api.infobip.com");
            var request = new RestRequest("ccaas/1/conversations", Method.Get);
            request.AddHeader("Authorization", $"App {configuration["InfoBip:APIKey"]}");
            request.AddJsonBody(new ConversationMessageRequest { Channel = "WHATSAPP", ContentType = "TEXT", From = from, To = to, Content = new MessageContent { Text = message } });
            var response = await client.ExecuteAsync<ConversationMessageResponse>(request);
            return response.Data.Id;
        }
    }
}
