using AutoMapper;
using Jogl.Server.API.Services;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Controllers
{
    public class BaseController : ControllerBase
    {
        protected string? CurrentUserId => _contextService.CurrentUserId;

        protected readonly IEntityService _entityService;
        protected readonly IContextService _contextService;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        public BaseController(IEntityService entityService, IContextService contextService, IMapper mapper, ILogger logger)
        {
            _entityService = entityService;
            _contextService = contextService;
            _mapper = mapper;
            _logger = logger;
        }

        protected async Task InitCreationAsync(Entity entity)
        {
            entity.CreatedByUserId = CurrentUserId;
            entity.CreatedUTC = DateTime.UtcNow;

            await _entityService.ProcessEmbeddedDataAsync(entity);
        }

        protected async Task InitCreationAsync(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                await InitCreationAsync(entity);
            }
        }

        protected async Task InitUpdateAsync(Entity entity)
        {
            entity.UpdatedByUserId = CurrentUserId;
            entity.UpdatedUTC = DateTime.UtcNow;

            await _entityService.ProcessEmbeddedDataAsync(entity);
        }

        protected async Task InitUpdateAsync(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                await InitUpdateAsync(entity);
            }
        }

        protected void ApplyPatchModel<TPatch, TPUpsert>(TPatch patchObject, TPUpsert upsertObject)
        {
            var patchType = typeof(TPatch);
            var upsertType = typeof(TPUpsert);

            var jo = HttpContext.Items["jo"] as JsonObject;
            foreach (var child in jo)
            {
                var patchProp = patchType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Where(p => Attribute.IsDefined(p, typeof(JsonPropertyNameAttribute)))
                    .Where(p => p.GetCustomAttribute<JsonPropertyNameAttribute>().Name == child.Key)
                    .SingleOrDefault();
                
                var upsertProp = upsertType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Where(p => Attribute.IsDefined(p, typeof(JsonPropertyNameAttribute)))
                    .Where(p => p.GetCustomAttribute<JsonPropertyNameAttribute>().Name == child.Key)
                    .SingleOrDefault();

                if (patchProp == null)
                    continue;

                if (upsertProp == null)
                    continue;

                var val = patchProp.GetValue(patchObject);
                upsertProp.SetValue(upsertObject, val);
            }
        }
    }
}