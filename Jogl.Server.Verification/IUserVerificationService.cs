using Jogl.Server.Data;

namespace Jogl.Server.Verification
{
    public class VerificationResult
    {
        public string RedirectURL { get; set; }
        public VerificationStatus Status { get; set; }
    }
    public interface IUserVerificationService
    {
        Task<string> CreateAsync(User user, VerificationAction action, string redirectURL = null, bool notify = false);
        VerificationStatus GetVerificationStatus(string userEmail, VerificationAction action, string code);
        Task<VerificationResult> VerifyAsync(string userEmail, VerificationAction action, string code);
    }
}