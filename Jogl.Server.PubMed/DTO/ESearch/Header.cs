using System.Text.Json.Serialization;

namespace Jogl.Server.PubMed.DTO.ESearch
{
    public class Header
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
