using System.Text.Json.Serialization;

namespace Jogl.Server.SemanticScholar.DTO
{
    public class Journal
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("pages")]
        public string Pages { get; set; }

        [JsonPropertyName("volume")]
        public string Volume { get; set; }
    }
}
