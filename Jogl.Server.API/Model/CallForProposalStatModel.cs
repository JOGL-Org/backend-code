using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CallForProposalStatModel : CommunityEntityStatModel
    {
        [JsonPropertyName("proposals_count")]
        public int ProposalCount { get; set; }

        [JsonPropertyName("submitted_proposals_count")]
        public int SubmittedProposalCount { get; set; }
    }
}