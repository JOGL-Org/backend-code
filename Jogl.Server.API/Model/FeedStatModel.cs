using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FeedStatModel
    {
        [JsonPropertyName("post_count")]
        public int PostCount { get; set; }
        
        [JsonPropertyName("new_post_count")]
        public int NewPostCount { get; set; }

        [JsonPropertyName("new_mention_count")]
        public int NewMentionCount { get; set; }

        [JsonPropertyName("new_thread_activity_count")]
        public int NewThreadActivityCount { get; set; }

        [Obsolete]
        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }
    }
}