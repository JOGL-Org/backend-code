using System.Text.Json.Serialization;

namespace Jogl.Server.PubMed.DTO.ESearch
{
    public class ESearchResult
    {
        [JsonPropertyName("count")]
        public string Count { get; set; }

        [JsonPropertyName("retmax")]
        public string RetMax { get; set; }

        [JsonPropertyName("retstart")]
        public string RetStart { get; set; }

        [JsonPropertyName("querykey")]
        public string QueryKey { get; set; }

        [JsonPropertyName("webenv")]
        public string WebEnv { get; set; }

        [JsonPropertyName("idlist")]
        public List<string> IdList {get; set; }
    }
}

