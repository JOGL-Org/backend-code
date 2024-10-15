using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class AuthResultModel
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }
    }
}