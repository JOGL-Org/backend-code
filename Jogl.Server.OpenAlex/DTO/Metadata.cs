using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Metadata
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("perPage")]
        public int PerPage { get; set; }

        [JsonPropertyName("curson")]
        public string Cursor { get; set; }
    }
}
