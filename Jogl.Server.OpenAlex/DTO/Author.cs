using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Author
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("orcid")]
        public string Orcid { get; set; }
    }
}
