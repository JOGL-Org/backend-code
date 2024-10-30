using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserCreateWalletModel
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        [EmailAddress]
        public string Email { get; set; }

        [JsonPropertyName("wallet")]
        public string Wallet { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }

        [JsonPropertyName("terms_confirmation")]
        public bool TermsConfirmation { get; set; }

        [JsonPropertyName("age_confirmation")]
        public bool AgeConfirmation { get; set; }
    }
}