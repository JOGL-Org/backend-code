using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class UserNotificationSettings
    {
        public bool MentionJogl { get; set; }
        public bool MentionEmail { get; set; }

        public bool PostMemberContainerJogl { get; set; }
        public bool PostMemberContainerEmail { get; set; }

        public bool ThreadActivityJogl { get; set; }
        public bool ThreadActivityEmail { get; set; }

        public bool PostAuthoredObjectJogl { get; set; }
        public bool PostAuthoredObjectEmail { get; set; }

        public bool DocumentMemberContainerJogl { get; set; }
        public bool DocumentMemberContainerEmail { get; set; }

        public bool NeedMemberContainerJogl { get; set; }
        public bool NeedMemberContainerEmail { get; set; }

        public bool PaperMemberContainerJogl { get; set; }
        public bool PaperMemberContainerEmail { get; set; }

        public bool EventMemberContainerJogl { get; set; }
        public bool EventMemberContainerEmail { get; set; }

        public bool ContainerInvitationJogl { get; set; }
        public bool ContainerInvitationEmail { get; set; }

        public bool EventInvitationJogl { get; set; }
        //public bool EventInvitationEmail { get; set; }

        public bool PostAttendingEventJogl { get; set; }
        public bool PostAttendingEventEmail { get; set; }

        public bool PostAuthoredEventJogl { get; set; }
        public bool PostAuthoredEventEmail { get; set; }
    }
}