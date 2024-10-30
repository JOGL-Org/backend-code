using Jogl.Server.Data;

namespace Jogl.Server.Auth
{
    public interface IAuthService
    {
        string GetTokenWithPassword(User user, string password);
        string GetToken(string email);
        string HashPasword(string password, out byte[] salt);
        bool VerifyPassword(string password, string hash, byte[] salt);

        string GetTokenWithSignature(User user, WalletType walletType, string wallet, string signature);
        bool VerifySignature(WalletType walletType, string wallet, string signature);
    }
}