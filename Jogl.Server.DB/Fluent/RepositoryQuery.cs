﻿using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class RepositoryQuery<T> : BaseQuery<T>, IRepositoryQuery<T> where T : Entity
    {
        protected const string INDEX_SEARCH = "default";
        protected const string INDEX_AUTOCOMPLETE = "autocomplete_default";

        private readonly IRepository<T> _repository;

        public RepositoryQuery(IConfiguration configuration, IRepository<T> repository, IOperationContext context, IAggregateFluent<T> query) : base(configuration, context, query)
        {
            _repository = repository;
        }

        private class EntityWithUFRs
        {
            public List<UserFeedRecord> UserFeedRecords { get; set; }
        }

        public IRepositoryQuery<T> WithFeedRecordData()
        {
            var currentUserId = _context.UserId;
            var userFeedRecordCollection = _repository.GetCollection<UserFeedRecord>("userFeedRecords");

            _query = _query.Lookup<T, UserFeedRecord, UserFeedRecord, List<UserFeedRecord>, EntityWithUFRs>(
                foreignCollection: userFeedRecordCollection,
                let: new BsonDocument("feedId", new BsonDocument("$toString", "$_id")),
                lookupPipeline: new EmptyPipelineDefinition<UserFeedRecord>()
                    .Match(new BsonDocument("$expr", new BsonDocument("$and", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { $"${nameof(UserFeedRecord.FeedId)}", "$$feedId" }),
                        new BsonDocument("$eq", new BsonArray { $"${nameof(UserFeedRecord.UserId)}", $"{currentUserId}" })
                    }))),
                @as: o => o.UserFeedRecords)
                .As<T>()
                .AppendStage<T>(new BsonDocument("$addFields", new BsonDocument
                                {
                                    {
                                        nameof(Entity.LastOpenedUTC), new BsonDocument("$cond", new BsonDocument
                                        {
                                            { "if", new BsonDocument("$gt", new BsonArray { new BsonDocument("$size", $"$UserFeedRecords"), 0 }) },
                                            { "then", new BsonDocument("$arrayElemAt", new BsonArray { $"$UserFeedRecords.LastOpenedUTC", 0 }) },
                                            { "else", BsonNull.Value }
                                        })
                                    }
                                }));

            return this;
        }

        public new IRepositoryQuery<T> Filter(Expression<Func<T, bool>> filter)
        {
            if (filter != null)
                _query = _query.Match(filter);

            return this;
        }

        public IRepositoryQuery<T> FilterFeedEntities(string currentUserId, IEnumerable<Membership> currentUserMemberships, FeedEntityFilter? filter = null)
        {
            switch (filter)
            {
                case FeedEntityFilter.CreatedByUser:
                    _query = _query.Match(e => e.CreatedByUserId == currentUserId);
                    break;
                case FeedEntityFilter.SharedWithUser:
                    _query = _query.Match(e => (e as FeedEntity).UserVisibility.Any(u => u.UserId == currentUserId));
                    break;
                case FeedEntityFilter.OpenedByUser:
                    _query = _query.Match(e => e.LastOpenedUTC != null);
                    break;
                default:
                    //TODO this is a monstrosity, remove when file and link visibility is reworked
                    if (typeof(T) == typeof(Document))
                        _query = _query.Match(e => (e as Document).Type == DocumentType.Document || (e as Document).Type == DocumentType.Link ||
                                             e.CreatedByUserId == currentUserId ||
                                            (e as FeedEntity).DefaultVisibility != null ||
                                           ((e as FeedEntity).UserVisibility != null & (e as FeedEntity).UserVisibility.Any(uv => uv.UserId == currentUserId)) ||
                                           ((e as FeedEntity).CommunityEntityVisibility != null && (e as FeedEntity).CommunityEntityVisibility.Any(cev => currentUserMemberships.Any(m => m.CommunityEntityId == cev.CommunityEntityId))));
                    else
                        _query = _query.Match(e => e.CreatedByUserId == currentUserId ||
                                             (e as FeedEntity).DefaultVisibility != null ||
                                            ((e as FeedEntity).UserVisibility != null & (e as FeedEntity).UserVisibility.Any(uv => uv.UserId == currentUserId)) ||
                                            ((e as FeedEntity).CommunityEntityVisibility != null && (e as FeedEntity).CommunityEntityVisibility.Any(cev => currentUserMemberships.Any(m => m.CommunityEntityId == cev.CommunityEntityId))));
                    break;
            }

            return this;
        }

        public IRepositoryQuery<T> Sort(SortKey sortKey, bool ascending = true)
        {
            var sort = _repository.GetSort(sortKey);
            if (sort == null)
                return this;

            _query = ascending ? _query.SortBy(sort) : _query.SortByDescending(sort);
            return this;
        }

        public IBaseQuery<TOut> GroupBy<TGroup, TOut>(Expression<Func<T, TGroup>> groupBy, Expression<Func<IGrouping<TGroup, T>, TOut>> outExpr)
        {
            var newQuery = _query.Group(groupBy, outExpr)
                .ReplaceRoot<TOut>("$_v");

            return new BaseQuery<TOut>(_configuration, _context, newQuery);
        }

        public new IRepositoryQuery<T> Page(int page, int pageSize)
        {
            _query = _query.Skip((page - 1) * pageSize).Limit(pageSize);
            return this;
        }

        public List<TNew> ToList<TNew>(Expression<Func<T, TNew>> selector)
        {
            var builder = new ProjectionDefinitionBuilder<T>();
            var q = _query.Project(builder.Expression(selector));

            return q.ToList();
        }

        IBaseQuery<T> IBaseQuery<T>.Filter(Expression<Func<T, bool>> filter)
        {
            return Filter(filter);
        }

        IBaseQuery<T> IBaseQuery<T>.Page(int page, int pageSize)
        {
            return Page(page, pageSize);
        }
    }
}