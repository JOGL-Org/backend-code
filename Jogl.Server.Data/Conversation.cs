using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public class Conversation : FeedEntity
    {
        public override string FeedTitle => string.Empty;

        public override FeedType FeedType => FeedType.Conversation;

        [BsonIgnore]
        public User User { get; set; }

        [BsonIgnore]
        public ContentEntity LatestMessage { get; set; }
    }
}