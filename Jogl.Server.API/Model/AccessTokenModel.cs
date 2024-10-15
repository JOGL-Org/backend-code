using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class AccessTokenModel
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }
}