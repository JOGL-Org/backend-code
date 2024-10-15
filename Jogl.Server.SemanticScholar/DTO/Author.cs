using System.Text.Json.Serialization;

namespace Jogl.Server.SemanticScholar.DTO
{
    public class Author
    {
        [JsonPropertyName("authorId")]
        public string AuthorId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("papers")]
        public List<SemanticPaper> Papers { get; set; }
    }
}
