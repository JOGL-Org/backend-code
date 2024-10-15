using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ProjectDetailModel : ProjectModel
    {
        [JsonIgnore]
        public override CommunityEntityStatModel Stats { get; set; }

        [JsonPropertyName("stats")]
        public CommunityEntityDetailStatModel DetailStats { get; set; }
    }
}
