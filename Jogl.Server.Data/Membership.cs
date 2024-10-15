using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum AccessLevel { Member, Admin, Owner }
    public enum SimpleAccessLevel { Member, Admin }

    public class Membership : Entity
    {
        public string UserId { get; set; }
        public AccessLevel AccessLevel { get; set; }
        [Obsolete]
        public string Description { get; set; }
        public string CommunityEntityId { get; set; }
        public string Contribution { get; set; }
        public CommunityEntityType CommunityEntityType { get; set; }
        public DateTime? OnboardedUTC { get; set; }
        public List<string> Labels { get; set; }

        [Obsolete]
        public object OnboardingData { get; set; }

        [BsonIgnore]
        public User User { get; set; }
        [BsonIgnore]
        public CommunityEntity CommunityEntity { get; set; }
    }
}