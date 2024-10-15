//using AutoMapper;
//using Jogl.Server.API.Model;
//using Jogl.Server.API.Services;
//using Jogl.Server.Business;
//using Jogl.Server.Data;
//using Jogl.Server.Data.Enum;
//using Jogl.Server.Data.Util;
//using Jogl.Server.URL;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Swashbuckle.AspNetCore.Annotations;
//using System.Net;

//namespace Jogl.Server.API.Controllers
//{
//    [Obsolete]
//    [Authorize]
//    [ApiController]
//    [Route("communities")]
//    public class CommunityController : CommunityEntityController<Community, CommunityModel, CommunityDetailModel, CommunityEntityMiniModel, CommunityUpsertModel, CommunityPatchModel>
//    {
//        private readonly IProjectService _projectService;
//        private readonly ICommunityService _workspaceService;
//        private readonly INodeService _nodeService;
//        private readonly IOrganizationService _organizationService;
//        private readonly ICallForProposalService _callForProposalsService;

//        public CommunityController(IProjectService projectService, ICommunityService workspaceService, INodeService nodeService, IOrganizationService organizationService, ICallForProposalService callForProposalsService, INeedService needService, IInvitationService invitationService, IAccessService accessService, IMembershipService membershipService, IUserService userService, IDocumentService documentService, ICommunityEntityService communityEntityService, ICommunityEntityInvitationService communityEntityInvitationService, ICommunityEntityMembershipService communityEntityMembershipService, IContentService contentService, IEventService eventService, IChannelService channelService, IPaperService paperService, IResourceService resourceService, IUrlService urlService, IConfiguration configuration, IMapper mapper, ILogger<CommunityController> logger, IEntityService entityService, IContextService contextService) : base(accessService, invitationService, membershipService, userService, documentService, communityEntityService, communityEntityInvitationService, communityEntityMembershipService, contentService, eventService, channelService, paperService, resourceService, needService, urlService, configuration, mapper, logger, entityService, contextService)
//        {
//            _projectService = projectService;
//            _workspaceService = workspaceService;
//            _nodeService = nodeService;
//            _organizationService = organizationService;
//            _callForProposalsService = callForProposalsService;
//        }

//        protected override CommunityEntityType EntityType => CommunityEntityType.Workspace;

//        protected async override Task<string> CreateEntityAsync(Community c)
//        {
//            return await _workspaceService.CreateAsync(c);
//        }

//        protected async override Task DeleteEntity(string id)
//        {
//            await _workspaceService.DeleteAsync(id);
//        }

//        protected override Community GetEntity(string id)
//        {
//            return _workspaceService.Get(id, CurrentUserId);
//        }

//        protected override Community GetEntityDetail(string id)
//        {
//            return _workspaceService.GetDetail(id, CurrentUserId);
//        }

//        protected override List<Community> Autocomplete(string userId, string search, int page, int pageSize)
//        {
//            return _workspaceService.Autocomplete(userId, search, page, pageSize);
//        }

//        protected override ListPage<Community> List(string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            return _workspaceService.List(CurrentUserId, search, page, pageSize, sortKey, ascending);
//        }

//        protected async override Task UpdateEntityAsync(Community c)
//        {
//            await _workspaceService.UpdateAsync(c);
//        }

//        protected override List<Project> ListProjects(string id, string search, int page, int pageSize)
//        {
//            return _projectService.ListForCommunity(CurrentUserId, id, search, page, pageSize);
//        }

//        protected override List<Community> ListCommunities(string id, string search, int page, int pageSize)
//        {
//            throw new NotImplementedException();
//        }

//        protected override List<Data.Node> ListNodes(string id, string search, int page, int pageSize)
//        {
//            return _nodeService.ListForCommunity(CurrentUserId, id, search, page, pageSize);
//        }

//        protected override List<Organization> ListOrganizations(string id, string search, int page, int pageSize)
//        {
//            return _organizationService.ListForCommunity(CurrentUserId, id, search, page, pageSize);
//        }

