using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class UserExperience
    {
        public string Company { get; set; }
        public string Position { get; set; }
        public string? Description { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
        public bool Current { get; set; }
    }
}