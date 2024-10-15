using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FeedEntityCommunityEntityVisibilityUpsertModel
    {
        [JsonPropertyName("visibility")]
        public FeedEntityVisibility Visibility { get; set; }

        [JsonPropertyName("community_entity_id")]
        public string CommunityEntityId { get; set; }
    }
}