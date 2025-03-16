using Jogl.Server.Data;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ResourceUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("data")]
        public JsonObject? Data { get; set; }

        [JsonPropertyName("default_visibility")]
        public FeedEntityVisibility? DefaultVisibility { get; set; }

        [JsonPropertyName("user_visibility")]
        public List<FeedEntityUserVisibilityUpsertModel>? UserVisibility { get; set; }

        [JsonPropertyName("communityentity_visibility")]
        public List<FeedEntityCommunityEntityVisibilityUpsertModel>? CommunityEntityVisibility { get; set; }
    }
}