using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum DiscussionItemSource { Post, Reply, Mention }

    [BsonIgnoreExtraElements]
    public class DiscussionItem : Entity
    {
        public string FeedId { get; set; }
        public string ContentEntityId { get; set; }
        public bool IsReply { get; set; }
        public string Text { get; set; }
        public string ReplyToText { get; set; }
        public DiscussionItemOverrides Overrides { get; set; }

        [BsonIgnore]
        public FeedEntity FeedEntity { get; set; }

        [BsonIgnore]
        public bool IsNew { get; set; }

        [BsonIgnore]
        public DiscussionItemSource Source { get; set; }
    }
}