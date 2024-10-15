using Jogl.Server.Data;
using Jogl.Server.Events.DTO;

namespace Jogl.Server.Events
{
    public interface ICalendarService
    {
        Task<string> CreateCalendarAsync(string title);
        Task<string> GetJoglCalendarAsync();
        Task<string> CreateEventAsync(string calendarId, Event ev, IEnumerable<User> organizers);
        Task UpdateEventAsync(string calendarId, Event ev, IEnumerable<User> organizers, bool generateMeetLink, bool generateZoomLink);
        Task DeleteEventAsync(string calendarId, string eventId);
        Task InviteUserAsync(string calendarId, string eventId, string email, AttendanceStatus status = AttendanceStatus.Pending);
        Task InviteUserAsync(string calendarId, string eventId, Dictionary<string, AttendanceStatus> emails);
        Task UninviteUserAsync(string calendarId, string eventId, string email);
        Task UninviteUserAsync(string calendarId, string eventId, List<string> emails);
        Task UpdateInvitationStatus(string calendarId, string eventId, string email, AttendanceStatus status);
        Task<List<Attendee>> ListAttendeesAsync(string calendarId, string eventId);

        Task<Dictionary<string, string>> ListCalendarsAsync();
        Task<Dictionary<string, string>> ListEventsForCalendarAsync(string calendarId);
        Task<string> GetEventAsync(string calendarId, string eventId);
    }
}