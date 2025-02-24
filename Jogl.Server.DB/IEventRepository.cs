using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IEventRepository : IRepository<Event>
    {
        IRepositoryQuery<Event> QueryWithAttendanceData(string searchValue, string currentUserId);
    }
}