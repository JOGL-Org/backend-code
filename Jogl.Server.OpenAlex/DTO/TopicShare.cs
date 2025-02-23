using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class TopicShare
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("subfield")]
        public Subfield Subfield { get; set; }

        [JsonPropertyName("field")]
        public Field Field { get; set; }

        [JsonPropertyName("domain")]
        public Domain Domain { get; set; }
    }
}