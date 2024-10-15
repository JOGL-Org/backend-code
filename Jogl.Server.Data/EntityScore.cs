using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class EntityScore : Entity
    {
        public const string USER = "USER";

        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public decimal Score { get; set; }
    }
}