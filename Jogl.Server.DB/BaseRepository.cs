using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Search;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public abstract class BaseRepository<T> : IRepository<T> where T : Entity
    {
        protected const string INDEX_SEARCH = "default";
        protected const string INDEX_AUTOCOMPLETE = "autocomplete_default";
        protected const string INDEX_UNIQUE = "unique_default";
        private readonly IConfiguration _configuration;
        private readonly IOperationContext _context;

        protected abstract string CollectionName { get; }
        protected Collation DefaultCollation { get { return new Collation("en", strength: CollationStrength.Primary); } }

        protected virtual UpdateDefinition<T> GetDefaultUpdateDefinition(T updatedEntity) => throw new NotImplementedException($"{GetType()} has no default update definition");
        protected virtual UpdateDefinition<T> GetDefaultUpsertDefinition(T updatedEntity) => throw new NotImplementedException($"{GetType()} has no default upsert definition");

        protected virtual Expression<Func<T, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.CreatedDate:
                    return (e) => e.CreatedUTC;
                case SortKey.LastActivity:
                    return (e) => e.LastActivityUTC;
                case SortKey.Date:
                    return (e) => e.CreatedUTC;
                default:
                    return null;
            }
        }

        protected virtual Expression<Func<T, string>> AutocompleteField => null;
        protected virtual Expression<Func<T, object>>[] SearchFields => null;

        protected BaseRepository(IConfiguration configuration, IOperationContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        protected IMongoDatabase GetDatabase(string db)
        {
            var settings = MongoClientSettings.FromConnectionString(_configuration["MongoDB:ConnectionString"]);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            return client.GetDatabase(db);
        }

        protected IMongoCollection<T> GetCollection<T>()
        {
            return GetCollection<T>(CollectionName);
        }

        protected IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            var db = GetDatabase(_configuration["MongoDB:DB"]);
            return db.GetCollection<T>(collectionName);
        }

        protected async Task<List<string>> ListSearchIndexesAsync()
        {
            var coll = GetCollection<CallForProposal>();
            var indexNames = (await coll.SearchIndexes.ListAsync()).ToList().Select(i => i["name"].AsString);
            return indexNames.ToList();
        }

        protected async Task<List<string>> ListIndexesAsync()
        {
            var coll = GetCollection<CallForProposal>();
            var indexNames = (await coll.Indexes.ListAsync()).ToList().Select(i => i["name"].AsString);
            return indexNames.ToList();
        }

        public async Task<string> CreateAsync(T entity)
        {
            var coll = GetCollection<T>();
            await coll.InsertOneAsync(entity);
            return entity.Id.ToString();
        }

        public async Task<List<string>> CreateAsync(List<T> entities)
        {
            var coll = GetCollection<T>();
            await coll.InsertManyAsync(entities);
            return entities.Select(e => e.Id.ToString()).ToList();
        }

        public async Task CreateBulkAsync(List<T> entities)
        {
            var coll = GetCollection<T>();
            await coll.BulkWriteAsync(entities.Select(e => new InsertOneModel<T>(e)));
        }

        public T Get(string entityId)
        {
            ObjectId id;
            if (!ObjectId.TryParse(entityId, out id))
                return default;

            var coll = GetCollection<T>();
            return coll.Find(e => e.Id == id && !e.Deleted, new FindOptions { Collation = DefaultCollation }).FirstOrDefault();
        }

        public List<T> Get(List<string> entityIds)
        {
            if (entityIds == null)
                return new List<T>();

            var coll = GetCollection<T>();
            var filterBuilder = Builders<T>.Filter;
            var list = entityIds.Where(eid => eid != null).Select(ObjectId.Parse).ToList();
            var filter = filterBuilder.In(e => e.Id, list) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter, new FindOptions { Collation = DefaultCollation }).ToList();
        }

        public T Get(Expression<Func<T, bool>> filter, bool includeDeleted = false)
        {
            var coll = GetCollection<T>();
            var filterBuilder = Builders<T>.Filter;
            var filterDefinition = new ExpressionFilterDefinition<T>(filter) & (includeDeleted ? filterBuilder.Empty : filterBuilder.Eq(e => e.Deleted, false));
            return coll.Find(filterDefinition, new FindOptions { Collation = DefaultCollation }).FirstOrDefault();
        }

        public T GetNewest(Expression<Func<T, bool>> filter)
        {
            var coll = GetCollection<T>();
            var filterBuilder = Builders<T>.Filter;
            var filterDefinition = new ExpressionFilterDefinition<T>(filter) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filterDefinition, new FindOptions { Collation = DefaultCollation }).SortByDescending(e => e.CreatedUTC).FirstOrDefault();
        }

        public List<T> List(List<string> entityIds, SortKey sortKey, bool ascending = false)
        {
            if (entityIds == null)
                return new List<T>();

            var coll = GetCollection<T>();
            var filterBuilder = Builders<T>.Filter;
            var list = entityIds.Select(ObjectId.Parse).ToList();
            var filter = filterBuilder.In(e => e.Id, list) & filterBuilder.Eq(e => e.Deleted, false);
            var query = coll.Find(filter, new FindOptions { Collation = DefaultCollation });

            var sort = GetSort(sortKey);
            if (sort != null)
                if (ascending)
                    query = query.SortBy(sort);
                else
                    query = query.SortByDescending(sort);

            return query.ToList();
        }

        public List<T> List(Expression<Func<T, bool>> filter, int page, int pageSize, SortKey sortKey = SortKey.Relevance, bool ascending = false)
        {
            var coll = GetCollection<T>();
            var query = coll
            .Find(filter, new FindOptions { Collation = DefaultCollation });

            var sort = GetSort(sortKey);
            if (sort != null)
                if (ascending)
                    query = query.SortBy(sort);
                else
                    query = query.SortByDescending(sort);

            return query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToList();
        }

        public List<T> List(Expression<Func<T, bool>> filter, SortKey sortKey, bool ascending = false)
        {
            var coll = GetCollection<T>();
            var query = coll
            .Find(filter, new FindOptions { Collation = DefaultCollation });

            var sort = GetSort(sortKey);
            if (sort != null)
                if (ascending)
                    query = query.SortBy(sort);
                else
                    query = query.SortByDescending(sort);

            return query.ToList();
        }

        public List<T> List(Expression<Func<T, bool>> filter)
        {
            var coll = GetCollection<T>();
            return coll
            .Find(filter, new FindOptions { Collation = DefaultCollation })
            .ToList();
        }

        public List<T> Search(string searchValue)
        {
            var coll = GetCollection<T>();

            if (string.IsNullOrEmpty(searchValue) || !SearchFields.Any())
                return coll.Find(itm => !itm.Deleted).ToList();

            return GetSearchQuery(searchValue).ToList();
        }

        public List<T> SearchSort(string searchValue, SortKey sortKey, bool ascending)
        {
            var sort = GetSort(sortKey);
            if (sort == null)
                return Search(searchValue);

            var coll = GetCollection<T>();
            if (string.IsNullOrEmpty(searchValue))
                return coll.Find(itm => !itm.Deleted).ToList();

            var searchQuery = GetSearchQuery(searchValue);
            return (ascending ? searchQuery.SortBy(sort) : searchQuery.SortByDescending(sort)).ToList();
        }

        public long SearchCount(string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
                return Count();

            return GetSearchQuery(searchValue).Count().SingleOrDefault()?.Count ?? 0;
        }

        public long SearchCount(Expression<Func<T, bool>> filter, string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
                return Count(filter);

            return GetSearchQuery(searchValue).Match(filter).Count().SingleOrDefault()?.Count ?? 0;
        }

        public List<T> SearchGet(List<string> entityIds, string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
                return Get(entityIds);

            return GetSearchQuery(searchValue).Match(itm => entityIds.Contains(itm.Id.ToString())).ToList();
        }

        public List<T> SearchGetSort(List<string> entityIds, SortKey sortKey, bool ascending, string searchValue)
        {
            var sort = GetSort(sortKey);
            if (sort == null)
                return SearchGet(entityIds, searchValue);

            var coll = GetCollection<T>();
            if (string.IsNullOrEmpty(searchValue))
                return List(entityIds, sortKey, ascending);

            var filterBuilder = Builders<T>.Filter;
            var list = entityIds.Select(ObjectId.Parse).ToList();
            var filter = filterBuilder.In(e => e.Id, list) & filterBuilder.Eq(e => e.Deleted, false);

            var searchQuery = GetSearchQuery(searchValue).Match(filter);
            return (ascending ? searchQuery.SortBy(sort) : searchQuery.SortByDescending(sort)).ToList();
        }

        public List<T> SearchList(Expression<Func<T, bool>> filter, string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
                return List(filter);

            return GetSearchQuery(searchValue).Match(filter).ToList();
        }

        public List<T> SearchListSort(Expression<Func<T, bool>> filter, SortKey sortKey, bool ascending, string searchValue)
        {
            var sort = GetSort(sortKey);
            if (sort == null)
                return SearchList(filter, searchValue);

            var coll = GetCollection<T>();
            if (string.IsNullOrEmpty(searchValue))
                return List(filter, sortKey, ascending).ToList();

            var searchQuery = GetSearchQuery(searchValue).Match(filter);
            return (ascending ? searchQuery.SortBy(sort) : searchQuery.SortByDescending(sort)).ToList();
        }

        public List<T> Autocomplete(string searchValue)
        {
            var coll = GetCollection<T>();

            if (string.IsNullOrEmpty(searchValue))
                return coll.Find(itm => !itm.Deleted).ToList();

            return GetAutocompleteQuery(searchValue).ToList();
        }

        public List<T> Autocomplete(string searchValue, int page, int pageSize)
        {
            var coll = GetCollection<T>();

            if (string.IsNullOrEmpty(searchValue))
                return coll.Find(itm => !itm.Deleted).ToList();

            return GetAutocompleteQuery(searchValue)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
        }

        public List<T> AutocompleteGet(List<string> entityIds, string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
                return Get(entityIds);

            return GetAutocompleteQuery(searchValue).Match(itm => entityIds.Contains(itm.Id.ToString())).ToList();
        }

        public List<T> AutocompleteList(Expression<Func<T, bool>> filter, string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
                return List(filter);

            return GetAutocompleteQuery(searchValue).Match(filter).ToList();
        }

        public List<T> AutocompleteList(Expression<Func<T, bool>> filter, string searchValue, SortKey sortKey, bool ascending)
        {
            if (string.IsNullOrEmpty(searchValue))
                return List(filter, sortKey, ascending);

            return GetAutocompleteQuery(searchValue).Match(filter).ToList();
        }

        public long Count(Expression<Func<T, bool>> filter = null)
        {
            var coll = GetCollection<T>();

            var filterBuilder = Builders<T>.Filter;
            var deletedFilter = filterBuilder.Eq(e => e.Deleted, false);
            if (filter == null)
                return coll.CountDocuments(itm => !itm.Deleted);

            var andFilter = filterBuilder.And(filter, deletedFilter);
            return coll.CountDocuments(andFilter);
        }

        public Dictionary<string, long> Counts(Expression<Func<T, string>> groupBy)
        {
            var coll = GetCollection<T>();
            var x = coll.Aggregate().Group(groupBy, g => new
            {
                Id = g.Key.ToString(),
                Count = g.LongCount()
            }).ToList();
            return x.ToDictionary(i => i.Id, i => i.Count);
        }

        public Dictionary<string, long> Counts(Expression<Func<T, bool>> filter, Expression<Func<T, string>> groupBy)
        {
            var coll = GetCollection<T>();
            var x = coll.Aggregate().Match(filter).Group(groupBy, g => new
            {
                Id = g.Key.ToString(),
                Count = g.LongCount()
            }).ToList();
            return x.ToDictionary(i => i.Id, i => i.Count);
        }

        public async Task UpdateAsync(T entity)
        {
            await UpdateAsync(entity, GetDefaultUpdateDefinition(entity));
        }

        public async Task UpdateAsync(List<T> entities)
        {
            foreach (var entity in entities)
            {
                await UpdateAsync(entity, GetDefaultUpdateDefinition(entity));
            }
        }

        protected async Task UpdateAsync(T entity, UpdateDefinition<T> updateDefinition)
        {
            await UpdateAsync(entity.Id.ToString(), updateDefinition);
        }

        public async Task UpsertAsync(T entity, Expression<Func<T, object>> key)
        {
            var coll = GetCollection<T>();

            var filter = Builders<T>.Filter.Eq(key, key.Compile().Invoke(entity));
            var update = GetDefaultUpsertDefinition(entity);

            await UpsertAsync(filter, update);
        }

        protected async Task UpdateAsync(string id, UpdateDefinition<T> updateDefinition)
        {
            var coll = GetCollection<T>();
            await coll.UpdateOneAsync(e => e.Id.ToString() == id, updateDefinition);
        }

        public async Task UpdateLastActivityAsync(string id, DateTime lastActivityUTC, string updatedByUserId)
        {
            var updateDefinition = Builders<T>.Update.Set(e => e.LastActivityUTC, lastActivityUTC);

            var coll = GetCollection<T>();
            await coll.UpdateOneAsync(e => e.Id.ToString() == id, updateDefinition);
        }

        public async Task UpdateLastActivityAsync(Entity entity)
        {
            var updateDefinition = Builders<T>.Update.Set(e => e.LastActivityUTC, entity.LastActivityUTC)
                                                     .Set(e => e.UpdatedUTC, entity.LastActivityUTC)
                                                     .Set(e => e.UpdatedByUserId, entity.UpdatedByUserId);

            var coll = GetCollection<T>();
            await coll.UpdateOneAsync(e => e.Id.ToString() == entity.Id.ToString(), updateDefinition);
        }

        protected async Task UpsertAsync(FilterDefinition<T> filterDefinition, UpdateDefinition<T> updateDefinition)
        {
            var coll = GetCollection<T>();
            await coll.UpdateOneAsync(filterDefinition, updateDefinition, new UpdateOptions { IsUpsert = true });
        }

        public async Task DeleteAsync(string entityId)
        {
            var coll = GetCollection<T>();
            await coll.UpdateOneAsync(e => e.Id == ObjectId.Parse(entityId), Builders<T>.Update.Set(e => e.Deleted, true));
        }

        public async Task DeleteAsync(T entity)
        {
            var coll = GetCollection<T>();
            await coll.UpdateOneAsync(e => e.Id == entity.Id, Builders<T>.Update.Set(e => e.Deleted, true));
        }

        public async Task DeleteAsync(FilterDefinition<T> filter)
        {
            var coll = GetCollection<T>();
            await coll.UpdateOneAsync(filter, Builders<T>.Update.Set(e => e.Deleted, true));
        }

        public async Task DeleteAsync(List<string> entityIds)
        {
            if (!entityIds.Any())
                return;

            var ids = entityIds.Select(ObjectId.Parse).ToList();
            var coll = GetCollection<T>();
            await coll.UpdateManyAsync(e => ids.Contains(e.Id), Builders<T>.Update.Set(e => e.Deleted, true));
        }

        public async Task DeleteAsync(List<T> entities)
        {
            if (!entities.Any())
                return;

            var ids = entities.Select(e => e.Id).ToList();
            var coll = GetCollection<T>();
            await coll.UpdateManyAsync(e => ids.Contains(e.Id), Builders<T>.Update.Set(e => e.Deleted, true));
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> filter)
        {
            var dbFilter = Builders<T>.Filter.Where(filter);
            await DeleteAsync(dbFilter);
        }

        public virtual async Task EnsureExistsAsync()
        {
            var coll = GetCollection<T>();
            var db = GetDatabase(_configuration["MongoDB:DB"]);

            //query collection
            var filter = new BsonDocument("name", CollectionName);
            var collections = await db.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });

            //check for existence
            if (await collections.AnyAsync())
                return;

            //collection does not exist - create
            await db.CreateCollectionAsync(CollectionName);
        }

        public virtual async Task InitializeAsync()
        {

        }

        private class EntityWithUFRs : Entity
        {
            public List<UserFeedRecord> UserFeedRecords { get; set; }
        }

        private IAggregateFluent<T> GetSearchQuery(string searchValue, SortKey? sortKey = null)
        {
            var builder = new SearchPathDefinitionBuilder<T>();
            var searchDefinition = builder.Multi(SearchFields);

            var coll = GetCollection<T>();
            var query = coll.Aggregate().Search(Builders<T>.Search.Text(searchDefinition, searchValue, new SearchFuzzyOptions
            {
                MaxEdits = 2,
                PrefixLength = 1,
                MaxExpansions = 256,
            }), new SearchOptions<T> { IndexName = INDEX_SEARCH }).Match(itm => !itm.Deleted);

            return query;
        }

        private IAggregateFluent<T> GetAutocompleteQuery(string searchValue)
        {
            var builder = new SearchPathDefinitionBuilder<T>();
            var searchDefinition = builder.Single(AutocompleteField);

            var coll = GetCollection<T>();
            return coll.Aggregate().Search(Builders<T>.Search.Autocomplete(searchDefinition, searchValue), new SearchOptions<T> { IndexName = INDEX_AUTOCOMPLETE }).Match(itm => !itm.Deleted);
        }
    }
}