using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ActivityRecordModel
    {
        [JsonPropertyName("community_entity")]
        public CommunityEntityMiniModel CommunityEntity { get; set; }

        [JsonPropertyName("content_entity")]
        public ContentEntityModel ContentEntity { get; set; }

        [JsonPropertyName("comment")]
        public CommentExtendedModel Comment { get; set; }

        [JsonPropertyName("reaction")]
        public ReactionExtendedModel Reaction { get; set; }
    }
}