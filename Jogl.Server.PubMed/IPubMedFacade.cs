using Jogl.Server.Data.Util;
using Jogl.Server.PubMed.DTO;

namespace Jogl.Server.PubMed
{
    public interface IPubMedFacade
    {
        Task<MeshHeading[]> GetTagsFromDOI(string doi);
        Task<string> GetPMIDFromDOI(string doi);
        Task<ListPage<PubmedArticle>> ListArticlesAsync(string search, int page, int pageSize);
        Task<List<PubmedArticle>> ListNewPapersAsync(string lastId);
        Dictionary<string, List<string>> ListCategories();
    }
}