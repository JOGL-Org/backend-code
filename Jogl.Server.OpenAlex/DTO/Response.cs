using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Response<T>
    {
        [JsonPropertyName("meta")]
        public Metadata Meta { get; set; }

        [JsonPropertyName("results")]
        public List<T> Results { get; set; }
    }
}
