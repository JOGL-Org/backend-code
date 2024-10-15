using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EventAttendanceLevelBatchModel
    {
        [JsonPropertyName("attendance_ids")]
        public List<string> AttendanceIds { get; set; }

        [JsonPropertyName("access_level")]
        public AttendanceAccessLevel AccessLevel { get; set; }
    }
}