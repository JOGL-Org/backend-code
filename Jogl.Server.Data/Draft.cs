using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class Draft : Entity
    {
        public string EntityId { get; set; }

        public string UserId { get; set; }

        public string Text { get; set; }
    }
}