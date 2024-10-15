using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Institution
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("ror")]
        public string Ror { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("type")]
        public string type { get; set; }
    }
}
