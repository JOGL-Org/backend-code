using Jogl.Server.WhatsApp.DTO;
using Microsoft.Extensions.Configuration;
using System.Threading.RateLimiting;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Jogl.Server.WhatsApp
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        public WhatsAppService(IConfiguration configuration)
        {
            _configuration = configuration;
            TwilioClient.Init(configuration["Twilio:AccountSID"], configuration["Twilio:AuthToken"]);
        }

        public async Task<string> SendMessageAsync(string number, string message)
        {
            var firstMessageId = string.Empty;
            foreach (var chunk in SplitString(message, 1600))
            {
                var msg = await MessageResource.CreateAsync(
                  body: chunk,
                  from: new Twilio.Types.PhoneNumber($"whatsapp:{_configuration["Twilio:Number"]}"),
                  to: new Twilio.Types.PhoneNumber($"whatsapp:{number}"));

                if (string.IsNullOrEmpty(firstMessageId))
                    firstMessageId = msg.Sid;
            }

            return firstMessageId;
        }

        public async Task<string> GetMessageAsync(string number, string messageId)
        {
            var msg = await MessageResource.FetchAsync(messageId.Trim());
            return msg?.Body;
        }

        public async Task SendMessageButtonAsync(string number)
        {
            var msg = await MessageResource.CreateAsync(
                     contentSid: "HX7dd2f66c5e458c96046cd4b721834aa2",
                     from: new Twilio.Types.PhoneNumber($"whatsapp:{_configuration["Twilio:Number"]}"),
                     to: new Twilio.Types.PhoneNumber($"whatsapp:{number}"));
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
            var messages = await MessageResource.ReadAsync(
       from: new Twilio.Types.PhoneNumber($"whatsapp:{_configuration["Twilio:Number"]}"),
       to: new Twilio.Types.PhoneNumber($"whatsapp:{number}"),
       limit: 20
   );

            // Twilio returns messages newest first → reverse to chronological order
            var chronological = messages.Reverse().ToList();

            var result = new List<MessageDTO>();
            bool inRange = false;

            foreach (var m in chronological)
            {
                // Skip if this message is in ignore list
                if (ignoreIds != null && ignoreIds.Contains(m.Sid))
                    continue;

                // Wait until we reach the firstMessageId
                if (!inRange)
                {
                    if (m.Sid == firstMessageId)
                    {
                        inRange = true;
                        result.Add(new MessageDTO(m.Sid, m.Direction == MessageResource.DirectionEnum.Inbound, m.Body));
                    }
                    continue;
                }

                // Already in range → add message
                result.Add(new MessageDTO(m.Sid, m.Direction == MessageResource.DirectionEnum.Inbound, m.Body));

                // Stop when next inbound message is found (but include it)
                if (m.Direction == MessageResource.DirectionEnum.Inbound && m.Sid != firstMessageId)
                    break;
            }

            return result;
        }
    }
}
