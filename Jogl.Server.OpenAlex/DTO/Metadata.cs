using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Metadata
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("db_response_time_ms")]
        public int DbResponseTimeMs { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }

        [JsonPropertyName("groups_count")]
        public object GroupsCount { get; set; }
    }
}
