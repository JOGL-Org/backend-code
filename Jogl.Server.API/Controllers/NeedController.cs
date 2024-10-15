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
    [Route("needs")]
    public class NeedController : BaseController
    {
        private readonly INeedService _needService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IUserService _userService;
        private readonly IDocumentService _documentService;
        private readonly IMembershipService _membershipService;
        private readonly IConfiguration _configuration;

        public NeedController(INeedService needService, ICommunityEntityService communityEntityService, IUserService userService, IDocumentService documentService, IMembershipService membershipService, IConfiguration configuration, IMapper mapper, ILogger<EventController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _needService = needService;
        }

        [HttpPost]
        [Route("{entityId}/needs")]
        [SwaggerOperation($"Adds a new need for the specified feed.")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add need for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The need was created", typeof(string))]
        public async Task<IActionResult> AddNeed([FromRoute] string entityId, [FromBody] NeedUpsertModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.PostNeed, CurrentUserId))
                return Forbid();

            var need = _mapper.Map<Need>(model);
            need.FeedId = entityId;

            await InitCreationAsync(need);
            var id = await _needService.CreateAsync(need);
            return Ok(id);
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation($"Lists all needs")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of needs matching the search query visible to the current user", typeof(ListPage<NeedModel>))]
        public async Task<IActionResult> Search([FromQuery] SearchModel model)
        {
            var needs = _needService.List(CurrentUserId, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var needModels = needs.Items.Select(_mapper.Map<NeedModel>);
            return Ok(new ListPage<NeedModel>(needModels, needs.Total));
        }

        //[HttpGet]
        //[AllowAnonymous]
        //[Route("count")]
        //[SwaggerOperation($"Counts needs")]
        //[SwaggerResponse((int)HttpStatusCode.OK, "A count of needs matching the search query visible to the current user", typeof(long))]
        //public async Task<IActionResult> Count([FromQuery] SearchModel model)
        //{
        //    var count = _needService.Count(CurrentUserId, model.Search);
        //    return Ok(count);
        //}

        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation($"Returns a single need")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No need was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the need")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(NeedModel))]
        public async Task<IActionResult> GetNeed([FromRoute] string id)
        {
            var need = _needService.Get(id, CurrentUserId);
            if (need == null)
                return NotFound();

            if (!need.Permissions.Contains(Permission.Read))
                return Forbid();

            var eventModel = _mapper.Map<NeedModel>(need);
            return Ok(eventModel);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{entityId}/needs")]
        [SwaggerOperation($"Lists all needs for the specified entity")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view needs for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The need data", typeof(List<NeedModel>))]
        public async Task<IActionResult> GetNeeds([FromRoute] string entityId, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.Read, CurrentUserId))
                return Forbid();

            var needs = _needService.ListForEntity(CurrentUserId, entityId, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var needModels = needs.Select(_mapper.Map<NeedModel>);
            return Ok(needModels);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the need")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No need was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit the need")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The need was updated")]
        public async Task<IActionResult> UpdateNeed([FromRoute] string id, [FromBody] NeedUpsertModel model)
        {
            var existingNeed = _needService.Get(id, CurrentUserId);
            if (existingNeed == null)
                return NotFound();

            if (!existingNeed.Permissions.Contains(Permission.Manage))
                return Forbid();

            var need = _mapper.Map<Need>(model);
            need.Id = ObjectId.Parse(id);
            need.EntityId = existingNeed.EntityId;
            await InitUpdateAsync(need);
            await _needService.UpdateAsync(need);

            return Ok();
        }


        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes a need")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No need was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete this need")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The need was deleted")]
        public async Task<IActionResult> DeleteNeed([FromRoute] string id)
        {
            var need = _needService.Get(id, CurrentUserId);
            if (need == null)
                return NotFound();

            if (!need.Permissions.Contains(Permission.Manage))
                return Forbid();

            await _needService.DeleteAsync(id);
            return Ok();
        }
    }
}
