using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EventAttendanceUpsertModel
    {
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
        [JsonPropertyName("user_email")]
        public string? UserEmail { get; set; }
        [JsonPropertyName("community_entity_id")]
        public string? CommunityEntityId { get; set; }
        [JsonPropertyName("origin_community_entity_id")]
        public string? OriginCommunityEntityId { get; set; }
        [JsonPropertyName("labels")]
        public List<string>? Labels { get; set; }
        [JsonPropertyName("access_level")]
        public AttendanceAccessLevel AccessLevel { get; set; }
    }
}