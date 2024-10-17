using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EventAttendanceAccessLevelModel 
    {
        [JsonPropertyName("access_level")]
        public AttendanceAccessLevel AccessLevel { get; set; }
    }
}