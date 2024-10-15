using Jogl.Server.Data.Util;
using Jogl.Server.SemanticScholar.DTO;

namespace Jogl.Server.SemanticScholar
{
    public interface ISemanticScholarFacade
    {
        Task<ListPage<SemanticPaper>> ListWorksAsync(string search, int page, int pageSize);
        Task<ListPage<Author>> ListAuthorsAsync(string search, int page, int pageSize);
        Task<SemanticTags> ListTagsByDOIAsync(string search);
    }
}