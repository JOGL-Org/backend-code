using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum EventVisibility { Private, Container, Public }
    public enum EventTag { Invited, Attending, Rejected, Organizer, Speaker, Attendee, Online, Physical }

    [BsonIgnoreExtraElements]
    public class Event : FeedEntity, ICommunityEntityOwned, IFeedEntityOwned
    {
        public string Title { get; set; }
        [RichText]
        public string Description { get; set; }
        public bool GenerateMeetLink { get; set; }
        public bool GenerateZoomLink { get; set; }
        public string MeetingURL { get; set; }
        public string GeneratedMeetingURL { get; set; }
        public string BannerId { get; set; }
        public EventVisibility Visibility { get; set; }
        public string CommunityEntityId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Timezone Timezone { get; set; }
        public string ExternalId { get; set; }
        public List<string> Keywords { get; set; }
        public Geolocation Location { get; set; }

        [BsonIgnore]
        public int PostCount { get; set; }

        [BsonIgnore]
        public int NewPostCount { get; set; }

        [BsonIgnore]
        public int NewMentionCount { get; set; }

        [BsonIgnore]
        public int NewThreadActivityCount { get; set; }

        [BsonIgnore]
        public int CommentCount { get; set; }
        [BsonIgnore]
        public EventAttendance UserAttendance { get; set; }
        [BsonIgnore]
        public int AttendeeCount { get; set; }
        [BsonIgnore]
        public int InviteeCount { get; set; }

        [BsonIgnore]
        public override FeedType FeedType => FeedType.Event;

        [BsonIgnore]
        public override string FeedTitle => Title;

        [BsonIgnore]
        public override string FeedLogoId => BannerId;

        [BsonIgnore]
        public bool IsNew { get; set; }

        [BsonIgnore]
        public CommunityEntity CommunityEntity { get; set; }

        [BsonIgnore]
        public FeedEntity FeedEntity { get => CommunityEntity; set { CommunityEntity = value as CommunityEntity; } }

        [BsonIgnore]
        public string FeedEntityId { get => CommunityEntityId; }

        [BsonIgnore]
        public List<EventAttendance> Attendances { get; set; }
    }
}