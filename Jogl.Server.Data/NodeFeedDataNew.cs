using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public class NodeFeedDataNew : Node
    {
        [BsonIgnore]
        public List<CommunityEntity> Entities { get; set; }

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
