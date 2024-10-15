using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface IImageService
    {
        Task<string> CreateAsync(Image image);
        Task<Image> GetAsync(string imageId, bool tn=false);
    }
}