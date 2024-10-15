using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum AttendanceStatus { Pending, Yes, No }
    public enum AttendanceType { User, Email, CommunityEntity }
    public enum AttendanceAccessLevel { Member, Admin }
    public class EventAttendance : Entity
    {
        public string EventId { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string CommunityEntityId { get; set; }
        public CommunityEntityType? CommunityEntityType { get; set; }
        public string OriginCommunityEntityId { get; set; }
        public AttendanceStatus Status { get; set; }
        public AttendanceAccessLevel AccessLevel { get; set; }
        public List<string> Labels { get; set; }

        [BsonIgnore]
        public User User { get; set; }
        [BsonIgnore]
        public CommunityEntity CommunityEntity { get; set; }

        [BsonIgnore]
        public CommunityEntity OriginCommunityEntity { get; set; }
    }
}