using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class Paper
    {
        [JsonPropertyName("doi")]
        public string Doi { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("publication_date")]
        public string PublicationDate { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        //[JsonPropertyName("open_access")]
        //public bool OpenAccess { get; set; }

        [JsonPropertyName("author_list")]
        public string AuthorList { get; set; }

        [JsonPropertyName("topics")]
        public string Topics { get; set; }

        [JsonPropertyName("keywords")]
        public string Keywords { get; set; }

        [JsonPropertyName("mesh")]
        public string Mesh { get; set; }

        [JsonPropertyName("sources")]
        public List<string> Sources { get; set; } = new List<string>();

        [JsonPropertyName("openalex_id")]
        public string? OpenAlexId { get; set; }

        [JsonPropertyName("semantic_scholar_id")]
        public string? SemanticScholarId { get; set; }

        [JsonPropertyName("abstract")]
        public string? Abstract { get; set; }
    }
}
