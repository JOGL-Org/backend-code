using Jogl.Server.Data;

namespace Jogl.Server.Search
{
    public interface ISearchService
    {
        Task<List<User>> SearchUsersAsync(string query, IEnumerable<string>? userIds = default);
        Task IndexUsersAsync(IEnumerable<Data.User> users, IEnumerable<Document> documents, IEnumerable<Paper> papers);
    }
}
