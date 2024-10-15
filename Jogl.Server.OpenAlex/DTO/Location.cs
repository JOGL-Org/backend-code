using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Location
    {
        [JsonPropertyName("is_oa")]
        public bool IsOa { get; set; }

        [JsonPropertyName("landing_page_url")]
        public string LandingPageUrl { get; set; }

        [JsonPropertyName("pdf_url")]
        public string PdfUrl { get; set; }

        [JsonPropertyName("source")]
        public LocationSource Source { get; set; }

        [JsonPropertyName("license")]
        public string License { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("is_accepted")]
        public bool IsAccepted { get; set; }

        [JsonPropertyName("is_published")]
        public bool IsPublished { get; set; }
    }
}