//        protected override List<Resource> ListResources(string id, string search, int page, int pageSize)
//        {
//            return _resourceService.ListForFeed(id, search, page, pageSize);
//        }

//        protected override ListPage<Paper> ListPapersAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, PaperType? type, List<PaperTag> tags, string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            return _paperService.ListForCommunity(CurrentUserId, id, types, communityEntityIds, type, tags, search, page, pageSize, sortKey, ascending);
//        }

//        protected override ListPage<Document> ListDocumentsAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, DocumentFilter? type, string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            throw new NotImplementedException();
//        }

//        protected override ListPage<Need> ListNeedsAggregate(string id, List<string> communityEntityIds, bool currentUser, string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            return _needService.ListForCommunity(CurrentUserId, id, communityEntityIds, search, page, pageSize, sortKey, ascending);
//        }

//        protected override ListPage<Event> ListEventsAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            throw new NotImplementedException();
//        }

//        protected override List<CommunityEntity> ListEcosystem(string id, string search, int page, int pageSize)
//        {
//            var projects = _projectService.ListForCommunity(CurrentUserId, id, search, page, pageSize).Cast<CommunityEntity>();
//            var nodes = _nodeService.ListForCommunity(CurrentUserId, id, search, page, pageSize).Cast<CommunityEntity>();

//            return projects.Union(nodes).ToList();
//        }

//        [HttpPost]
//        [SwaggerOperation($"Create a new community. The current user becomes a member of the community with the {nameof(AccessLevel.Owner)} role")]
//        public async override Task<IActionResult> Create([FromBody] CommunityUpsertModel model)
//        {
//            return await base.Create(model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}")]
//        [SwaggerOperation("Returns a single community")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "The community data", typeof(CommunityModel))]
//        public async override Task<IActionResult> Get([SwaggerParameter("ID of the community")] string id)
//        {
//            return await base.Get(id);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/detail")]
//        [SwaggerOperation("Returns a single community including detailed stats")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "The community data including detailed stats", typeof(CommunityDetailModel))]
//        public async override Task<IActionResult> GetDetail([SwaggerParameter("ID of the community")] string id)
//        {
//            return await base.GetDetail(id);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("autocomplete")]
//        [SwaggerOperation("List the basic information of communities for a given search query. Only communities accessible to the currently logged in user will be returned")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
//        public async override Task<IActionResult> Autocomplete([FromQuery] SearchModel model)
//        {
//            return await base.Autocomplete(model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [SwaggerOperation("List all accessible communities for a given search query. Only communities accessible to the currently logged in user will be returned")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ListPage<CommunityEntityMiniModel>))]
//        public async override Task<IActionResult> Search([FromQuery] SearchModel model)
//        {
//            return await base.Search(model);
//        }

//        [HttpPatch]
//        [Route("{id}")]
//        [SwaggerOperation($"Patches the specified community")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this community")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
//        public async override Task<IActionResult> Patch([SwaggerParameter("ID of the community")][FromRoute] string id, [FromBody] CommunityPatchModel model)
//        {
//            return await base.Patch(id, model);
//        }

//        [HttpPut]
//        [Route("{id}")]
//        [SwaggerOperation($"Updates the specified community")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this community")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
//        public async override Task<IActionResult> Update([SwaggerParameter("ID of the community")][FromRoute] string id, [FromBody] CommunityUpsertModel model)
//        {
//            return await base.Update(id, model);
//        }

