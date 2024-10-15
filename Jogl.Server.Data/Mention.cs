using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum MentionOrigin { Comment, ContentEntity }
    public enum MentionType { User, Project, Community, Node, Organization }
    public class Mention : Entity
    {
        public MentionOrigin OriginType { get; set; }
        public string OriginId { get; set; }
        public string OriginFeedId { get; set; }
        public FeedType EntityType { get; set; }
        public string EntityId { get; set; }
        public string EntityTitle { get; set; }
        public bool Unread { get; set; }

        [BsonIgnore]
        public FeedEntity OriginFeedEntity { get; set; }

        [BsonIgnore]
        public ContentEntity OriginContentEntity { get; set; }

        [BsonIgnore]
        public Comment OriginComment { get; set; }

        [BsonIgnore]
        public User CreatedByUser { get; set; }
    }
}