using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class Education
    {
        [JsonPropertyName("school")]
        public string School { get; set; }

        [JsonPropertyName("program")]
        public string Program { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("dateFrom")]
        public DateTime? DateFrom { get; set; }

        [JsonPropertyName("dateTo")]
        public DateTime? DateTo { get; set; }

        [JsonPropertyName("current")]
        public bool Current { get; set; }
    }
}
