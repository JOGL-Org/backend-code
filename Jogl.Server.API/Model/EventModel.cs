using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EventModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("generate_meet_link")]
        public bool GenerateMeetLink { get; set; }

        [JsonPropertyName("meeting_url")]
        public string MeetingURL { get; set; }

        [JsonPropertyName("generated_meeting_url")]
        public string GeneratedMeetingURL { get; set; }

        [JsonPropertyName("banner_id")]
        public string BannerId { get; set; }

        [JsonPropertyName("banner_url")]
        public string BannerUrl { get; set; }

        [JsonPropertyName("banner_url_sm")]
        public string BannerUrlSmall { get; set; }

        [JsonPropertyName("visibility")]
        public EventVisibility Visibility { get; set; }

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        [JsonPropertyName("timezone")]
        public TimezoneModel Timezone { get; set; }

        [JsonPropertyName("feed_stats")]
        public FeedStatModel FeedStats { get; set; }

        [JsonPropertyName("permissions")]
        public List<Permission> Permissions { get; set; }

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; }

        [JsonPropertyName("location")]
        public GeolocationModel Location { get; set; }

        [JsonPropertyName("feed_entity")]
        public EntityMiniModel? FeedEntity { get; set; }

        [JsonPropertyName("user_attendance")]
        public EventAttendanceModel UserAttendance { get; set; }

        [JsonPropertyName("attendee_count")]
        public int AttendeeCount { get; set; }

        [JsonPropertyName("invitee_count")]
        public int InviteeCount { get; set; }

        [JsonPropertyName("is_new")]
        public bool IsNew { get; set; }

        [JsonPropertyName("path")]
        public List<EntityMiniModel> Path { get; set; }
    }
}