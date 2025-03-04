using Jogl.Server.Data.Util;
using Jogl.Server.OpenAlex.DTO;

namespace Jogl.Server.OpenAlex
{
    public interface IOpenAlexFacade
    {
        Task<ListPage<Author>> ListAuthorsAsync(string search, int page, int pageSize);
        Task<ListPage<Work>> ListWorksForAuthorIdAsync(string authorId, int page, int pageSize);
        Task<ListPage<Work>> ListWorksForAuthorIdsAsync(IEnumerable<string> authorIds, int page, int pageSize);
        Task<ListPage<Work>> ListWorksForAuthorNameAsync(string authorSearch, int page, int pageSize);
        Task<ListPage<Work>> ListWorksAsync(string search, int page, int pageSize);
        Task<Work> GetWorkAsync(string id);
        Task<Work> GetWorkFromDOI(string doi);
        Task<List<Concept>> ListTagsByDOIAsync(string doi);
        Task<Dictionary<string, List<int>>> GetAbstractFromDOIAsync(string doi);
    }
}