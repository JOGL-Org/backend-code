using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum InterfaceUserOnboardingStatus { EmailPending, CodePending, Onboarded }

    [BsonIgnoreExtraElements]
    public class InterfaceUser : Entity
    {
        public string UserId { get; set; }
        public string ChannelId { get; set; }
        public string ExternalId { get; set; }
        public InterfaceUserOnboardingStatus OnboardingStatus { get; set; }
    }
}