﻿using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public interface IFluentQuery<T>
    {
        IFluentQuery<T> Filter(Expression<Func<T, bool>> filter);
        IFluentQuery<T> FilterFeedEntities(string currentUserId, IEnumerable<Membership> currentUserMemberships, FeedEntityFilter? filter = null);
        IFluentQuery<T> Sort(SortKey sortKey, bool ascending = true);
        IFluentQuery<T> WithFeedRecordData();
        IFluentQuery<T> Page(int page, int pageSize);
        List<T> ToList();
        List<TNew> ToList<TNew>(Expression<Func<T, TNew>> selector);
        long Count();
        bool Any();
    }
}