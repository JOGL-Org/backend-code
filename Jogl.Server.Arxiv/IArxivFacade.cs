using Jogl.Server.Arxiv.DTO;

namespace Jogl.Server.Arxiv
{
    public interface IArxivFacade
    {
        Task<List<Entry>> ListNewPapersAsync(DateTime since);
        List<string> ListCategories();
    }
}