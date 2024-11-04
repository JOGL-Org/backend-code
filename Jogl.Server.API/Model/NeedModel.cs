using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class NeedModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("keywords")]
        public List<string> Skills { get; set; }

        [JsonPropertyName("interests")]
        public List<string> Interests { get; set; }

        [JsonPropertyName("type")]
        public NeedType Type { get; set; }

        [JsonPropertyName("default_visibility")]
        public FeedEntityVisibility? DefaultVisibility { get; set; }

        [JsonPropertyName("user_visibility")]
        public List<FeedEntityUserVisibilityModel>? UserVisibility { get; set; }

        [JsonPropertyName("communityentity_visibility")]
        public List<FeedEntityCommunityEntityVisibilityModel>? CommunityEntityVisibility { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }

        [JsonPropertyName("entity")]
        public CommunityEntityMiniModel CommunityEntity { get; set; }

        [JsonPropertyName("feed_stats")]
        public FeedStatModel FeedStats { get; set; }

        [JsonPropertyName("is_new")]
        public bool IsNew { get; set; }

        [JsonPropertyName("permissions")]
        public List<Permission> Permissions { get; set; }

        [JsonPropertyName("path")]
        public List<EntityMiniModel> Path { get; set; }
    }
}