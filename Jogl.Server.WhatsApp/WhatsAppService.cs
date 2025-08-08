using Microsoft.Extensions.Configuration;
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
            foreach (var chunk in SplitString(message, 1600))
            {
                var msg = await MessageResource.CreateAsync(
                  body: chunk,
                  from: new Twilio.Types.PhoneNumber($"whatsapp:{_configuration["Twilio:Number"]}"),
                  to: new Twilio.Types.PhoneNumber($"whatsapp:{number}"));

            }

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
    }
}
