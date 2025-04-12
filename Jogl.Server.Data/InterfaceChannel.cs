using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum ChannelType { Slack }

    [BsonIgnoreExtraElements]
    public class InterfaceChannel : Entity
    {
        public string NodeId { get; set; }
        public ChannelType Type { get; set; }
        public string ExternalId { get; set; }
        public string Key { get; set; }
    }
}