using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class UserFeedRecord : Entity
    {
        public string UserId { get; set; }
        public string FeedId { get; set; }
        public DateTime? LastListedUTC { get; set; }
        public DateTime? LastReadUTC { get; set; }
        public DateTime? LastWriteUTC { get; set; }
        public DateTime? LastMentionUTC { get; set; }
       
        public bool Muted { get; set; }
        public bool Starred { get; set; }

        [BsonIgnore]
        public FeedEntity FeedEntity { get; set; }

        [BsonIgnore]
        public FeedEntity ParentFeedEntity { get; set; }

        [BsonIgnore]
        public int UnreadMentions { get; set; }

        [BsonIgnore]
        public int UnreadPosts { get; set; }

        [BsonIgnore]
        public int UnreadThreads { get; set; }

        [BsonIgnore]
        public bool IsForUser { get; set; }

        [BsonIgnore]
        public bool IsNew { get; set; }
    }
}