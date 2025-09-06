using Infobip.Api.SDK;
using Infobip.Api.SDK.WhatsApp.Models;
using Jogl.Server.WhatsApp.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.WhatsApp
{
    public class InfoBipNativeWhatsappService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<InfoBipNativeWhatsappService> _logger;
        public InfoBipNativeWhatsappService(IConfiguration configuration, ILogger<InfoBipNativeWhatsappService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Dictionary<string, string>> SendMessageAsync(string number, string message)
        {
            var configuration = new ApiClientConfiguration(
                "https://api.infobip.com",
                "70b9369c02e513b96fdd807381fce0d8-81a492bf-e04e-40ca-a808-55990d835180"
            );

            var client = new InfobipApiClient(configuration);
            var res = new Dictionary<string, string>();
            foreach (var chunk in SplitString(message, 4096))
            {
                var request = new WhatsAppTextMessageRequest(
                    from: _configuration["InfoBip:Number"],
                    to: number,
                    content: new WhatsAppTextContent(message));
                var msgRes = await client.WhatsApp.SendWhatsAppTextMessage(request);
                res.Add(msgRes.MessageId, chunk);
            }

            return res;
        }

        public async Task<string> GetMessageAsync(string number, string messageId)
        {
            return null;
        }

        private List<string> SplitString(string str, int maxChunkSize)
        {
            var result = new List<string>();
            int offset = 0;

            while (offset < str.Length)
            {
                int length = Math.Min(maxChunkSize, str.Length - offset);
                result.Add(str.Substring(offset, length));
                offset += length;
            }

            return result;
        }

        public async Task<List<MessageDTO>> GetConversationAsync(string number, string firstMessageId, IEnumerable<string>? ignoreIds = null)
        {
            var configuration = new ApiClientConfiguration(
               "https://api.infobip.com",
               "70b9369c02e513b96fdd807381fce0d8-81a492bf-e04e-40ca-a808-55990d835180"
           );

            var client = new InfobipApiClient(configuration);

            var twilioNumber = _configuration["InfoBip:Number"];
            var userNumber = number;

            //client.WhatsApp.
            //// Get messages sent from your system to user
            //var outboundMessages = await MessageResource.ReadAsync(
            //    from: new Twilio.Types.PhoneNumber(twilioNumber),
            //    to: new Twilio.Types.PhoneNumber(userNumber),
            //    limit: 20
            //);

            //// Get messages sent from user to your system
            //var inboundMessages = await MessageResource.ReadAsync(
            //    from: new Twilio.Types.PhoneNumber(userNumber),
            //    to: new Twilio.Types.PhoneNumber(twilioNumber),
            //    limit: 20
            //);

            //// Combine and sort by date
            //var allMessages = outboundMessages.Concat(inboundMessages)
            //    .OrderBy(m => m.DateSent)
            //    .ToList();

            //// Rest of your filtering logic...
            //var result = new List<MessageDTO>();
            //bool inRange = false;

            //foreach (var m in allMessages)
            //{
            //    if (ignoreIds != null && ignoreIds.Contains(m.Sid))
            //        continue;

            //    if (!inRange)
            //    {
            //        if (m.Sid == firstMessageId)
            //        {
            //            inRange = true;
            //            result.Add(new MessageDTO(m.Sid, m.Direction == MessageResource.DirectionEnum.Inbound, m.Body));
            //        }
            //        continue;
            //    }

            //    result.Add(new MessageDTO(m.Sid, m.Direction == MessageResource.DirectionEnum.Inbound, m.Body));
            //}

            //return result;
            return null;
        }
    }
}
