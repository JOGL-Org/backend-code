using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EventUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("generate_meet_link")]
        public bool GenerateMeetLink { get; set; }

        [JsonPropertyName("meeting_url")]
        public string? MeetingURL { get; set; }

        [JsonPropertyName("banner_id")]
        public string? BannerId { get; set; }

        [JsonPropertyName("visibility")]
        public EventVisibility Visibility { get; set; }

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        [JsonPropertyName("timezone")]
        public TimezoneModel Timezone { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }

        [JsonPropertyName("location")]
        public GeolocationModel? Location { get; set; }

        [JsonPropertyName("attendances_to_upsert")]
        public List<EventAttendanceUpsertModel>? Attendances { get; set; }
    }
}