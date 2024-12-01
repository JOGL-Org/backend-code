using Jogl.Server.Data.Util;

namespace Jogl.Server.DB
{
    public interface IFluentQuery<T>
    {
        IFluentQuery<T> Search(string searchValue);
        IFluentQuery<T> Sort(SortKey sortKey, bool ascending = true);
        IFluentQuery<T> WithLastOpenedUTC();
        IFluentQuery<T> Page(int page, int pageSize);
        List<T> ToList();
    }
}