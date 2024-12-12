using Jogl.Server.Data.Util;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public interface IFluentQuery<T>
    {
        IFluentQuery<T> Filter(Expression<Func<T, bool>> filter);
        IFluentQuery<T> Sort(SortKey sortKey, bool ascending = true);
        IFluentQuery<T> WithFeedRecordDataUTC();
        IFluentQuery<T> Page(int page, int pageSize);
        List<T> ToList();
        List<TNew> ToList<TNew>(Expression<Func<T, TNew>> selector);
        long Count();
    }
}