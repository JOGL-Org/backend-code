using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public enum DocumentType { Document, Link, JoglDoc }
    public enum DocumentFilter { Document, File, Media, Link, JoglDoc }

    public class Document : FeedEntity, IFeedEntityOwned
    {
        public string Name { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int FileSize { get; set; }
        public string URL { get; set; }
        public DocumentType Type { get; set; }
        public ContentEntityStatus Status { get; set; }
        [RichText]
        public string? Description { get; set; }
        public string? ImageId { get; set; }
        [Obsolete]
        public string EntityId { get; set; }
        public string FeedId { get; set; }
        public string ContentEntityId { get; set; }
        public string CommentId { get; set; }
        public string ProposalId { get; set; }
        public string FolderId { get; set; }
        public List<string>? Keywords { get; set; }
        public List<string>? UserIds { get; set; }
        [Obsolete]
        public ContentEntityVisibility Visibility { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public FeedEntity FeedEntity { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public List<User> Users { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public byte[] Data { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public int NewPostCount { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public int NewMentionCount { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public int NewThreadActivityCount { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public bool IsNew { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        [Obsolete]
        public int CommentCount { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public override FeedType FeedType => FeedType.Document;

        [BsonIgnore]
        [JsonIgnore]
        public override string FeedTitle => Name;

        [BsonIgnore]
        [JsonIgnore]
        public override string FeedLogoId => ImageId;

        public string FeedEntityId => FeedId;

        [BsonIgnore]
        public bool IsMedia { get; set; }
    }
}