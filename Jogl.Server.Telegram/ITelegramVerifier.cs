using Jogl.Server.Telegram.DTO;

namespace Jogl.Server.Telegram
{
    public interface ITelegramVerifier
    {
        Task<bool> VerifyPayloadAsync(TelegramVerificationPayload payload);
    }
}