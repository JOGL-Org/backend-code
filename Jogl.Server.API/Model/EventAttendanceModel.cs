using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EventAttendanceModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("event_id")]
        public string EventId { get; set; }
        [JsonPropertyName("user")]
        public UserMiniModel User { get; set; }
        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; }
        [JsonPropertyName("community_entity")]
        public CommunityEntityMiniModel CommunityEntity { get; set; }
        [JsonPropertyName("origin_community_entity")]
        public CommunityEntityMiniModel OriginCommunityEntity { get; set; }
        [JsonPropertyName("labels")]
        public List<string>? Labels { get; set; }
        public AttendanceStatus Status { get; set; }
        [JsonPropertyName("access_level")]
        public AttendanceAccessLevel AccessLevel { get; set; }
        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }
    }
}