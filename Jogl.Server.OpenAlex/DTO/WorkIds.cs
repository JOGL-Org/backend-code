using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class WorkIds
    {
        [JsonPropertyName("mag")]
        public string MAG { get; set; }

        [JsonPropertyName("pmid")]
        public string PubMed { get; set; }

        [JsonPropertyName("pmcid")]
        public string PubMedCentral { get; set; }
    }
}
