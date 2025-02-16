using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public interface IBaseQuery<T>
    {
        IBaseQuery<T> Filter(Expression<Func<T, bool>> filter);
        IBaseQuery<T> Page(int page, int pageSize);
        List<T> ToList();
        long Count();
        bool Any();
    }
}