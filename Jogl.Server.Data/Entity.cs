using Jogl.Server.Data.Converters;
using Jogl.Server.Data.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public abstract class Entity
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedUTC { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime? UpdatedUTC { get; set; }
        public string? UpdatedByUserId { get; set; }
        public DateTime? LastActivityUTC { get; set; }

        [BsonIgnore]
        public List<Permission> Permissions { get; set; }

        [BsonIgnore]
        public List<FeedEntity>? Path { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public User CreatedBy { get; set; }
    }
}