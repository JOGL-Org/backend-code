using System.Text.Json.Serialization;

namespace Jogl.Server.PubMed.DTO
{
    public class PMIDResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("responseDate")]
        public string ResponseDate { get; set; }

        [JsonPropertyName("request")]
        public string request { get; set; }

        [JsonPropertyName("records")]
        public List<Record> Records { get; set; }
    }

    public class Record {
        [JsonPropertyName("pmcid")]
        public string PMCID { get; set; }

        [JsonPropertyName("pmid")]
        public string PMID { get; set; }

        [JsonPropertyName("doi")]
        public string DOI { get; set; }
    }
}
