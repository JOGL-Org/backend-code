using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class VerificationStartModel
    {
        [JsonPropertyName("email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}