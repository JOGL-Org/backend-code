using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class DiscussionModel : ListPage<ContentEntityModel>
    {
        public DiscussionModel(IEnumerable<ContentEntityModel> items) : base(items)
        {
        }

        [JsonPropertyName("unread_posts")]
        public int UnreadPosts { get; set; }

        [JsonPropertyName("unread_mentions")]
        public int UnreadMentions { get; set; }

        [JsonPropertyName("unread_threads")]
        public int UnreadThreads { get; set; }

        [JsonPropertyName("permissions")]
        public List<Permission> Permissions { get; set; }

        [JsonPropertyName("feed_entity")]
        public EntityMiniModel? FeedEntity { get; set; }

        [JsonPropertyName("parent_feed_entity")]
        public EntityMiniModel? ParentFeedEntity { get; set; }
    }
}