using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class InterfaceMessage : Entity
    {
        public const string TAG_ONBOARDING_EMAIL_RECEIVED = "ONBOARDING_EMAIL_RECEIVED";
        public const string TAG_ONBOARDING_CODE_RECEIVED = "ONBOARDING_CODE_RECEIVED";
        public const string TAG_ONBOARDING_COMPLETED = "ONBOARDING_COMPLETED";
        public const string TAG_SEARCH_USER = "TAG_SEARCH_USER";
        public const string TAG_CONSULT_PROFILE = "TAG_CONSULT_PROFILE";

        public string MessageId { get; set; }
        public string? MessageMirrorId { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public string ConversationId { get; set; }
        public string ChannelId { get; set; }
        public string? Tag { get; set; }
        public string? Context { get; set; }
        public string? OriginalQuery { get; set; }
    }
}