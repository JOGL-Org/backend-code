using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public class Comment : Entity
    {
        public string FeedId { get; set; }
        public string ContentEntityId { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
        public string ReplyToId { get; set; }
        public string? ExternalID { get; set; }
        public string? ExternalSourceID { get; set; }
        public DiscussionItemOverrides Overrides { get; set; }
       
        [BsonIgnore]
        public FeedEntity FeedEntity { get; set; }

        [BsonIgnore]
        public ContentEntity ContentEntity { get; set; }

        [BsonIgnore]
        public List<Document> Documents { get; set; }

        [BsonIgnore]
        public int UserMentions { get; set; }

        [BsonIgnore]
        public bool IsNew { get; set; }

        [BsonIgnore]
        public List<Mention> Mentions { get; set; }

        [BsonIgnore]
        public List<Document> DocumentsToAdd { get; set; }

        [BsonIgnore]
        public List<string> DocumentsToDelete { get; set; }

        [BsonIgnore]
        public List<Reaction> Reactions { get; set; }

        [BsonIgnore]
        public Reaction UserReaction { get; set; }

        [BsonIgnore]
        public string NodeId { get; set; }
    }
}