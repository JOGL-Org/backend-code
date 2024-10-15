using Jogl.Server.DB;
using Jogl.Server.Storage;
using ImageProcessor.Imaging.Formats;
using System.Drawing;
using ImageProcessor;

namespace Jogl.Server.Business
{
    public class ImageService : IImageService
    {
        //private const int MAX_SIZE = 4 * 1024 * 1024;
        private readonly IImageRepository _imageRepository;
        private readonly IStorageService _storageService;
        public ImageService(IImageRepository imageRepository, IStorageService storageService)
        {
            _imageRepository = imageRepository;
            _storageService = storageService;
        }
        public async Task<string> CreateAsync(Data.Image image)
        {
            var imageId = await _imageRepository.CreateAsync(image);

            var originalData = image.Data;
            var thumbnailData = GenerateThumbnailData(image.Data);

            await _storageService.CreateOrReplaceAsync(IStorageService.IMAGE_CONTAINER, imageId, originalData);
            await _storageService.CreateOrReplaceAsync(IStorageService.IMAGE_CONTAINER, imageId + "_tn", thumbnailData);

            return imageId;
        }

        public async Task<Data.Image> GetAsync(string imageId, bool tn = false)
        {
            var image = _imageRepository.Get(imageId);
            if (image == null)
                return null;

            if (tn)
            {
                var exists = await _storageService.DocumentExistsAsync(IStorageService.IMAGE_CONTAINER, imageId + "_tn");
                //autogenerate TN if not existing
                if (!exists)
                {
                    var data = await _storageService.GetDocumentAsync(IStorageService.IMAGE_CONTAINER, imageId);
                    var thumbnailData = GenerateThumbnailData(data);
                    await _storageService.CreateOrReplaceAsync(IStorageService.IMAGE_CONTAINER, imageId + "_tn", thumbnailData);
                }

                image.Data = await _storageService.GetDocumentAsync(IStorageService.IMAGE_CONTAINER, imageId + "_tn");
            }
            else
                image.Data = await _storageService.GetDocumentAsync(IStorageService.IMAGE_CONTAINER, imageId);

            return image;
        }

        private byte[] GenerateThumbnailData(byte[] data)
        {
            var format = new PngFormat { Quality = 70 };
            var size = new Size(128, 0);
            using (MemoryStream inStream = new MemoryStream(data))
            using (MemoryStream outStream = new MemoryStream())
            {
                using (var imageFactory = new ImageFactory(preserveExifData: true))
                {
                    imageFactory.Load(inStream)
                                .Resize(size)
                                .Format(format)
                                .Save(outStream);
                }
                return outStream.ToArray();
            }
        }
    }
}