using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IEventRepository : IRepository<Event>
    {
        IFluentQuery<Event> QueryForInvitationStatus(string searchValue, string currentUserId, AttendanceStatus? status);
    }
}