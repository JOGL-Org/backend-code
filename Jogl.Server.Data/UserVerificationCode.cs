namespace Jogl.Server.Data
{
    public enum VerificationAction { Verify, PasswordReset, OneTimeLogin }
    public class UserVerificationCode : Entity
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string Code { get; set; }
        public string RedirectURL { get; set; }
        public VerificationAction Action { get; set; }
        public DateTime ValidUntilUTC { get; set; }
    }
}