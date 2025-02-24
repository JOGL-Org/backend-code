using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class XConcept
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("wikidata")]
        public string Wikidata { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }
}