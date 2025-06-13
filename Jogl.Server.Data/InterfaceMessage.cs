using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class InterfaceMessage : Entity
    {
        public const string TAG_ONBOARDING = "ONBOARDING";
        public const string TAG_SEARCH_USER = "TAG_SEARCH_USER";

        public string MessageId { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public string ConversationId { get; set; }
        public string ChannelId { get; set; }
        public string Tag { get; set; }
        public string Context { get; set; }
    }
}