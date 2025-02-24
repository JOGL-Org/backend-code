
using System.Text.Json.Serialization;
namespace Jogl.Server.OpenAlex.DTO
{
    public class AuthorIds
    {
        [JsonPropertyName("openalex")]
        public string Openalex { get; set; }

        [JsonPropertyName("orcid")]
        public string Orcid { get; set; }

        [JsonPropertyName("scopus")]
        public string Scopus { get; set; }
    }
}