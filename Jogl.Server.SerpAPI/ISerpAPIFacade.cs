using Jogl.Server.SerpAPI.DTO;

namespace Jogl.Server.SerpAPI
{
    public interface ISerpAPIFacade
    {
        Task<string> GetProfileAsync(string firstName, string lastName, string affil);
    }
}