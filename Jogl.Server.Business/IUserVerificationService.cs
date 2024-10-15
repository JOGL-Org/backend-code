using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface IUserVerificationService
    {
        Task<string> CreateAsync(User user, VerificationAction action, bool notify);
        VerificationStatus GetVerificationStatus (string userEmail, VerificationAction action, string code);
        Task<VerificationStatus> VerifyAsync (string userEmail, VerificationAction action, string code);
    }
}