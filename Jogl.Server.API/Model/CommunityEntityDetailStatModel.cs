using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CommunityEntityDetailStatModel : CommunityEntityStatModel
    {
        [JsonPropertyName("needs_count_aggregate")]
        public int NeedCountAggregate { get; set; }

        [JsonPropertyName("documents_count")]
        public int DocumentCount { get; set; }

        [JsonPropertyName("documents_count_aggregate")]
        public int DocumentCountAggregate { get; set; }

        [JsonPropertyName("papers_count")]
        public int PaperCount { get; set; }

        [JsonPropertyName("papers_count_aggregate")]
        public int PaperCountAggregate { get; set; }

        [JsonPropertyName("resources_count")]
        public int ResourceCount { get; set; }

        [JsonPropertyName("resources_count_aggregate")]
        public int ResourceCountAggregate { get; set; }

        [JsonPropertyName("proposal_count")]
        public int ProposalCount { get; set; }
    }
}