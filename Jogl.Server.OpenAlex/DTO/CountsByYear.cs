using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class CountsByYear
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("works_count")]
        public int WorksCount { get; set; }

        [JsonPropertyName("cited_by_count")]
        public int CitedByCount { get; set; }
    }
}