using Jogl.Server.Data.Util;
using Jogl.Server.Data;
using Jogl.Server.Storage;
using System.Text.RegularExpressions;
using Jogl.Server.DB;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Business
{
    public class EntityService : IEntityService
    {
        protected readonly IImageRepository _imageRepository;
        protected readonly IStorageService _storageService;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly ILogger<EntityService> _logger;

        public EntityService(IImageRepository imageRepository, IStorageService storageService, IHttpContextAccessor httpContextAccessor, ILogger<EntityService> logger)
        {

            _imageRepository = imageRepository;
            _storageService = storageService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task ProcessEmbeddedDataAsync(Entity entity)
        {
            if (entity == null)
                return;

            try
            {
                await ProcessEmbeddedData(entity, entity);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during embedded data processing: " + ex.ToString());
            }
        }

        private async Task ProcessEmbeddedData(Entity parentEntity, object obj)
        {
            if (parentEntity == null || obj == null)
                return;

            var type = obj.GetType();
            foreach (var prop in type.GetProperties())
            {
                if (prop.GetCustomAttribute<BsonIgnoreAttribute>() != null)
                    continue;

                if (prop.GetCustomAttribute<RichTextAttribute>() != null)
                {
                    var stringValue = prop.GetValue(obj) as string;
                    var listValue = prop.GetValue(obj) as IEnumerable<string>;
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        prop.SetValue(obj, await ProcessEmbeddedDataInField(parentEntity, stringValue));
                    }
                    else if (listValue != null && listValue.Any())
                    {
                        prop.SetValue(obj, listValue.Select(async itemValue => await ProcessEmbeddedDataInField(parentEntity, itemValue)).ToList());
                    }
                }
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string) && !prop.PropertyType.IsAssignableTo(typeof(IEnumerable<string>)))
                {
                    if (prop.PropertyType.IsAssignableTo(typeof(IEnumerable<object>)))
                    {
                        var listValue = prop.GetValue(obj) as IEnumerable<object>;
                        if (listValue == null || !listValue.Any())
                            continue;

                        foreach (var itemValue in listValue)
                        {
                            await ProcessEmbeddedData(parentEntity, itemValue);
                        }
                    }
                    else
                    {
                        var childObj = prop.GetValue(obj);
                        if (childObj == null)
                            continue;

                        await ProcessEmbeddedData(parentEntity, childObj);
                    }
                }
            }
        }

        private async Task<string> ProcessEmbeddedDataInField(Entity entity, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldValue))
                return fieldValue;

            var pattern = $"(data:image\\/[^;]+;base64[^\"]+)";
            var i = 0;
            foreach (Match match in Regex.Matches(fieldValue, pattern))
            {
                var url = match.Groups[0].Value;
                var bytes = Convert.FromBase64String(url.Substring(url.IndexOf(",") + 1));
                var type = url.Substring(5, url.IndexOf(";") - 5);

                var imageId = await _imageRepository.CreateAsync(new Image
                {
                    Data = bytes,
                    CreatedByUserId = entity.UpdatedByUserId ?? entity.CreatedByUserId,
                    CreatedUTC = entity.UpdatedUTC ?? entity.CreatedUTC,
                    Filename = entity.Id.ToString() + "_" + i++,
                    Filetype = type,
                });

                var req = _httpContextAccessor.HttpContext.Request;
                var apiUrl = req.Scheme + "://" + req.Host;

                await _storageService.CreateOrReplaceAsync(IStorageService.IMAGE_CONTAINER, imageId, bytes);
                fieldValue = fieldValue.Replace(match.Groups[0].Value, $"{apiUrl}/images/{imageId}/full");
            }

            return fieldValue;
        }
    }
}
