using Jogl.Server.WhatsApp.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Jogl.Server.WhatsApp
{
    public class TwilioWhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwilioWhatsAppService> _logger;
        public TwilioWhatsAppService(IConfiguration configuration, ILogger<TwilioWhatsAppService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            TwilioClient.Init(configuration["Twilio:AccountSID"], configuration["Twilio:AuthToken"]);
        }

        public async Task<Dictionary<string, string>> SendMessageAsync(string number, string message)
        {
            var res = new Dictionary<string, string>();
            var chunks = SplitString(message, 1600);
            foreach (var chunk in chunks)
            {
                var msg = await MessageResource.CreateAsync(
                    body: chunk,
                    from: new Twilio.Types.PhoneNumber($"whatsapp:{_configuration["Twilio:Number"]}"),
                    to: new Twilio.Types.PhoneNumber($"whatsapp:{number}"));

                res.Add(msg.Sid, chunk);

                //sleep if more messages 
                if (chunks.IndexOf(chunk) != chunks.Count - 1)
                    Thread.Sleep(5000);
            }

            return res;
        }

        public async Task<string> GetMessageAsync(string number, string messageId)
        {
            var msg = await MessageResource.FetchAsync(messageId.Trim());
            return msg?.Body;
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
            var twilioNumber = $"whatsapp:{_configuration["Twilio:Number"]}";
            var userNumber = $"whatsapp:{number}";

            // Get messages sent from your system to user
            var outboundMessages = await MessageResource.ReadAsync(
                from: new Twilio.Types.PhoneNumber(twilioNumber),
                to: new Twilio.Types.PhoneNumber(userNumber),
                limit: 20
            );

            // Get messages sent from user to your system
            var inboundMessages = await MessageResource.ReadAsync(
                from: new Twilio.Types.PhoneNumber(userNumber),
                to: new Twilio.Types.PhoneNumber(twilioNumber),
                limit: 20
            );

            // Combine and sort by date
            var allMessages = outboundMessages.Concat(inboundMessages)
                .OrderBy(m => m.DateSent)
                .ToList();

            // Rest of your filtering logic...
            var result = new List<MessageDTO>();
            bool inRange = false;

            foreach (var m in allMessages)
            {
                if (ignoreIds != null && ignoreIds.Contains(m.Sid))
                    continue;

                if (!inRange)
                {
                    if (m.Sid == firstMessageId)
                    {
                        inRange = true;
                        result.Add(new MessageDTO(m.Sid, m.Direction == MessageResource.DirectionEnum.Inbound, m.Body));
                    }
                    continue;
                }

                result.Add(new MessageDTO(m.Sid, m.Direction == MessageResource.DirectionEnum.Inbound, m.Body));
            }

            return result;
        }
    }
}
