using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public interface IBaseQuery<T>
    {
        IBaseQuery<T> Filter(Expression<Func<T, bool>> filter);
        IBaseQuery<T> Page(int page, int pageSize);
        IBaseQuery<T> Sort(Expression<Func<T, object>> sort, bool ascending = true);
        List<T> ToList();
        long Count();
        bool Any();
    }
}