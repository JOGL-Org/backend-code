using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class NodeFeedDataModel : CommunityEntityMiniModel
    {
        [JsonPropertyName("feeds")]
        public List<UserFeedRecordModel> Feeds { get; set; }

        [JsonPropertyName("new_events")]
        public bool NewEvents { get; set; }

        [JsonPropertyName("new_needs")]
        public bool NewNeeds { get; set; }

        [JsonPropertyName("new_documents")]
        public bool NewDocuments { get; set; }

        [JsonPropertyName("new_papers")]
        public bool NewPapers { get; set; }

        [JsonPropertyName("unread_posts_total")]
        public int UnreadPostsTotal { get; set; }

        [JsonPropertyName("unread_mentions_total")]
        public int UnreadMentionsTotal { get; set; }

        [JsonPropertyName("unread_threads_total")]
        public int UnreadThreadsTotal { get; set; }

        [JsonPropertyName("unread_posts_events")]
        public int UnreadPostsInEvents { get; set; }

        [JsonPropertyName("unread_mentions_events")]
        public int UnreadMentionsInEvents { get; set; }

        [JsonPropertyName("unread_threads_events")]
        public int UnreadThreadsInEvents { get; set; }

        [JsonPropertyName("unread_posts_needs")]
        public int UnreadPostsInNeeds { get; set; }

        [JsonPropertyName("unread_mentions_needs")]
        public int UnreadMentionsInNeeds { get; set; }

        [JsonPropertyName("unread_threads_needs")]
        public int UnreadThreadsInNeeds { get; set; }

        [JsonPropertyName("unread_posts_documents")]
        public int UnreadPostsInDocuments { get; set; }

        [JsonPropertyName("unread_mentions_documents")]
        public int UnreadMentionsInDocuments { get; set; }

        [JsonPropertyName("unread_threads_documents")]
        public int UnreadThreadsInDocuments { get; set; }

        [JsonPropertyName("unread_posts_papers")]
        public int UnreadPostsInPapers { get; set; }

        [JsonPropertyName("unread_mentions_papers")]
        public int UnreadMentionsInPapers { get; set; }

        [JsonPropertyName("unread_threads_papers")]
        public int UnreadThreadsInPapers { get; set; }


    }
}