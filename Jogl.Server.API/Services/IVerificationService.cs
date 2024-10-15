namespace Jogl.Server.API.Services
{
    public interface IVerificationService
    {
        Task<bool> VerifyAsync(string token, string action);
    }
}
