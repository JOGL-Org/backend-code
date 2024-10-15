using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserLoginCodeModel
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}