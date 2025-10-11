using Jogl.Server.Telegram.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Jogl.Server.Telegram
{
    public class TelegramVerifier : ITelegramVerifier
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelegramVerifier> _logger;

        public TelegramVerifier(IConfiguration configuration, ILogger<TelegramVerifier> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> VerifyPayloadAsync(TelegramVerificationPayload payload)
        {
            var data = GetTelegramData(payload);
            if (!data.ContainsKey("hash"))
            {
                throw new ArgumentException("Hash is missing from auth data");
            }

            string receivedHash = data["hash"];

            // Remove hash from data for verification
            var dataToVerify = data
                .Where(x => x.Key != "hash")
                .OrderBy(x => x.Key)
                .ToList();

            // Create data check string
            string dataCheckString = string.Join("\n",
                dataToVerify.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            // Step 1: Hash the bot token with SHA256
            byte[] secretKey;
            using (var sha256 = SHA256.Create())
            {
                secretKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(_configuration["Telegram-BotToken"]));
            }

            // Step 2: Create HMAC-SHA256 with the secret key
            string calculatedHash;
            using (var hmac = new HMACSHA256(secretKey))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
                calculatedHash = BitConverter.ToString(hashBytes)
                    .Replace("-", "")
                    .ToLower();
            }

            // Step 3: Compare hashes
            if (calculatedHash != receivedHash)
            {
                return false;
            }

            // Step 4: Check auth date (prevent replay attacks)
            if (data.ContainsKey("auth_date"))
            {
                if (long.TryParse(data["auth_date"], out long authDate))
                {
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    long timeDifference = currentTime - authDate;

                    // Reject if older than 24 hours (86400 seconds)
                    if (timeDifference > 86400)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private Dictionary<string, string> GetTelegramData(TelegramVerificationPayload payload)
        {
            var authData = new Dictionary<string, string>
            {
                { "id", payload.Id.ToString() },
                { "first_name", payload.FirstName },
                { "auth_date", payload.AuthDate.ToString() },
                { "hash", payload.Hash }
            };

            // Add optional fields if present
            if (!string.IsNullOrEmpty(payload.LastName))
                authData["last_name"] = payload.LastName;
            if (!string.IsNullOrEmpty(payload.Username))
                authData["username"] = payload.Username;
            if (!string.IsNullOrEmpty(payload.PhotoUrl))
                authData["photo_url"] = payload.PhotoUrl;


            return authData;
        }
    }
}