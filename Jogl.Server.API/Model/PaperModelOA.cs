using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class PaperModelOA : ExternalPaperModel
    {
        [JsonPropertyName("open_access_pdf")]
        public string OpenAccessPdfUrl { get; set; }

        [JsonPropertyName("external_id_url")]
        public string ExternalIdUrl { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
    }
}