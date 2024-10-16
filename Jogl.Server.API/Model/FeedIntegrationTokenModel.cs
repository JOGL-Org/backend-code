using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FeedIntegrationTokenModel
    {
        [JsonPropertyName("type")]
        public FeedIntegrationType Type { get; set; }

        [JsonPropertyName("authorization_code")]
        public string? AuthorizationCode { get; set; }
    }
}