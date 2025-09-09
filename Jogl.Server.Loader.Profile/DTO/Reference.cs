using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class Reference
    {
        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
