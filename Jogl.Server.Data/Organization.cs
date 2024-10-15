using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class Organization : CommunityEntity
    {
        public string Address { get; set; }

        [BsonIgnore]
        public override CommunityEntityType Type => CommunityEntityType.Organization;

        [BsonIgnore]
        public override FeedType FeedType => FeedType.Organization;
    }
}