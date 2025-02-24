using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Author
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("orcid")]
        public string Orcid { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("display_name_alternatives")]
        public List<string> DisplayNameAlternatives { get; set; }

        [JsonPropertyName("relevance_score")]
        public double RelevanceScore { get; set; }

        [JsonPropertyName("works_count")]
        public int WorksCount { get; set; }

        [JsonPropertyName("cited_by_count")]
        public int CitedByCount { get; set; }

        [JsonPropertyName("summary_stats")]
        public SummaryStats SummaryStats { get; set; }

        [JsonPropertyName("ids")]
        public AuthorIds Ids { get; set; }

        [JsonPropertyName("affiliations")]
        public List<Affiliation> Affiliations { get; set; }

        [JsonPropertyName("last_known_institutions")]
        public List<Institution> LastKnownInstitutions { get; set; }

        [JsonPropertyName("topics")]
        public List<Topic> Topics { get; set; }

        [JsonPropertyName("topic_share")]
        public List<TopicShare> TopicShare { get; set; }

        [JsonPropertyName("x_concepts")]
        public List<XConcept> XConcepts { get; set; }

        [JsonPropertyName("counts_by_year")]
        public List<CountsByYear> CountsByYear { get; set; }

        [JsonPropertyName("works_api_url")]
        public string WorksApiUrl { get; set; }

        [JsonPropertyName("updated_date")]
        public DateTime UpdatedDate { get; set; }

        [JsonPropertyName("created_date")]
        public string CreatedDate { get; set; }
    }
}