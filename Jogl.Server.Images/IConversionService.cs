using Jogl.Server.Data.Util;

namespace Jogl.Server.Images
{
    public interface IConversionService
    {
        Task<FileData> ConvertAsync(ImageConversion conversion);
    }
}