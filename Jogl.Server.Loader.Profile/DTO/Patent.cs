using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class Patent
    {
        [JsonPropertyName("family_id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("abstract")]
        public string Abstract { get; set; }

        [JsonPropertyName("inventors")]
        public List<string> Inventors { get; set; }

        [JsonPropertyName("claims")]
        public List<string> Claims { get; set; }

        [JsonPropertyName("priority_date")]
        public string PriorityDate { get; set; }

        [JsonPropertyName("applicant")]
        public string Applicant { get; set; }

        [JsonPropertyName("references")]
        public List<Reference> References { get; set; }

        [JsonPropertyName("jurisdictions")]
        public List<object> Jurisdictions { get; set; }

        [JsonPropertyName("status_summary")]
        public string StatusSummary { get; set; }
    }
}
