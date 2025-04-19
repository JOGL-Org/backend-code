using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{

    [BsonIgnoreExtraElements]
    public class InterfaceMessage : Entity
    {
        public const string TAG_ONBOARDING = "ONBOARDING";

        public string ExternalId { get; set; }
        public string Tag { get; set; }
    }
}