//        [HttpDelete]
//        [Route("{id}")]
//        [SwaggerOperation($"Deletes the specified community and removes all of the community's associations from communities and nodes")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to delete this community")]
//        public async override Task<IActionResult> Delete([SwaggerParameter("ID of the community")][FromRoute] string id)
//        {
//            return await base.Delete(id);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/projects")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community's content", typeof(string))]
//        public async Task<IActionResult> GetProjects([SwaggerParameter("ID of the community")] string id, [FromQuery] SearchModel model)
//        {
//            return await GetProjectsAsync(id, model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/nodes")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community's content", typeof(string))]
//        public async Task<IActionResult> GetNodes([SwaggerParameter("ID of the community")][FromRoute] string id, [FromQuery] SearchModel model)
//        {
//            return await GetNodesAsync(id, model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/organizations")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community's content", typeof(string))]
//        public async Task<IActionResult> GetOrganizations([SwaggerParameter("ID of the community")][FromRoute] string id, [FromQuery] SearchModel model)
//        {
//            return await GetOrganizationsAsync(id, model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/cfps")]
//        [SwaggerOperation($"Lists all calls for proposals for the community")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community's contents", typeof(string))]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CallForProposalModel>))]
//        public async Task<IActionResult> GetCallsForProposals([SwaggerParameter("ID of the community")][FromRoute] string id, [FromQuery] SearchModel model)
//        {
//            var community = _workspaceService.Get(id, CurrentUserId);
//            if (community == null)
//                return NotFound();

//            var cfps = _callForProposalsService.ListForCommunity(CurrentUserId, id, model.Search, model.Page, model.PageSize);
//            var cfpModels = cfps.Select(_mapper.Map<CallForProposalMiniModel>);
//            return Ok(cfpModels);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/papers/aggregate")]
//        [SwaggerOperation($"Lists all papers for the specified community and its ecosystem")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community's content", typeof(string))]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<PaperModel>))]
//        public async Task<IActionResult> GetPapersAggregate([SwaggerParameter("ID of the community")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] List<string> communityEntityIds, [FromQuery] PaperType? type, [FromQuery] List<PaperTag> tags, [FromQuery] SearchModel model)
//        {
//            return await GetPapersAggregateAsync(id, types, communityEntityIds, type, tags, model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/papers/aggregate/communityEntities")]
//        [SwaggerOperation($"Lists all community entities for papers for the specified community and its ecosystem")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community's content")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
//        public async Task<IActionResult> GetPaperCommunityEntities([SwaggerParameter("ID of the community")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] PaperType? type, [FromQuery] List<PaperTag> tags, [FromQuery] SearchModel model)
//        {
//            var entity = GetEntity(id);
//            if (entity == null)
//                return NotFound();

//            if (!entity.Permissions.Contains(Permission.Read))
//                return Forbid();

//            var communityEntities = _paperService.ListCommunityEntitiesForCommunityPapers(CurrentUserId, id, types, type, tags, model.Search, model.Page, model.PageSize);
//            var communityEntityModels = communityEntities.Select(_mapper.Map<CommunityEntityMiniModel>);
//            return Ok(communityEntityModels);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/needs/aggregate")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community's content", typeof(string))]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<NeedModel>))]
//        public async Task<IActionResult> GetNeedsAggregate([SwaggerParameter("ID of the community")] string id, [FromQuery] List<string> communityEntityIds, [FromQuery] SearchModel model)
//        {
//            return await GetNeedsAggregateAsync(id, communityEntityIds, false, model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/needs/aggregate/communityEntities")]
//        [SwaggerOperation($"Lists all community entities for needs for the specified community and its ecosystem")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community's content")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
//        public async Task<IActionResult> GetNeedCommunityEntities([SwaggerParameter("ID of the community")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] bool currentUser, [FromQuery] SearchModel model)
//        {
//            var entity = GetEntity(id);
//            if (entity == null)
//                return NotFound();

//            if (!entity.Permissions.Contains(Permission.Read))
//                return Forbid();

//            var communityEntities = _needService.ListCommunityEntitiesForCommunityNeeds(id, CurrentUserId, types, currentUser, model.Search, model.Page, model.PageSize);
//            var communityEntityModels = communityEntities.Select(_mapper.Map<CommunityEntityMiniModel>);
//            return Ok(communityEntityModels);
//        }
//    }
//}