using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public class Image : Entity
    {
        public string Name { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        [BsonIgnore]
        public byte[] Data { get; set; }
    }
}