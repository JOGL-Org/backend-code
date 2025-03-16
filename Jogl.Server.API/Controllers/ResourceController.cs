using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("resources")]
    public class ResourceController : BaseController
    {
        private readonly IResourceService _resourceService;
        private readonly ICommunityEntityService _communityEntityService;

        public ResourceController(IResourceService resourceService, ICommunityEntityService communityEntityService, IMapper mapper, ILogger<EventController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _resourceService = resourceService;
            _communityEntityService = communityEntityService;
        }

        [HttpPost]
        [Route("{entityId}/resources")]
        [SwaggerOperation($"Adds a new resource for the specified feed.")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add resource for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The resource was created", typeof(string))]
        public async Task<IActionResult> AddResource([FromRoute] string entityId, [FromBody] ResourceUpsertModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.PostResources, CurrentUserId))
                return Forbid();

            var resource = _mapper.Map<Resource>(model);
            resource.EntityId = entityId;

            await InitCreationAsync(resource);
            var id = await _resourceService.CreateAsync(resource);
            return Ok(id);
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation($"Lists all resources")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of resources matching the search query visible to the current user", typeof(ListPage<ResourceModel>))]
        public async Task<IActionResult> Search([FromQuery] SearchModel model)
        {
            var resources = _resourceService.List(CurrentUserId, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var resourceModels = resources.Items.Select(_mapper.Map<ResourceModel>);
            return Ok(new ListPage<ResourceModel>(resourceModels, resources.Total));
        }

        //[HttpGet]
        //[AllowAnonymous]
        //[Route("count")]
        //[SwaggerOperation($"Counts resources")]
        //[SwaggerResponse((int)HttpStatusCode.OK, "A count of resources matching the search query visible to the current user", typeof(long))]
        //public async Task<IActionResult> Count([FromQuery] SearchModel model)
        //{
        //    var count = _resourceService.Count(CurrentUserId, model.Search);
        //    return Ok(count);
        //}

        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation($"Returns a single resource")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No resource was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the resource")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ResourceModel))]
        public async Task<IActionResult> GetResource([FromRoute] string id)
        {
            var resource = _resourceService.Get(id, CurrentUserId);
            if (resource == null)
                return NotFound();

            if (!resource.Permissions.Contains(Permission.Read))
                return Forbid();

            var eventModel = _mapper.Map<ResourceModel>(resource);
            return Ok(eventModel);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{entityId}/resources")]
        [SwaggerOperation($"Lists all resources for the specified entity")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view resources for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The resource data", typeof(List<ResourceModel>))]
        public async Task<IActionResult> GetResources([FromRoute] string entityId, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.Read, CurrentUserId))
                return Forbid();

            var resources = _resourceService.ListForEntity(CurrentUserId, entityId, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var resourceModels = resources.Select(_mapper.Map<ResourceModel>);
            return Ok(resourceModels);
        }

        //[HttpGet]
        //[Route("{entityId}/resources/new")]
        //[SwaggerOperation($"Returns a value indicating whether or not there are new resources for a particular entity")]
        //[SwaggerResponse((int)HttpStatusCode.OK, $"True or false", typeof(bool))]
        //public async Task<IActionResult> GetResourcesHasNew([FromRoute] string entityId)
        //{
        //    if (!_communityEntityService.HasPermission(entityId, Permission.Read, CurrentUserId))
        //        return Forbid();

        //    var res = _resourceService.ListForEntityHasNew(CurrentUserId, entityId);
        //    return Ok(res);
        //}

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the resource")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No resource was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit the resource")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The resource was updated")]
        public async Task<IActionResult> UpdateResource([FromRoute] string id, [FromBody] ResourceUpsertModel model)
        {
            var existingResource = _resourceService.Get(id, CurrentUserId);
            if (existingResource == null)
                return NotFound();

            if (!existingResource.Permissions.Contains(Permission.Manage))
                return Forbid();

            var resource = _mapper.Map<Resource>(model);
            resource.Id = ObjectId.Parse(id);
            resource.EntityId = existingResource.EntityId;
            await InitUpdateAsync(resource);
            await _resourceService.UpdateAsync(resource);

            return Ok();
        }


        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes a resource")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No resource was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete this resource")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The resource was deleted")]
        public async Task<IActionResult> DeleteResource([FromRoute] string id)
        {
            var resource = _resourceService.Get(id, CurrentUserId);
            if (resource == null)
                return NotFound();

            if (!resource.Permissions.Contains(Permission.Manage))
                return Forbid();

            await _resourceService.DeleteAsync(resource);
            return Ok();
        }
    }
}