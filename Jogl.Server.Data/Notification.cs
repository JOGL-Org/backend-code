using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum NotificationType { Invite, AdminRequest, AdminCommunityEntityInvite, Acceptation, AdminAccessLevel, Follower, Need, Resource, Note, NoteAuthor, Paper, Relation, Member, Comment, NoteEdit, AdminRelation, AdminMember, Mention, EventInvite, EventInviteUpdate }
    public class Notification : Entity
    {
        public string UserId { get; set; }
        public NotificationType Type { get; set; }
        public bool Actioned { get; set; }
        public string OriginFeedId { get; set; }
        public List<NotificationData> Data { get; set; }
        public string Text { get; set; }

        [BsonIgnore]
        public FeedEntity OriginFeedEntity { get; set; }

        [BsonIgnore]
        public User CreatedByUser { get; set; }
    }
}