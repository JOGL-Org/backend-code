using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OrganizationDetailModel : OrganizationModel
    {
        [JsonIgnore]
        public override CommunityEntityStatModel Stats { get; set; }

        [JsonPropertyName("stats")]
        public CommunityEntityDetailStatModel DetailStats { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }
    }
}
