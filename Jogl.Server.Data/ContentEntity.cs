using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum ContentEntityFilter { Posts, Mentions, Threads }
    public enum ContentEntityType { Announcement, JoglDoc, Preprint, Article, Protocol, Need, Paper }
    public enum ContentEntityStatus { Active, Draft }
    public enum ContentEntityVisibility { Public, Entity, Event, Ecosystem, Authors }

    [BsonIgnoreExtraElements]
    public class ContentEntity : Entity
    {
        public string FeedId { get; set; }
        public ContentEntityType Type { get; set; }
        public ContentEntityStatus Status { get; set; }
        [RichText]
        public string Text { get; set; }
        public string? ExternalID { get; set; }
        public string? ExternalSourceID { get; set; }
        public DiscussionItemOverrides Overrides { get; set; }

        [BsonIgnore]
        public FeedEntity FeedEntity { get; set; }
        [BsonIgnore]
        public List<Document> Documents { get; set; }
        [BsonIgnore]
        public List<Reaction> Reactions { get; set; }
        [BsonIgnore]
        public Reaction UserReaction { get; set; }
        [BsonIgnore]
        public int CommentCount { get; set; }
        [BsonIgnore]
        public int NewCommentCount { get; set; }
        [BsonIgnore]
        public int UserMentions { get; set; }

        [BsonIgnore]
        public int UserMentionsInComments { get; set; }

        [BsonIgnore]
        public bool IsNew { get; set; }

        [BsonIgnore]
        public List<Mention> Mentions { get; set; }

        [BsonIgnore]
        public List<Document> DocumentsToAdd { get; set; }

        [BsonIgnore]
        public List<string> DocumentsToDelete { get; set; }

        [BsonIgnore]
        public List<User> Users { get; set; }
    }
}