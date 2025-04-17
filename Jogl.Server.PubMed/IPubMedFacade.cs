using Jogl.Server.Data.Util;
using Jogl.Server.PubMed.DTO;
using Jogl.Server.PubMed.DTO.EFetch;
using MeshHeading = Jogl.Server.PubMed.DTO.MeshHeading;

namespace Jogl.Server.PubMed
{
    public interface IPubMedFacade
    {
        Task<MeshHeading[]> GetTagsFromDOI(string doi);
        Task<string> GetPMIDFromDOI(string doi);
        Task<ListPage<PubmedArticle>> ListArticlesAsync(string search, int page, int pageSize);
        Task<List<PubmedArticle>> ListNewPapersAsync(string lastId);
        List<string> ListCategories();
    }
}