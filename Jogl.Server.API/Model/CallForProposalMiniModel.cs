using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CallForProposalMiniModel : CommunityEntityMiniModel
    {
        [JsonPropertyName("submissions_from")]
        public DateTime? SubmissionsFrom { get; set; }

        [JsonPropertyName("submissions_to")]
        public DateTime? SubmissionsTo { get; set; }
    }
}