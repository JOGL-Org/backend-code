using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CallForProposalDetailModel : CallForProposalModel
    {
        [JsonIgnore]
        public override CommunityEntityStatModel Stats { get; set; }

        [JsonIgnore]
        public override CallForProposalStatModel CFPStats { get; set; }

        [JsonPropertyName("stats")]
        public CallForProposalDetailStatModel CFPDetailStats { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }
    }
}
