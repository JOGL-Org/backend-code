using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class AuthorizationCodeModel
    {
        [JsonPropertyName("authorization_code")]
        public string AuthorizationCode { get; set; }
    }
}