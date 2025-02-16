using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class ContentEntityRepository : BaseRepository<ContentEntity>, IContentEntityRepository
    {
        public ContentEntityRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "contentEntity";

        public List<ContentEntity> List(IEnumerable<string> feedIds, Expression<Func<ContentEntity, bool>> filter, int page, int pageSize)
        {
            var coll = GetCollection<ContentEntity>();
            var filterBuilder = Builders<ContentEntity>.Filter;
            var filterObject = filterBuilder.In(e => e.FeedId, feedIds) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filterObject)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
        }

        public List<ContentEntity> List(IEnumerable<string> feedIds)
        {
            var coll = GetCollection<ContentEntity>();
            var filterBuilder = Builders<ContentEntity>.Filter;
            var filterObject = filterBuilder.In(e => e.FeedId, feedIds) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filterObject).ToList();
        }

        private class EntityWithComments
        {
            public List<Comment> Comments { get; set; }
        }

        public IRepositoryQuery<ContentEntity> QueryForPosts(string currentUserId, Expression<Func<ContentEntity, bool>> filter = null)
        {
            var coll = GetCollection<ContentEntity>();
            var q = coll.Aggregate().Match(itm => !itm.Deleted);
            if (filter != null)
                q = q.Match(filter);

            q = q.Match(ce => ce.CreatedByUserId != currentUserId);

            return new RepositoryQuery<ContentEntity>(_configuration, this, _context, q);
        }

        public IRepositoryQuery<ContentEntity> QueryForReplies(string currentUserId, Expression<Func<ContentEntity, bool>> filter = null)
        {
            var coll = GetCollection<ContentEntity>();
            var q = coll.Aggregate().Match(itm => !itm.Deleted);
            if (filter != null)
                q = q.Match(filter);

            var commentCollection = GetCollection<Comment>("comments");

            q = q.Lookup<ContentEntity, Comment, Comment, List<Comment>, EntityWithComments>(
                foreignCollection: commentCollection,
                let: new BsonDocument("contentEntityId", new BsonDocument("$toString", "$_id")),
                lookupPipeline: new EmptyPipelineDefinition<Comment>()
                    .Match(new BsonDocument("$expr", new BsonDocument("$and", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { $"${nameof(Comment.ContentEntityId)}", "$$contentEntityId" }),
                        new BsonDocument("$ne", new BsonArray { $"${nameof(UserFeedRecord.CreatedByUserId)}", $"{currentUserId}" })
                    }))),
                @as: o => o.Comments)
                .As<ContentEntity>()
                .Match(new BsonDocument("$or", new BsonArray
                    {
                        new BsonDocument("Comments", new BsonDocument("$ne", new BsonArray()))
                    }));

            return new RepositoryQuery<ContentEntity>(_configuration, this, _context, q);
        }

        protected override UpdateDefinition<ContentEntity> GetDefaultUpdateDefinition(ContentEntity updatedEntity)
        {
            return Builders<ContentEntity>.Update.Set(e => e.Text, updatedEntity.Text)
                                                 .Set(e => e.Status, updatedEntity.Status)
                                                 .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                 .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId);
        }
    }
}