using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Work
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("doi")]
        public string Doi { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("relevance_score")]
        public double RelevanceScore { get; set; }

        [JsonPropertyName("publication_year")]
        public int? PublicationYear { get; set; }

        [JsonPropertyName("publication_date")]
        public string PublicationDate { get; set; }

        [JsonPropertyName("ids")]
        public Ids Ids { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("primary_location")]
        public Location PrimaryLocation { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("authorships")]
        public List<Authorship> Authorships { get; set; }

        [JsonPropertyName("concepts")]
        public List<Concept> Concepts { get; set; }

        [JsonPropertyName("cited_by_count")]
        public int? CitedByCount { get; set; }

        [JsonPropertyName("biblio")]
        public Bibliography Bibliography { get; set; }

        [JsonPropertyName("best_oa_location")]
        public Location BestOaLocation { get; set; }

        [JsonPropertyName("abstract_inverted_index")]
        public Dictionary<string, List<int>> AbstractInvertedIndex { get; set; }
    }
}
