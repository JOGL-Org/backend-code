using Azure.Search.Documents.Models;
using Jogl.Server.Data;
using User = Jogl.Server.Search.Model.User;

namespace Jogl.Server.Search
{
    public interface ISearchService
    {
        Task<List<SearchResult<User>>> SearchUsersAsync(string query, string configuration = "default", IEnumerable<string>? userIds = default);
        Task IndexUsersAsync(IEnumerable<Data.User> users, IEnumerable<Document> documents, IEnumerable<Paper> papers);
    }
}
