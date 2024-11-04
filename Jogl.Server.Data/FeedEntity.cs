using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public enum FeedEntityVisibility { View, Comment, Edit }

    public abstract class FeedEntity : Entity
    {
        [BsonIgnore]
        public abstract string FeedTitle { get; }

        [BsonIgnore]
        public abstract FeedType FeedType { get; }

        [BsonIgnore]
        public virtual string? FeedLogoId { get; }

        public FeedEntityVisibility? DefaultVisibility { get; set; }
        public List<FeedEntityUserVisibility>? UserVisibility { get; set; }
        public List<FeedEntityCommunityEntityVisibility>? CommunityEntityVisibility { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public int PostCount { get; set; }
    }
}