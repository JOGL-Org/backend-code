using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserFeedRecordModel
    {
        [JsonPropertyName("feed_id")]
        public string FeedId { get; set; }


        [JsonPropertyName("last_read")]
        public DateTime? LastReadUTC { get; set; }

        [JsonPropertyName("feed_entity")]
        public EntityMiniModel FeedEntity { get; set; }

        [JsonPropertyName("parent_feed_entity")]
        public EntityMiniModel ParentFeedEntity { get; set; }

        [JsonPropertyName("muted")]
        public bool Muted { get; set; }

        [JsonPropertyName("starred")]
        public bool Starred { get; set; }

        [JsonPropertyName("mentions")]
        [Obsolete]
        public int Mentions { get => UnreadMentions; }

        [JsonPropertyName("unread_posts")]
        public int UnreadPosts { get; set; }

        [JsonPropertyName("unread_mentions")]
        public int UnreadMentions { get; set; }

        [JsonPropertyName("unread_threads")]
        public int UnreadThreads { get; set; }

        [JsonPropertyName("is_for_user")]
        public int IsForUser { get; set; }

        [JsonPropertyName("is_new")]
        public int IsNew { get; set; }
    }
}