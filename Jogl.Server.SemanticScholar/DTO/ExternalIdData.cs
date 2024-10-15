using System.Text.Json.Serialization;

namespace Jogl.Server.SemanticScholar.DTO
{
    public class ExternalIdData
    {
        [JsonPropertyName("MAG")]
        public string MAG { get; set; }

        [JsonPropertyName("DOI")]
        public string DOI { get; set; }

        [JsonPropertyName("CorpusId")]
        public int CorpusId { get; set; }

        [JsonPropertyName("PubMed")]
        public string PubMed { get; set; }

        [JsonPropertyName("PubMedCentral")]
        public string PubMedCentral { get; set; }

        [JsonPropertyName("DBLP")]
        public string DBLP { get; set; }
    }
}
