using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Mvc;
using Jogl.Server.Images;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Jogl.Server.API.Controllers
{
    [ApiController]
    [Route("images")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IConversionService _conversionService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public ImageController(IImageService imageService, IConversionService conversionService, IMapper mapper, ILogger<UserController> logger, IEntityService entityService, IContextService contextService)
        {
            _imageService = imageService;
            _conversionService = conversionService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Image data", typeof(ImageInsertResultModel))]
        public async Task<IActionResult> Upload(ImageInsertModel model)
        {
            var image = _mapper.Map<Image>(model);
            var id = await _imageService.CreateAsync(image);
            var url = _mapper.Map<string>(image);

            return Ok(new ImageInsertResultModel
            {
                Id = id,
                Url = url
            });
        }

        [HttpGet]
        [Route("{id}/full")]
        public async Task<IActionResult> GetImage(string id)
        {
            var image = await _imageService.GetAsync(id);
            return File(image.Data, image.Filetype);
        }

        [HttpGet]
        [Route("{id}/full/model")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Image data", typeof(ImageModel))]
        public async Task<IActionResult> GetImageModel(string id)
        {
            var image = await _imageService.GetAsync(id);
            var imageModel = _mapper.Map<ImageModel>(image);
            return Ok(imageModel);
        }

        [HttpGet]
        [Route("{id}/tn")]
        public async Task<IActionResult> GetImageThumbnail(string id)
        {
            var image = await _imageService.GetAsync(id, true);
            return File(image.Data, image.Filetype);
        }

        [HttpGet]
        [Route("{id}/tn/model")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Image data", typeof(ImageModel))]
        public async Task<IActionResult> GetImageThumbnailModel(string id)
        {
            var image = await _imageService.GetAsync(id, true);
            var imageModel = _mapper.Map<ImageModel>(image);
            return Ok(imageModel);
        }

        [HttpPost]
        [Route("convert")]
        public async Task<IActionResult> ConvertImage([FromBody] ImageConversionUpsertModel model)
        {
            var conversion = _mapper.Map<ImageConversion>(model);
            var file = await _conversionService.ConvertAsync(conversion);
            var imageModel = _mapper.Map<string>(file);

            return Ok(imageModel);
        }
    }
}