namespace Jogl.Server.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, EmailTemplate template, object data, string replyTo = null, string fromName = null);
        Task SendEmailAsync(Dictionary<string, object> toAndData, EmailTemplate template, string replyTo = null, string fromName = null);
    }
}