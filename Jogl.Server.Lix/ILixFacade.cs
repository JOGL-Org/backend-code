using Jogl.Server.Lix.DTO;

namespace Jogl.Server.Lix
{
    public interface ILixFacade
    {
        Task<Profile> GetProfileAsync(string linkedInUrl);
    }
}