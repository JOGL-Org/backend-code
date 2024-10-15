using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserLoginPasswordModel
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}