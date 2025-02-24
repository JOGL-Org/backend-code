using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public interface IRepositoryQuery<T> : IBaseQuery<T>
    {
        new IRepositoryQuery<T> Filter(Expression<Func<T, bool>> filter);
        IRepositoryQuery<T> FilterFeedEntities(string currentUserId, IEnumerable<Membership> currentUserMemberships, FeedEntityFilter? filter = null);
        IRepositoryQuery<T> Sort(SortKey sortKey, bool ascending = true);
        IBaseQuery<TOut> GroupBy<TGroup, TOut>(Expression<Func<T, TGroup>> groupBy, Expression<Func<IGrouping<TGroup, T>, TOut>> outExpr);
        IRepositoryQuery<T> WithFeedRecordData();
        new IRepositoryQuery<T> Page(int page, int pageSize);
        //List<T> ToList();
        List<TNew> ToList<TNew>(Expression<Func<T, TNew>> selector);
        //long Count();
        //bool Any();
    }
}