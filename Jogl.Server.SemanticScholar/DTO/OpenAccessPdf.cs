using System.Text.Json.Serialization;

namespace Jogl.Server.SemanticScholar.DTO
{
    public class OpenAccessPdf
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("status")]
        public object Status { get; set; }
    }
}
