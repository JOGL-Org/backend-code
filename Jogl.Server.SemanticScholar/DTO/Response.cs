using System.Text.Json.Serialization;

namespace Jogl.Server.SemanticScholar.DTO
{
    public class Response<T>
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("next")]
        public int Next { get; set; }

        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }
}
