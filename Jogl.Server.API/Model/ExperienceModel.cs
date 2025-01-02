using System.Text.Json.Serialization;

namespace Jogl.Server.LinkedIn.DTO
{
    public class ExperienceModel
    {
        [JsonPropertyName("organization")]
        public string Organization { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("from")]
        public string? From { get; set; }

        [JsonPropertyName("to")]
        public string? To { get; set; }
    }
}
