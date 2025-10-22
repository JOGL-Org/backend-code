using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class AgentChannelCreateModel
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("nodeId")]
        public string? NodeId { get; set; }
    }
}