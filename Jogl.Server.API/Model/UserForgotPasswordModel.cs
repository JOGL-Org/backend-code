using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserForgotPasswordModel
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("new_password")]
        public string NewPassword { get; set; }
    }
}