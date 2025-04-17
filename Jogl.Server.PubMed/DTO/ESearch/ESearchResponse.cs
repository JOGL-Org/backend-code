using System.Text.Json.Serialization;

namespace Jogl.Server.PubMed.DTO.ESearch
{
    public class ESearchResponse
    {
        [JsonPropertyName("header")]
        public Header Header { get; set; }

        [JsonPropertyName("esearchresult")]
        public ESearchResult ESearchResult { get; set; }
    }
}
