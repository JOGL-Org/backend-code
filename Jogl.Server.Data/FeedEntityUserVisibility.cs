using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public class FeedEntityUserVisibility
    {
        public string UserId { get; set; }
        public FeedEntityVisibility Visibility { get; set; }

        [BsonIgnore]
        public User User { get; set; }
    }
}