using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface ICommentRepository : IRepository<Comment>
    {
        List<Comment> ListForContentEntities(IEnumerable<string> contentEntityIds);
    }
}