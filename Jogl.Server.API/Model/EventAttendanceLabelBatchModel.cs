using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EventAttendanceLabelBatchModel
    {
        [JsonPropertyName("attendance_ids")]
        public List<string> AttendanceIds { get; set; }

        [JsonPropertyName("labels")]
        public List<string>? Labels { get; set; }
    }
}