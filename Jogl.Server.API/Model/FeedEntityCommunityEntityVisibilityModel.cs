using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FeedEntityCommunityEntityVisibilityModel
    {
        [JsonPropertyName("visibility")]
        public FeedEntityVisibility Visibility { get; set; }

        [JsonPropertyName("community_entity")]
        public CommunityEntityMiniModel CommunityEntity { get; set; }
    }
}