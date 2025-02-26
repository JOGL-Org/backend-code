using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using MongoDB.Bson;
using MongoDB.Driver.Search;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public interface IRepository
    {
        Task EnsureExistsAsync();
        Task InitializeAsync();

        IMongoCollection<T> GetCollection<T>();
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }

    public interface IRepository<T> : IRepository where T : Entity
    {
        Expression<Func<T, object>> GetSort(SortKey sortKey);
        Task<string> CreateAsync(T entity);
        Task<List<string>> CreateAsync(List<T> entities);
        Task CreateBulkAsync(List<T> entities);
        T Get(string entityId);
        T Get(Expression<Func<T, bool>> filter, bool includeDeleted = false);
        T GetNewest(Expression<Func<T, bool>> filter);
        List<T> Get(List<string> entityIds);
        List<T> List(Expression<Func<T, bool>> filter, int page, int pageSize, SortKey sortKey = SortKey.Relevance, bool ascending = false);
        List<T> List(Expression<Func<T, bool>> filter, SortKey sortKey, bool ascending = false);
        List<T> List(Expression<Func<T, bool>> filter);
        List<T> Search(string searchValue);
        List<T> SearchGet(List<string> entityIds, string searchValue);
        List<T> SearchGetSort(List<string> entityIds, SortKey sortKey, bool ascending, string searchValue);
        List<T> SearchList(Expression<Func<T, bool>> filter, string searchValue);
        List<T> SearchListSort(Expression<Func<T, bool>> filter, SortKey sortKey, bool ascending, string searchValue);
        List<T> SearchSort(string searchValue, SortKey sortKey, bool ascending);
        long SearchCount(string searchValue);
        long SearchCount(Expression<Func<T, bool>> filter, string searchValue);
        List<T> Autocomplete(string searchValue);
        List<T> Autocomplete(string searchValue, int page, int pageSize);
        List<T> AutocompleteGet(List<string> entityIds, string searchValue);
        List<T> AutocompleteList(Expression<Func<T, bool>> filter, string searchValue);
        List<T> AutocompleteList(Expression<Func<T, bool>> filter, string searchValue, SortKey sortKey, bool ascending);
        long Count(Expression<Func<T, bool>> filter);
        Dictionary<string, long> Counts(Expression<Func<T, string>> groupBy);
        Dictionary<string, long> Counts(Expression<Func<T, bool>> filter, Expression<Func<T, string>> groupBy);
        Task UpdateAsync(T entity);
        Task UpdateAsync(List<T> entities);
        Task UpsertAsync(T entity, Expression<Func<T, object>> key);
        Task UpdateLastActivityAsync(string id, DateTime lastActivityUTC, string updatedByUserId);
        Task UpdateLastActivityAsync(Entity entity);
        Task DeleteAsync(string entityId);
        Task UndeleteAsync(string entityId);
        Task DeleteAsync(T entity);
        Task UndeleteAsync(T entity);
        Task DeleteAsync(List<string> entityIds);
        Task DeleteAsync(List<T> entities);
        Task DeleteAsync(Expression<Func<T, bool>> filter);
        Task UndeleteAsync(Expression<Func<T, bool>> filter);

        public IRepositoryQuery<T> Query(Expression<Func<T, bool>> filter = null);
        public IRepositoryQuery<T> Query(string searchValue);
        public IRepositoryQuery<T> QueryAutocomplete(string searchValue);
    }
}