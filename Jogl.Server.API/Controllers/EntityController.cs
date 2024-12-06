using AutoMapper;
using Jogl.Server.Business;
using Jogl.Server.Data.Enum;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.API.Model;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("entities")]
    public class EntityController : BaseController
    {
        private readonly IContentService _contentService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly INeedService _needService;
        private readonly INodeService _nodeService;
        private readonly IOrganizationService _organizationService;
        private readonly IConfiguration _configuration;

        public EntityController(IContentService contentService, ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IUserService userService, IEventService eventService, INeedService needService, INodeService nodeService, IOrganizationService organizationService, IConfiguration configuration, IMapper mapper, ILogger<EntityController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _contentService = contentService;
            _communityEntityService = communityEntityService;
            _feedEntityService = feedEntityService;
            _userService = userService;
            _eventService = eventService;
            _needService = needService;
            _nodeService = nodeService;
            _organizationService = organizationService;
            _configuration = configuration;
        }
        
        [Obsolete]
        [HttpGet]
        [Route("permission/{id}/{permission}")]
        [SwaggerOperation($"Determines whether the current user has a given permission on a specified object")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"The given object was not found")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"True or false", typeof(bool))]
        public async Task<IActionResult> HasPermission([FromRoute] string id, [FromRoute] Permission permission)
        {
            var feed = _contentService.GetFeed(id);
            if (feed == null)
                return NotFound();

            return Ok(_communityEntityService.HasPermission(id, permission, CurrentUserId));
        }

        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation($"Returns a single entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"Not entity was found for given id")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Entity data", typeof(EntityMiniModel))]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var data = _feedEntityService.GetEntity(id);
            if(data == null) return NotFound();
            var model = _mapper.Map<EntityMiniModel>(data);
            return Ok(model);
        }

        [HttpGet]
        [Route("{id}/permissions")]
        [SwaggerOperation($"Loads permissions on an object for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Permission data", typeof(List<Permission>))]
        public async Task<IActionResult> ListPermissions([FromRoute] string id)
        {
            return Ok(_communityEntityService.ListPermissions(id, CurrentUserId));
        }

        //[AllowAnonymous]
        [HttpGet]
        [Route("communityEntities")]
        [SwaggerOperation($"Lists all ecosystem containers (projects, communities and nodes) with a given permissioni")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetEcosystemCommunityEntities([SwaggerParameter("ID of a given community entity")][FromQuery] string? id, [SwaggerParameter("Target permission")][FromQuery] Permission? permission, [FromQuery] SearchModel model)
        {
            var communityEntities = _communityEntityService.List(id, CurrentUserId, permission, model.Search, model.Page, model.PageSize);
            var communityEntityModels = communityEntities.Select(_mapper.Map<CommunityEntityMiniModel>);
            return Ok(communityEntityModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("search/totals")]
        [SwaggerOperation($"Lists the totals for a global search query")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Search result totals", typeof(SearchResultGlobalModel))]
        public async Task<IActionResult> GetSearchResults([FromQuery] BaseSearchModel model)
        {
            var userTask = Task.Run(() => _userService.Count(CurrentUserId, model.Search));
            var eventTask = Task.Run(() => _eventService.Count(CurrentUserId, model.Search));
            var needTask = Task.Run(() => _needService.Count(CurrentUserId, model.Search));
            var nodeTask = Task.Run(() => _nodeService.Count(CurrentUserId, model.Search));
            var orgTask = Task.Run(() => _organizationService.Count(CurrentUserId, model.Search));

            await Task.WhenAll(userTask, eventTask, needTask, nodeTask, orgTask);

            return Ok(new SearchResultGlobalModel
            {
                UserCount = await userTask,
                EventCount = await eventTask,
                NeedCount = await needTask,
                NodeCount = await nodeTask,
                OrgCount = await orgTask,
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/path")]
        [SwaggerOperation($"Gets a path for an entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Path data", typeof(List<EntityMiniModel>))]
        public async Task<IActionResult> GetPath([FromRoute] string id)
        {
            var entities = _feedEntityService.GetPath(id, CurrentUserId);
            var entityModels = entities.Select(_mapper.Map<EntityMiniModel>);
            return Ok(entityModels);
        }
    }
}