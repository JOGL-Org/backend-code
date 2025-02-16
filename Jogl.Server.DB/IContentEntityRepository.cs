using Jogl.Server.Data;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public interface IContentEntityRepository : IRepository<ContentEntity>
    {
        List<ContentEntity> List(IEnumerable<string> feedIds, Expression<Func<ContentEntity, bool>> filter, int page, int pageSize);
        List<ContentEntity> List(IEnumerable<string> feedIds);

        IRepositoryQuery<ContentEntity> QueryForPosts(string currentUserId, Expression<Func<ContentEntity, bool>> filter = null);
        IRepositoryQuery<ContentEntity> QueryForReplies(string currentUserId, Expression<Func<ContentEntity, bool>> filter = null);
    }
}