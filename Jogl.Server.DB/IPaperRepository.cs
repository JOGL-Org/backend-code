using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IPaperRepository : IRepository<Paper>
    {
        public List<Paper> ListForExternalIds(IEnumerable<string> externalIds);
    }
}