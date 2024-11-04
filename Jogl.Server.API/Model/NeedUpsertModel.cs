using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class NeedUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Skills { get; set; }

        [JsonPropertyName("interests")]
        public List<string>? Interests { get; set; }

        [JsonPropertyName("type")]
        public NeedType? Type { get; set; }

        [JsonPropertyName("default_visibility")]
        public FeedEntityVisibility? DefaultVisibility { get; set; }

        [JsonPropertyName("user_visibility")]
        public List<FeedEntityUserVisibilityUpsertModel>? UserVisibility { get; set; }

        [JsonPropertyName("communityentity_visibility")]
        public List<FeedEntityCommunityEntityVisibilityUpsertModel>? CommunityEntityVisibility { get; set; }

    }
}