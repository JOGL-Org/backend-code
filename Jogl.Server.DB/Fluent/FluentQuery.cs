using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class FluentQuery<T> : IFluentQuery<T> where T : Entity
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<T> _repository;
        private readonly IOperationContext _context;

        private IAggregateFluent<T> _query;

        public FluentQuery(IConfiguration configuration, IRepository<T> repository, IOperationContext context, IAggregateFluent<T> query)
        {
            _configuration = configuration;
            _context = context;

            _query = query;
        }

        private class EntityWithUFRs
        {
            public List<UserFeedRecord> UserFeedRecords { get; set; }
        }

        public IFluentQuery<T> WithLastOpenedUTC()
        {
            var currentUserId = _context.UserId;
            var userFeedRecordCollection = _repository.GetCollection<UserFeedRecord>("userFeedRecords");

            _query = _query.Lookup<T, UserFeedRecord, UserFeedRecord, IEnumerable<UserFeedRecord>, EntityWithUFRs>(
                foreignCollection: userFeedRecordCollection,
                let: new BsonDocument("feedId", new BsonDocument("$toString", "$_id")),
                lookupPipeline: new EmptyPipelineDefinition<UserFeedRecord>()
                    .Match(new BsonDocument("$expr", new BsonDocument("$and", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { nameof(UserFeedRecord.FeedId), "$$feedId" }),
                        new BsonDocument("$eq", new BsonArray { nameof(UserFeedRecord.UserId), $"{currentUserId}" })
                    }))),
                @as: o => o.UserFeedRecords)
                .Project<T>(new BsonDocument {  { "$ROOT", 1 },
                { nameof(Entity.LastOpenedUTC), new BsonDocument("$cond", new BsonDocument
                    {
                        { "if", new BsonDocument("$gt", new BsonArray { new BsonDocument("$size", $"${nameof(EntityWithUFRs.UserFeedRecords)}"), 0 }) },
                        { "then", new BsonDocument("$arrayElemAt", new BsonArray { $"${nameof(EntityWithUFRs.UserFeedRecords)}.${nameof(UserFeedRecord.LastReadUTC)}", 0 }) },
                        { "else", BsonNull.Value }
                    })
                }});

            return this;
        }

        public IFluentQuery<T> Sort(SortKey sortKey, bool ascending = true)
        {
            var sort = _repository.GetSort(sortKey);

            _query = (ascending ? _query.SortBy(sort) : _query.SortByDescending(sort));
            return this;
        }

        public IFluentQuery<T> Page(int page, int pageSize)
        {
            _query = _query.Skip((page - 1) * pageSize).Limit(pageSize);
            return this;
        }

        public List<T> ToList()
        {
            return _query.ToList();
        }
    }
}