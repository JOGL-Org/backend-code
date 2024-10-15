using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ResourceUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("image_id")]
        public string? ImageId { get; set; }
    }
}