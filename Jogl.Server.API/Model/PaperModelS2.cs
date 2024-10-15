using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class PaperModelS2 : ExternalPaperModel
    {
        [JsonPropertyName("open_access_pdf")]
        public string OpenAccessPdfUrl { get; set; }
    }
}