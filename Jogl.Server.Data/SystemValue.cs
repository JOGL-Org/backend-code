using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class SystemValue : Entity
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}