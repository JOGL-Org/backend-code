using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IEventAttendanceRepository : IRepository<EventAttendance>
    {
        Task UpdateUserAsync(EventAttendance attendance);
    }
}