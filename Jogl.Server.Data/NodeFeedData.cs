using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public class NodeFeedData : Node
    {
        [BsonIgnore]
        public List<UserFeedRecord> Feeds { get; set; }

        [BsonIgnore]
        public bool NewEvents { get; set; }

        [BsonIgnore]
        public bool NewNeeds { get; set; }

        [BsonIgnore]
        public bool NewDocuments { get; set; }

        [BsonIgnore]
        public bool NewPapers { get; set; }

        [BsonIgnore]
        public int UnreadPostsTotal { get; set; }

        [BsonIgnore]
        public int UnreadMentionsTotal { get; set; }

        [BsonIgnore]
        public int UnreadThreadsTotal { get; set; }

        [BsonIgnore]
        public int UnreadMentionsInEvents { get; set; }

        [BsonIgnore]
        public int UnreadThreadsInEvents { get; set; }

        [BsonIgnore]
        public int UnreadPostsInEvents { get; set; }

        [BsonIgnore]
        public int UnreadMentionsInNeeds { get; set; }

        [BsonIgnore]
        public int UnreadThreadsInNeeds { get; set; }

        [BsonIgnore]
        public int UnreadPostsInNeeds { get; set; }

        [BsonIgnore]
        public int UnreadMentionsInDocuments { get; set; }

        [BsonIgnore]
        public int UnreadThreadsInDocuments { get; set; }

        [BsonIgnore]
        public int UnreadPostsInDocuments { get; set; }

        [BsonIgnore]
        public int UnreadMentionsInPapers { get; set; }

        [BsonIgnore]
        public int UnreadThreadsInPapers { get; set; }

        [BsonIgnore]
        public int UnreadPostsInPapers { get; set; }
    }
}
