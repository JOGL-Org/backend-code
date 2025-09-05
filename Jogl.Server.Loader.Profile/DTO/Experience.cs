using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class Experience
    {
        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("position")]
        public string Position { get; set; }

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
