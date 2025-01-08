using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.Email
{
    public enum EmailTemplate { UserVerification, PasswordReset, InvitationWithEmail, InvitationWithUser, Request, Message, CFPMessage, EventMessage, ContactDemo, ObjectAdded, ContentEntityAddedInChannel, ContentEntityAddedInObject, CommentAdded, MentionCreated, ObjectShared, ObjectDeleted }
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public SendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetLanguage(object payload)
        {
            return payload.GetType().GetProperty("LANGUAGE")?.GetValue(payload)?.ToString() ?? "en";
        }

        public async Task SendEmailAsync(string to, EmailTemplate template, object data, string replyTo = null, string fromName = null)
        {
            if (string.IsNullOrEmpty(to))
                throw new ArgumentNullException(nameof(to));

            var whitelist = new string[] { "@ibercivis.es", "@jogl.io" };
            var suppressExternalEmails = bool.Parse(_configuration["App:SuppressExternalEmails"]);
            if (suppressExternalEmails && !whitelist.Any(domain => to.EndsWith(domain, StringComparison.InvariantCultureIgnoreCase)))
                return;

            var apiKey = _configuration["Email:Key"];
            var client = new SendGridClient(apiKey);
            var emailFrom = new EmailAddress(_configuration["Email:From:Address"], fromName ?? _configuration["Email:From:Name"]);
            var emailTo = new EmailAddress(to);
            var msg = MailHelper.CreateSingleTemplateEmail(emailFrom, emailTo, GetTemplateId(template, GetLanguage(data)), data);
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
            var whitelist = new string[] { "@ibercivis.es", "@jogl.io" };
            var suppressExternalEmails = bool.Parse(_configuration["App:SuppressExternalEmails"]);
            if (suppressExternalEmails)
            {
                foreach (var email in toAndData.Keys.ToList())
                {
                    if (!whitelist.Any(domain => email.EndsWith(domain, StringComparison.InvariantCultureIgnoreCase)))
                        toAndData.Remove(email);
                }
            }

            if (!toAndData.Any())
                return;

            var apiKey = _configuration["Email:Key"];
            var client = new SendGridClient(apiKey);
            var emailFrom = new EmailAddress(_configuration["Email:From:Address"], fromName != null ? $"{fromName} via JOGL" : _configuration["Email:From:Name"]);
            var emailsTo = toAndData.Keys.Select(email => new EmailAddress(email)).ToList();
            foreach (var grp in toAndData.Values.GroupBy(GetLanguage))
            {
                var msg = MailHelper.CreateMultipleTemplateEmailsToMultipleRecipients(emailFrom, emailsTo, GetTemplateId(template, grp.Key), toAndData.Values.ToList());
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
        }

        private string GetTemplateId(EmailTemplate template, string language)
        {
            //this is absolutely horrible and needs to be rewritten at some point
            switch (language)
            {
                case "fr":
                    switch (template)
                    {
                        case EmailTemplate.UserVerification:
                            return "d-73e3af0b525c491181b2ee8370b4c2d1";
                        case EmailTemplate.PasswordReset:
                            return "d-372da9f90ae642059d0391f6fd681fe4";
                        case EmailTemplate.InvitationWithEmail:
                            return "d-1eee8a128b7146cbbf63a834175a5461";
                        case EmailTemplate.InvitationWithUser:
                            return "d-2848efcca7bb44029010643776605c41";
                        case EmailTemplate.Request:
                            return "d-4fa0302b877746feadf75c68c123a5c2";
                        case EmailTemplate.Message:
                            return "d-c52fbf44c6704526b5a81cc61a077470";
                        case EmailTemplate.CFPMessage:
                            return "d-0a1043f603624097a23a315e1befb1ed";
                        case EmailTemplate.EventMessage:
                            return "d-2380e22ed36e45a0a4c0f116b81bf069";
                        case EmailTemplate.ContactDemo:
                            return "d-2609b925ce56498bbbcdd2ce47d3fb68";
                        case EmailTemplate.ObjectAdded:
                            return "d-f2f9c8679a7e4116a6af5b7c9b15221a";
                        case EmailTemplate.ObjectShared:
                            return "d-e9311b29b2294992b03571394c325030";
                        case EmailTemplate.ObjectDeleted:
                            return "d-084f3ea40a1a4c7da36011d769fde5b3";
                        case EmailTemplate.ContentEntityAddedInChannel:
                            return "d-cc6e0693bbca491987740d2ad0ec6705";
                        case EmailTemplate.ContentEntityAddedInObject:
                            return "d-db5972fe5ded48958571108e9e8b53eb";
                        case EmailTemplate.CommentAdded:
                            return "d-7fc56ff601c9411592f20fb0eb27025d";
                        case EmailTemplate.MentionCreated:
                            return "d-a2bad3a3c8704fc9a491b056270bfcf1";

                        default:
                            throw new Exception($"Cannot determine template id for template {template}");
                    }
                default:
                    switch (template)
                    {
                        case EmailTemplate.UserVerification:
                            return "d-8768fb8587ff41cb88b871b263fb9fee";
                        case EmailTemplate.PasswordReset:
                            return "d-e8044a66f0704411a761f7554e35d3fd";
                        case EmailTemplate.InvitationWithEmail:
                            return "d-02466968f9444988b8b8e630b37cbd8f";
                        case EmailTemplate.InvitationWithUser:
                            return "d-66cae35519aa4acbaa169834f93591ff";
                        case EmailTemplate.Request:
                            return "d-685fdd41ad4f4c01bf8ba33472127742";
                        case EmailTemplate.Message:
                            return "d-58225124b42242489618cfdad4de65ed";
                        case EmailTemplate.CFPMessage:
                            return "d-484994274b0d4fbab27c5231fc9056ae";
                        case EmailTemplate.EventMessage:
                            return "d-53ae8fe88db6448fb432045c557ffff1";
                        case EmailTemplate.ContactDemo:
                            return "d-7bcdf7ce315e4d9a8408e49cade752f8";
                        case EmailTemplate.ObjectAdded:
                            return "d-8e8440338abd41288faeb2c22d4a683d";
                        case EmailTemplate.ObjectShared:
                            return "d-4136ef0bc6234f58bfb29b825aa8f767";
                        case EmailTemplate.ObjectDeleted:
                            return "d-e27d71166e284ae0990611502706c6af";
                        case EmailTemplate.ContentEntityAddedInChannel:
                            return "d-75a732f77d5b47f898074c5e1eb95eb1";
                        case EmailTemplate.ContentEntityAddedInObject:
                            return "d-ca1cdff621ba4d3099f03951e63eb72a";
                        case EmailTemplate.CommentAdded:
                            return "d-675268e56fc64373b70d75ad0603e93d";
                        case EmailTemplate.MentionCreated:
                            return "d-4a328cd281a24346bd826f412af30c9f";


                        default:
                            throw new Exception($"Cannot determine template id for template {template}");
                    }
            }
        }
    }
}