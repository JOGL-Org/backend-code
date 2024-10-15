using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
   // public enum ReactionOrigin { Comment, ContentEntity }
    
    [BsonIgnoreExtraElements]
    public class Reaction : Entity
    {
        public string FeedId { get; set; }
        public string OriginId { get; set; }
        //public ReactionOrigin OriginType { get; set; }
        public string UserId { get; set; }
        public string Key { get; set; }

        [BsonIgnore]
        public ContentEntity ContentEntity { get; set; }
    }
}