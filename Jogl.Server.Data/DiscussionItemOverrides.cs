using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class DiscussionItemOverrides
    {
        public string UserAvatarURL { get; set; }
        public string UserURL { get; set; }
        public string UserName { get; set; }
    }
}