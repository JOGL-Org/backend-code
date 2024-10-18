using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public interface IRepository
    {
        Task InitializeAsync();
    }

    public interface IRepository<T> : IRepository where T : Entity
    {
        Task<string> CreateAsync(T entity);
        Task<List<string>> CreateAsync(List<T> entities);
        T Get(string entityId);
        T Get(Expression<Func<T, bool>> filter);
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
        Task DeleteAsync(T entity);
        Task DeleteAsync(List<string> entityIds);
        Task DeleteAsync(List<T> entities);
        Task DeleteAsync(Expression<Func<T, bool>> filter);
    }
}