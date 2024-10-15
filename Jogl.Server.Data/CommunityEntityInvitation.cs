using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public class CommunityEntityInvitation : Entity
    {
        public InvitationStatus Status { get; set; }
        public string SourceCommunityEntityId { get; set; }
        public CommunityEntityType SourceCommunityEntityType { get; set; }
        public string TargetCommunityEntityId { get; set; }
        public CommunityEntityType TargetCommunityEntityType { get; set; }
        
        [BsonIgnore]
        public CommunityEntity SourceCommunityEntity { get; set; }
        [BsonIgnore]
        public CommunityEntity TargetCommunityEntity { get; set; }
    }
}