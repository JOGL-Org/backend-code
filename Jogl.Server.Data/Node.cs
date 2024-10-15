using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class Node : CommunityEntity
    {
        public string Website { get; set; }
        public List<FAQItem> FAQ { get; set; }

        [BsonIgnore]
        public override CommunityEntityType Type => CommunityEntityType.Node;

        [BsonIgnore]
        public override FeedType FeedType => FeedType.Node;
    }
}