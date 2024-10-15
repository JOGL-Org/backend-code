using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ExternalPaperModel
    {

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("abstract")]
        public string Abstract { get; set; }

        [JsonPropertyName("journal")]
        public string Journal { get; set; }

        [JsonPropertyName("publication_date")]
        public string PublicationDate { get; set; }

        [JsonPropertyName("authors")]
        public string Authors { get; set; }

        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; }

        [JsonPropertyName("jogl_id")]
        public string JoglId { get; set; }
    }
}
