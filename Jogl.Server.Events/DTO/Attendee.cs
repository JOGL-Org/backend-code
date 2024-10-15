using Jogl.Server.Data;

namespace Jogl.Server.Events.DTO
{
    public class Attendee
    {
        public string Email { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}
