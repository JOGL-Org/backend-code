using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IEventRepository : IRepository<Event>
    {
        IFluentQuery<Event> QueryWithAttendanceData(string searchValue, string currentUserId);
    }
}