using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface INeedRepository : IRepository<Need>
    {
        List<Need> ListForEntityIds(IEnumerable<string> ids);
        List<Need> ListForUsers(IEnumerable<string> ids);
    }
}