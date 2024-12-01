using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.URL;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("organizations")]
    public class OrganizationController : BaseCommunityEntityController<Organization, OrganizationModel, OrganizationDetailModel, CommunityEntityMiniModel, OrganizationUpsertModel, OrganizationPatchModel>
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly INodeService _nodeService;
        private readonly IOrganizationService _organizationService;

        public OrganizationController( IWorkspaceService workspaceService, INodeService nodeService, IOrganizationService organizationService, INeedService needService, IInvitationService invitationService, IAccessService accessService, IMembershipService membershipService, IUserService userService, IDocumentService documentService, ICommunityEntityService communityEntityService, ICommunityEntityInvitationService communityEntityInvitationService, ICommunityEntityMembershipService communityEntityMembershipService, IContentService contentService, IEventService eventService, IChannelService channelService, IPaperService paperService, IResourceService resourceService, IUrlService urlService, IConfiguration configuration, IMapper mapper, ILogger<OrganizationController> logger, IEntityService entityService, IContextService contextService) : base(accessService, invitationService, membershipService, userService, documentService, communityEntityService, communityEntityInvitationService, communityEntityMembershipService, contentService, eventService, channelService, paperService, resourceService, needService, urlService, configuration, mapper, logger, entityService, contextService)
        {
            _workspaceService = workspaceService;
            _nodeService = nodeService;
            _organizationService = organizationService;
        }

        protected override CommunityEntityType EntityType => CommunityEntityType.Organization;

        protected async override Task<string> CreateEntityAsync(Organization c)
        {
            return await _organizationService.CreateAsync(c);
        }

        protected async override Task DeleteEntity(string id)
        {
            await _organizationService.DeleteAsync(id);
        }

        protected override Organization GetEntity(string id)
        {
            return _organizationService.Get(id, CurrentUserId);
        }

        protected override Organization GetEntityDetail(string id)
        {
            return _organizationService.GetDetail(id, CurrentUserId);
        }

        protected override List<Organization> Autocomplete(string userId, string search, int page, int pageSize)
        {
            return _organizationService.Autocomplete(userId, search, page, pageSize);
        }

        protected override ListPage<Organization> List(string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            return _organizationService.List(CurrentUserId, search, page, pageSize, sortKey, ascending);
        }

        protected async override Task UpdateEntityAsync(Organization c)
        {
            await _organizationService.UpdateAsync(c);
        }

        protected override List<Workspace> ListCommunities(string id, string search, int page, int pageSize)
        {
            return _workspaceService.ListForOrganization(CurrentUserId, id, search, page, pageSize);
        }

        protected override List<Data.Node> ListNodes(string id, string search, int page, int pageSize)
        {
            return _nodeService.ListForOrganization(CurrentUserId, id, search, page, pageSize);
        }

        protected override List<Organization> ListOrganizations(string id, string search, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        protected override List<Resource> ListResources(string id, string search, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        protected override ListPage<Event> ListEventsAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            return new ListPage<Event>(new List<Event>());// _eventService.ListForOrganization(id, CurrentUserId, types, communityEntityIds, tags, from, to, search, page, pageSize, sortKey, ascending);
        }

        protected override List<CommunityEntity> ListEcosystem(string id, string search, int page, int pageSize)
        {
            return new List<CommunityEntity>();
        }

        [HttpPost]
        [SwaggerOperation($"Create a new organization. The current user becomes a member of the organization with the {nameof(AccessLevel.Owner)} role")]
        public async override Task<IActionResult> Create([FromBody] OrganizationUpsertModel model)
        {
            return await base.Create(model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation("Returns an organization")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No organization was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The organization data", typeof(OrganizationModel))]
        public async override Task<IActionResult> Get([SwaggerParameter("ID of the organization")] string id)
        {
            return await base.Get(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/detail")]
        [SwaggerOperation("Returns an organization including detailed stats")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No organization was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The organization data including detailed stats", typeof(OrganizationDetailModel))]
        public async override Task<IActionResult> GetDetail([SwaggerParameter("ID of the organization")] string id)
        {
            return await base.GetDetail(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("autocomplete")]
        [SwaggerOperation("List the basic information of organizations for a given search query. Only organizations accessible to the currently logged in user will be returned")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async override Task<IActionResult> Autocomplete([FromQuery] SearchModel model)
        {
            return await base.Autocomplete(model);
        }

        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation("List all accessible organizations for a given search query. Only organizations accessible to the currently logged in user will be returned")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ListPage<CommunityEntityMiniModel>))]
        public async override Task<IActionResult> Search([FromQuery] SearchModel model)
        {
            return await base.Search(model);
        }

        [HttpPatch]
        [Route("{id}")]
        [SwaggerOperation($"Patches the specified organization")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No organization was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this organization")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
        public async override Task<IActionResult> Patch([SwaggerParameter("ID of the organization")][FromRoute] string id, [FromBody] OrganizationPatchModel model)
        {
            return await base.Patch(id, model);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the specified organization")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No organization was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this organization")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
        public async override Task<IActionResult> Update([SwaggerParameter("ID of the organization")][FromRoute] string id, [FromBody] OrganizationUpsertModel model)
        {
            return await base.Update(id, model);
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes the specified organization and removes all of the organization's associations from communities and nodes")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No organization was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to delete this organization")]
        public async override Task<IActionResult> Delete([SwaggerParameter("ID of the organization")][FromRoute] string id)
        {
            return await base.Delete(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/workspaces")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No organization was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this organization's content", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "Workspace data", typeof(List<WorkspaceModel>))]
        public async Task<IActionResult> GetSpaces([SwaggerParameter("ID of the organization")] string id, [FromQuery] SearchModel model)
        {
            return await GetCommunitiesAsync(id, model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/nodes")]
        [SwaggerOperation($"Lists all events for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No organization was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this organization's content", typeof(string))]
        public async Task<IActionResult> GetNodes([SwaggerParameter("ID of the organization")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            return await GetNodesAsync(id, model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/events/aggregate")]
        [SwaggerOperation($"Lists all events for the specified organization and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No organization was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this organization's content", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "Event data", typeof(ListPage<EventModel>))]
        public async Task<IActionResult> GetEventsAggregate([SwaggerParameter("ID of the organization")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] List<string> communityEntityIds, [FromQuery] List<EventTag> tags, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] SearchModel model)
        {
            return await GetEventsAggregateAsync(id, types, communityEntityIds, false, tags, from, to, model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/events/aggregate/communityEntities")]
        [SwaggerOperation($"Lists all community entities for events for the specified org and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No org was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this org's content")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetEventCommunityEntities([SwaggerParameter("ID of the org")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] bool currentUser, [FromQuery] List<EventTag> tags, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communityEntities = _eventService.ListCommunityEntitiesForOrgEvents(id, CurrentUserId, types, currentUser, tags, from, to, model.Search, model.Page, model.PageSize);
            var communityEntityModels = communityEntities.Select(_mapper.Map<CommunityEntityMiniModel>);
            return Ok(communityEntityModels);
        }

        //[HttpGet]
        //[AllowAnonymous]
        //[Route("migrations/bots")]
        //public async Task<IActionResult> PurgeBotOrgs()
        //{
        //    foreach (var id in System.IO.File.ReadAllLines("bots.txt"))
        //    {
        //        await _organizationService.DeleteAsync(id);
        //    }

        //    return Ok();
        //}
    }
}