using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OrcidLoadModel
    {
        [JsonPropertyName("authorization_code")]
        public string AuthorizationCode { get; set; }
    }
}