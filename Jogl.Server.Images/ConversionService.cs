using ImageMagick;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Images
{
    public class ConversionService : IConversionService
    {
        public async Task<FileData> ConvertAsync(ImageConversion conversion)
        {
            using (var stream = new MemoryStream())
            using (var image = new MagickImage(conversion.Data.Data))
            {
                image.Format = GetFormat(conversion.FormatTo);
                image.Write(stream);

                return new FileData { Data = stream.ToArray(), Filetype = conversion.FormatTo };
            }
        }

        private MagickFormat GetFormat(string format)
        {
            switch (format)
            {
                case "image/heic":
                    return MagickFormat.Heic;
                case "image/heif":
                    return MagickFormat.Heif;
                case "image/jpeg":
                    return MagickFormat.Jpg;
                case "image/jpg":
                    return MagickFormat.Jpeg;
                case "image/png":
                    return MagickFormat.Png;
                default:
                    throw new Exception($"Unknown image format {format}");
            }
        }
    }
}