using Jogl.Server.Data.Util;

namespace Jogl.Server.DB
{
    public interface IFluentQuery<T>
    {
        IFluentQuery<T> WithLastOpenedUTC();
        IFluentQuery<T> Sort(SortKey sortKey, bool ascending = true);
        IFluentQuery<T> Page(int page, int pageSize);
        List<T> ToList();
    }
}