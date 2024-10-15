using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserMembershipInfoModel
    {
        [JsonPropertyName("project_count")]
        public int ProjectCount { get; set; }

        [JsonPropertyName("community_count")]
        public int CommunityCount { get; set; }

        [JsonPropertyName("node_count")]
        public int NodeCount { get; set; }

        [JsonPropertyName("need_count")]
        public int NeedCount { get; set; }
    }
}