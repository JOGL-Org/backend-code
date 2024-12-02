using System.Text.Json.Serialization;
using Jogl.Server.Data;

namespace Jogl.Server.API.Model
{
    public class PaperModelOrcid
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("journal_title")]
        public string JournalTitle { get; set; }

        [JsonPropertyName("abstract")]
        public string Abstract { get; set; }

        [JsonPropertyName("publication_date")]
        public string PublicationDate { get; set; }

        [JsonPropertyName("authors")]
        public string Authors { get; set; }

        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; }

        [JsonPropertyName("external_url")]
        public string ExternalUrl { get; set; }

        [JsonPropertyName("type")]
        public PaperType Type { get; set; }

        [JsonPropertyName("source_name")]
        public string Source { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
    }
}