using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FeedIntegrationModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public FeedIntegrationType Type { get; set; }

        [JsonPropertyName("source_id")]
        public string SourceId { get; set; }

        [JsonPropertyName("source_url")]
        public string SourceUrl { get; set; }
    }
}