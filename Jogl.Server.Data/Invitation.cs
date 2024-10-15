using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public enum InvitationStatus { Pending, Accepted, Rejected }
    public enum InvitationType { Invitation, Request }
    public class Invitation : Entity
    {
        public string CommunityEntityId { get; set; }
        public CommunityEntityType CommunityEntityType { get; set; }
        public string InviteeUserId { get; set; }
        public string InviteeEmail { get; set; }
        public InvitationStatus Status { get; set; }
        public InvitationType Type { get; set; }
        public AccessLevel AccessLevel { get; set; }
        [BsonIgnore]
        [JsonIgnore]
        public User User { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public CommunityEntity Entity { get; set; }
    }
}