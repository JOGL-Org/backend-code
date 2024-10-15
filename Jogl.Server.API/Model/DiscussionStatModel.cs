using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class DiscussionStatModel
    {
        [JsonPropertyName("unread_posts")]
        public int UnreadPosts { get; set; }

        [JsonPropertyName("unread_mentions")]
        public int UnreadMentions { get; set; }

        [JsonPropertyName("unread_threads")]
        public int UnreadThreads { get; set; }
    }
}