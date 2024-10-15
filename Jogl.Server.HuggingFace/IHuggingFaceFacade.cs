using Jogl.Server.HuggingFace.DTO;

namespace Jogl.Server.HuggingFace
{
    public interface IHuggingFaceFacade
    {
        Task<List<Discussion>> ListPRsAsync(string repo);
    }
}