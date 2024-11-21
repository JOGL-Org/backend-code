using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Keyword
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }
}