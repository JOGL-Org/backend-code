using Jogl.Server.API.Model;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public class DiscussionItemModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("feed_id")]
        public string FeedId { get; set; }
        
        [JsonPropertyName("content_entity_id")]
        public string ContentEntityId { get; set; }
        
        [JsonPropertyName("is_reply")]
        public bool IsReply { get; set; }
    
        [JsonPropertyName("text")]
        public string Text { get; set; }
        
        [JsonPropertyName("reply_to_text")]
        public string ReplyToText { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }

        [JsonPropertyName("overrides")]
        public DiscussionItemOverridesModel? Overrides { get; set; }

        [JsonPropertyName("feed_entity")]
        public EntityMiniModel? FeedEntity { get; set; }

        [JsonPropertyName("is_new")]
        public bool IsNew { get; set; }

        [JsonPropertyName("source")]
        public DiscussionItemSource Source { get; set; }
    }
}