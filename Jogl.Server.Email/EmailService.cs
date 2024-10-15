using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.Email
{
    public enum EmailTemplate { UserVerification, PasswordReset, OneTimeLogin, Invitation, InvitationWithEmail, Request, Message, CFPMessage, UserOnboarding, DailyDigest, WeeklyDigest, EventMessage, ContactDemo, ObjectAdded, ContentEntityAddedInContainer, ContentEntityAddedInObject, UserInvitedToContainer, CommentAdded, MentionCreated, Test, ObjectShared, ObjectDeleted }
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public SendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, EmailTemplate template, object data, string replyTo = null, string fromName = null)
        {
            if (string.IsNullOrEmpty(to))
                throw new ArgumentNullException(nameof(to));

            var suppressExternalEmails = bool.Parse(_configuration["App:SuppressExternalEmails"]);
            if (suppressExternalEmails && !to.EndsWith("@jogl.io", StringComparison.InvariantCultureIgnoreCase))
                return;

            var apiKey = _configuration["Email:Key"];
            var client = new SendGridClient(apiKey);
            var emailFrom = new EmailAddress(_configuration["Email:From:Address"], fromName ?? _configuration["Email:From:Name"]);
            var emailTo = new EmailAddress(to);
            var msg = MailHelper.CreateSingleTemplateEmail(emailFrom, emailTo, GetTemplateId(template), data);
            if (!string.IsNullOrEmpty(_configuration["Email:BCC"]))
                msg.AddBcc(_configuration["Email:BCC"]);
            if (!string.IsNullOrEmpty(replyTo))
                msg.ReplyTo = new EmailAddress(replyTo);

            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                var str = await response.Body.ReadAsStringAsync();
                throw new Exception(str);
            }
        }

        public async Task SendEmailAsync(Dictionary<string, object> toAndData, EmailTemplate template, string replyTo = null, string fromName = null)
        {
            var suppressExternalEmails = bool.Parse(_configuration["App:SuppressExternalEmails"]);
            if (suppressExternalEmails)
            {
                foreach (var email in toAndData.Keys.ToList())
                {
                    if (!email.EndsWith("@jogl.io", StringComparison.InvariantCultureIgnoreCase))
                        toAndData.Remove(email);
                }
            }

            if (!toAndData.Any())
                return;

            var apiKey = _configuration["Email:Key"];
            var client = new SendGridClient(apiKey);
            var emailFrom = new EmailAddress(_configuration["Email:From:Address"], fromName != null ? $"{fromName} via JOGL" : _configuration["Email:From:Name"]);
            var emailsTo = toAndData.Keys.Select(email => new EmailAddress(email)).ToList();
            var msg = MailHelper.CreateMultipleTemplateEmailsToMultipleRecipients(emailFrom, emailsTo, GetTemplateId(template), toAndData.Values.ToList());
            foreach (var p in msg.Personalizations)
            {
                p.Bccs = new List<EmailAddress> { new EmailAddress(_configuration["Email:BCC"]) };
            }

            if (!string.IsNullOrEmpty(replyTo))
                msg.ReplyTo = new EmailAddress(replyTo);

            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
        }

        private string GetTemplateId(EmailTemplate template)
        {
            switch (template)
            {
                case EmailTemplate.UserVerification:
                    return "d-8768fb8587ff41cb88b871b263fb9fee";
                case EmailTemplate.PasswordReset:
                    return "d-e8044a66f0704411a761f7554e35d3fd";
                case EmailTemplate.OneTimeLogin:
                    return "d-a3b91b79b80b4eee8951a922fe4c48ad";
                case EmailTemplate.Invitation:
                    return "d-3a55637792294ea09df3ef5f68cd1864";
                case EmailTemplate.InvitationWithEmail:
                    return "d-02466968f9444988b8b8e630b37cbd8f";
                case EmailTemplate.Request:
                    return "d-685fdd41ad4f4c01bf8ba33472127742";
                case EmailTemplate.Message:
                    return "d-58225124b42242489618cfdad4de65ed";
                case EmailTemplate.CFPMessage:
                    return "d-484994274b0d4fbab27c5231fc9056ae";
                case EmailTemplate.UserOnboarding:
                    return "d-84d7a53520554e6ab34e7513b8486a5a";
                case EmailTemplate.DailyDigest:
                    return "d-c38f90cc3bf741b5ad9d72f385828064";
                case EmailTemplate.WeeklyDigest:
                    return "d-d69d241d91394fb9a7c76b794239b7bb";
                case EmailTemplate.EventMessage:
                    return "d-53ae8fe88db6448fb432045c557ffff1";
                case EmailTemplate.ContactDemo:
                    return "d-84d7a53520554e6ab34e7513b8486a5a";
                //new templates start here
                case EmailTemplate.ObjectAdded:
                    return "d-8e8440338abd41288faeb2c22d4a683d";
                case EmailTemplate.ObjectDeleted:
                    return "d-e27d71166e284ae0990611502706c6af";
                case EmailTemplate.UserInvitedToContainer:
                    return "d-66cae35519aa4acbaa169834f93591ff";
                case EmailTemplate.ContentEntityAddedInContainer:
                    return "d-75a732f77d5b47f898074c5e1eb95eb1";
                case EmailTemplate.ContentEntityAddedInObject:
                    return "d-ca1cdff621ba4d3099f03951e63eb72a";
                case EmailTemplate.CommentAdded:
                    return "d-675268e56fc64373b70d75ad0603e93d";
                case EmailTemplate.MentionCreated:
                    return "d-4a328cd281a24346bd826f412af30c9f";
                case EmailTemplate.Test:
                    return "d-e32ae102ddc444db9a7520b6a344274b";
                case EmailTemplate.ObjectShared:
                    return "d-4136ef0bc6234f58bfb29b825aa8f767";

                default:
                    throw new Exception($"Cannot determine template id for template {template}");
            }
        }
    }
}