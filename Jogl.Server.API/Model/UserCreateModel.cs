using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserCreateModel
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        [EmailAddress]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("newsletter")]
        public bool MailNewsletter { get; set; }

        [JsonPropertyName("terms_confirmation")]
        public bool TermsConfirmation { get; set; }

        [JsonPropertyName("age_confirmation")]
        public bool AgeConfirmation { get; set; }

        [JsonPropertyName("verification_code")]
        public string? VerificationCode { get; set; }

        [JsonPropertyName("redirect_url")]
        public string? RedirectURL { get; set; }

        [JsonPropertyName("captcha_verification_token")]
        public string? CaptchaVerificationToken { get; set; }
    }
}