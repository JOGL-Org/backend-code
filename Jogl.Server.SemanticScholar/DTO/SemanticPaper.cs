using System.Text.Json.Serialization;

namespace Jogl.Server.SemanticScholar.DTO
{
    public class SemanticPaper
    {
        [JsonPropertyName("paperId")]
        public string PaperId { get; set; }

        [JsonPropertyName("externalIds")]
        public ExternalIdData ExternalIds { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("journal")]
        public Journal Journal { get; set; }

        [JsonPropertyName("abstract")]
        public string Abstract { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("citationCount")]
        public int? CitationCount { get; set; }

        [JsonPropertyName("openAccessPdf")]
        public OpenAccessPdf OpenAccessPdf { get; set; }

        [JsonPropertyName("publicationTypes")]
        public List<string> PublicationTypes { get; set; }

        [JsonPropertyName("publicationDate")]
        public string PublicationDate { get; set; }

        [JsonPropertyName("authors")]
        public List<Author> Authors { get; set; }
    }
}
