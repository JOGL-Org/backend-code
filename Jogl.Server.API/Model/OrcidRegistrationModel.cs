using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OrcidRegistrationModel
    {
        [JsonPropertyName("authorization_code")]
        public string AuthorizationCode { get; set; }

        [JsonPropertyName("screen")]
        public string Screen { get; set; }
    }
}