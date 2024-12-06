using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.API.Services;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
using Jogl.Server.URL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("workspaces")]
    public class WorkspaceController : BaseCommunityEntityController<Workspace, WorkspaceModel, WorkspaceDetailModel, CommunityEntityMiniModel, WorkspaceUpsertModel, WorkspacePatchModel>
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly INodeService _nodeService;
        private readonly IOrganizationService _organizationService;
        private readonly ICallForProposalService _callForProposalsService;

        public WorkspaceController(IWorkspaceService workspaceService, INodeService nodeService, IOrganizationService organizationService, ICallForProposalService callForProposalsService, INeedService needService, IInvitationService invitationService, IAccessService accessService, IMembershipService membershipService, IUserService userService, IDocumentService documentService, ICommunityEntityService communityEntityService, ICommunityEntityInvitationService communityEntityInvitationService, ICommunityEntityMembershipService communityEntityMembershipService, IContentService contentService, IEventService eventService, IChannelService channelService, IPaperService paperService, IResourceService resourceService, IUrlService urlService, IConfiguration configuration, IMapper mapper, ILogger<WorkspaceController> logger, IEntityService entityService, IContextService contextService) : base(accessService, invitationService, membershipService, userService, documentService, communityEntityService, communityEntityInvitationService, communityEntityMembershipService, contentService, eventService, channelService, paperService, resourceService, needService, urlService, configuration, mapper, logger, entityService, contextService)
        {
            _workspaceService = workspaceService;
            _nodeService = nodeService;
            _organizationService = organizationService;
            _callForProposalsService = callForProposalsService;
        }

        protected override CommunityEntityType EntityType => CommunityEntityType.Workspace;

        protected async override Task<string> CreateEntityAsync(Workspace c)
        {
            return await _workspaceService.CreateAsync(c);
        }

        protected async override Task DeleteEntity(string id)
        {
            await _workspaceService.DeleteAsync(id);
        }

        protected override Workspace GetEntity(string id)
        {
            return _workspaceService.Get(id, CurrentUserId);
        }

        protected override Workspace GetEntityDetail(string id)
        {
            return _workspaceService.GetDetail(id, CurrentUserId);
        }

        protected override List<Workspace> Autocomplete(string userId, string search, int page, int pageSize)
        {
            return _workspaceService.Autocomplete(userId, search, page, pageSize);
        }

        protected override ListPage<Workspace> List(string search, int page, int pageSize, SortKey sort, bool ascending)
        {
            return _workspaceService.List(CurrentUserId, search, page, pageSize, sort, ascending);
        }

        protected async override Task UpdateEntityAsync(Workspace c)
        {
            await _workspaceService.UpdateAsync(c);
        }

        protected override List<Workspace> ListCommunities(string id, string search, int page, int pageSize)
        {
            return _workspaceService.ListForWorkspace(CurrentUserId, id, search, page, pageSize);
        }

        protected override List<Data.Node> ListNodes(string id, string search, int page, int pageSize)
        {
            return _nodeService.ListForCommunity(CurrentUserId, id, search, page, pageSize);
        }

        protected override List<Organization> ListOrganizations(string id, string search, int page, int pageSize)
        {
            return _organizationService.ListForCommunity(CurrentUserId, id, search, page, pageSize);
        }

        protected override List<Resource> ListResources(string id, string search, int page, int pageSize)
        {
            return _resourceService.ListForFeed(id, search, page, pageSize);
        }

        protected override List<CommunityEntity> ListEcosystem(string id, string search, int page, int pageSize)
        {
            return _nodeService.ListForCommunity(CurrentUserId, id, search, page, pageSize).Cast<CommunityEntity>().ToList();
        }

        [HttpPost]
        [SwaggerOperation($"Create a new workspace. The current user becomes a member of the workspace with the {nameof(AccessLevel.Owner)} role")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The user does not have the permission to create workspaces in the target community entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The workspace id", typeof(string))]
        public async override Task<IActionResult> Create([FromBody] WorkspaceUpsertModel model)
        {
            var entity = _mapper.Map<Workspace>(model);
            await InitCreationAsync(entity);
            var id = await CreateEntityAsync(entity);

            var parent = _communityEntityService.GetEnriched(model.ParentId, CurrentUserId);
            if (!parent.Permissions.Contains(Permission.CreateWorkspaces))
                return Forbid();

            var relation = new Relation
            {
                SourceCommunityEntityId = id,
                SourceCommunityEntityType = CommunityEntityType.Workspace,
                TargetCommunityEntityId = parent.Id.ToString(),
                TargetCommunityEntityType = parent.Type
            };

            await InitCreationAsync(relation);
            await _communityEntityMembershipService.CreateAsync(relation);

            return Ok(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation("Returns a single workspace")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The workspace data", typeof(WorkspaceModel))]
        public async override Task<IActionResult> Get([SwaggerParameter("ID of the workspace")] string id)
        {
            return await base.Get(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/detail")]
        [SwaggerOperation("Returns a single workspace including detailed stats")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The workspace data including detailed stats", typeof(WorkspaceDetailModel))]
        public async override Task<IActionResult> GetDetail([SwaggerParameter("ID of the workspace")] string id)
        {
            return await base.GetDetail(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("autocomplete")]
        [SwaggerOperation("List the basic information of workspaces for a given search query. Only workspaces accessible to the currently logged in user will be returned")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async override Task<IActionResult> Autocomplete([FromQuery] SearchModel model)
        {
            return await base.Autocomplete(model);
        }

        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation("List all accessible workspaces for a given search query. Only workspaces accessible to the currently logged in user will be returned")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ListPage<CommunityEntityMiniModel>))]
        public async override Task<IActionResult> Search([FromQuery] SearchModel model)
        {
            return await base.Search(model);
        }

        [HttpPatch]
        [Route("{id}")]
        [SwaggerOperation($"Patches the specified workspace")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this workspace")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
        public async override Task<IActionResult> Patch([SwaggerParameter("ID of the workspace")][FromRoute] string id, [FromBody] WorkspacePatchModel model)
        {
            return await base.Patch(id, model);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the specified workspace")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this workspace")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
        public async override Task<IActionResult> Update([SwaggerParameter("ID of the workspace")][FromRoute] string id, [FromBody] WorkspaceUpsertModel model)
        {
            return await base.Update(id, model);
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes the specified workspace and removes all of the community's associations from workspaces and nodes")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to delete this workspace")]
        public async override Task<IActionResult> Delete([SwaggerParameter("ID of the workspace")][FromRoute] string id)
        {
            return await base.Delete(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/nodes")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this workspace's content", typeof(string))]
        public async Task<IActionResult> GetNodes([SwaggerParameter("ID of the workspace")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            return await GetNodesAsync(id, model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/organizations")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this workspace's content", typeof(string))]
        public async Task<IActionResult> GetOrganizations([SwaggerParameter("ID of the workspace")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            return await GetOrganizationsAsync(id, model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/cfps")]
        [SwaggerOperation($"Lists all calls for proposals for the workspace")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this workspace's contents", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CallForProposalModel>))]
        public async Task<IActionResult> GetCallsForProposals([SwaggerParameter("ID of the workspace")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var workspace = _workspaceService.Get(id, CurrentUserId);
            if (workspace == null)
                return NotFound();

            var cfps = _callForProposalsService.ListForCommunity(CurrentUserId, id, model.Search, model.Page, model.PageSize);
            var cfpModels = cfps.Select(_mapper.Map<CallForProposalMiniModel>);
            return Ok(cfpModels);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/papers/aggregate")]
        [SwaggerOperation($"Lists all papers for the specified workspace and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this workspace's content", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<PaperModel>))]
        public async Task<IActionResult> GetPapersAggregate([SwaggerParameter("ID of the workspace")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] List<string> communityEntityIds, [FromQuery] PaperType? type, [FromQuery] List<PaperTag> tags, [FromQuery] SearchModel model)
        {
            return Ok(new List<PaperModel>());
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/papers/aggregate/communityEntities")]
        [SwaggerOperation($"Lists all community entities for papers for the specified workspace and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this workspace's content")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetPaperCommunityEntities([SwaggerParameter("ID of the workspace")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] PaperType? type, [FromQuery] List<PaperTag> tags, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communityEntityModels = new List<CommunityEntityMiniModel>();
            return Ok(communityEntityModels);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/needs/aggregate")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this workspace's content", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<NeedModel>))]
        public async Task<IActionResult> GetNeedsAggregate([SwaggerParameter("ID of the workspace")] string id, [FromQuery] List<string> communityEntityIds, [FromQuery] SearchModel model)
        {
            return await GetNeeds(id, model);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/needs/aggregate/communityEntities")]
        [SwaggerOperation($"Lists all community entities for needs for the specified workspace and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this workspace's content")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetNeedCommunityEntities([SwaggerParameter("ID of the workspace")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] bool currentUser, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communityEntityModels = new List<CommunityEntityMiniModel>();
            return Ok(communityEntityModels);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/workspaces")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No workspace was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this workspace's content", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "Workspace data", typeof(List<WorkspaceModel>))]
        public async Task<IActionResult> GetSpaces([SwaggerParameter("ID of the workspace")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            return await GetCommunitiesAsync(id, model);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("paper")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
        public async Task<IActionResult> ListForExternalId([FromQuery][SwaggerParameter("External ID")] string externalID)
        {
            var projects = _workspaceService.ListForPaperExternalId(CurrentUserId, WebUtility.UrlDecode(externalID));
            var projectModels = projects.Select(_mapper.Map<CommunityEntityMiniModel>);
            return Ok(projectModels);
        }
    }
}