using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Bibliography
    {
        [JsonPropertyName("volume")]
        public string Volume { get; set; }

        [JsonPropertyName("issue")]
        public string Issue { get; set; }

        [JsonPropertyName("first_page")]
        public string FirstPage { get; set; }

        [JsonPropertyName("last_page")]
        public string LastPage { get; set; }
    }
}
