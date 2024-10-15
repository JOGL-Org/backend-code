using Jogl.Server.Data.Util;
using Jogl.Server.OpenAlex.DTO;

namespace Jogl.Server.OpenAlex
{
    public interface IOpenAlexFacade
    {
        Task<ListPage<Work>> ListWorksAsync(string search, int page, int pageSize);
        Task<Work> GetWorkFromDOI(string doi);
        Task<List<Concept>> ListTagsByDOIAsync(string doi);
        Task<Dictionary<string, List<int>>> GetAbstractFromDOIAsync(string doi);
    }
}