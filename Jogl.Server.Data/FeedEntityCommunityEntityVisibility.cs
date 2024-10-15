using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public class FeedEntityCommunityEntityVisibility
    {
        public string CommunityEntityId { get; set; }
        public FeedEntityVisibility Visibility { get; set; }

        [BsonIgnore]
        public CommunityEntity CommunityEntity { get; set; }
    }
}