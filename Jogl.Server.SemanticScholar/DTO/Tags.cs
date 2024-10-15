using System.Text.Json.Serialization;

namespace Jogl.Server.SemanticScholar.DTO
{
    public class SemanticTags
    {
        [JsonPropertyName("paperId")]
        public string PaperId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("s2FieldsOfStudy")]
        public List<FieldsOfStudy> Tags { get; set; }
    }

    public class FieldsOfStudy {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }
    }
}
