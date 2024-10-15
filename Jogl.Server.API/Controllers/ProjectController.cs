//using AutoMapper;
//using Jogl.Server.API.Model;
//using Jogl.Server.API.Services;
//using Jogl.Server.Business;
//using Jogl.Server.Data;
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
//    [Route("projects")]
//    public class ProjectController : CommunityEntityController<Project, ProjectModel, ProjectDetailModel, CommunityEntityMiniModel, ProjectUpsertModel, ProjectPatchModel>
//    {
//        private readonly IProjectService _projectService;
//        private readonly IWorkspaceService _workspaceService;
//        private readonly INodeService _nodeService;
//        private readonly IOrganizationService _organizationService;
//        private readonly ITagService _tagService;
//        private readonly IProposalService _proposalService;

//        public ProjectController(IProjectService projectService, IWorkspaceService workspaceService, INodeService nodeService, IOrganizationService organizationService, INeedService needService, ITagService tagService, IInvitationService invitationService, IAccessService accessService, IMembershipService membershipService, IUserService userService, IDocumentService documentService, ICommunityEntityService communityEntityService, ICommunityEntityInvitationService communityEntityInvitationService, ICommunityEntityMembershipService communityEntityMembershipService, IContentService contentService, IEventService eventService, IChannelService channelService, IPaperService paperService, IResourceService resourceService, IProposalService proposalService, IUrlService urlService, IConfiguration configuration, IMapper mapper, ILogger<ProjectController> logger, IEntityService entityService, IContextService contextService) : base(accessService, invitationService, membershipService, userService, documentService, communityEntityService, communityEntityInvitationService, communityEntityMembershipService, contentService, eventService, channelService, paperService, resourceService, needService, urlService, configuration, mapper, logger, entityService, contextService)
//        {
//            _projectService = projectService;
//            _workspaceService = workspaceService;
//            _nodeService = nodeService;
//            _organizationService = organizationService;
//            _tagService = tagService;
//            _proposalService = proposalService;
//        }

//        protected override CommunityEntityType EntityType => CommunityEntityType.Project;

//        protected override Project GetEntity(string id)
//        {
//            return _projectService.Get(id, CurrentUserId);
//        }

//        protected override Project GetEntityDetail(string id)
//        {
//            return _projectService.GetDetail(id, CurrentUserId);
//        }

//        protected async override Task<string> CreateEntityAsync(Project p)
//        {
//            return await _projectService.CreateAsync(p);
//        }

//        protected override List<Project> Autocomplete(string userId, string search, int page, int pageSize)
//        {
//            return _projectService.Autocomplete(userId, search, page, pageSize);
//        }

//        protected override ListPage<Project> List(string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            return _projectService.List(CurrentUserId, search, page, pageSize, sortKey, ascending);
//        }

//        protected async override Task UpdateEntityAsync(Project p)
//        {
//            await _projectService.UpdateAsync(p);
//        }

//        protected async override Task DeleteEntity(string id)
//        {
//            await _projectService.DeleteAsync(id);
//        }

//        protected override List<CommunityEntity> ListEcosystem(string id, string search, int page, int pageSize)
//        {
//            return _nodeService.ListForProject(CurrentUserId, id, search, page, pageSize).Cast<CommunityEntity>().ToList();
//        }

//        protected override List<Project> ListProjects(string id, string search, int page, int pageSize)
//        {
//            throw new NotImplementedException();
//        }

//        protected override List<Workspace> ListCommunities(string id, string search, int page, int pageSize)
//        {
//            return new List<Workspace>();
//        }

//        protected override List<Data.Node> ListNodes(string id, string search, int page, int pageSize)
//        {
//            return _nodeService.ListForProject(CurrentUserId, id, search, page, pageSize);
//        }

//        protected override List<Organization> ListOrganizations(string id, string search, int page, int pageSize)
//        {
//            return _organizationService.ListForProject(CurrentUserId, id, search, page, pageSize);
//        }

//        protected override List<Resource> ListResources(string id, string search, int page, int pageSize)
//        {
//            return _resourceService.ListForFeed(id, search, page, pageSize);
//        }

//        protected override ListPage<Paper> ListPapersAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, PaperType? type, List<PaperTag> tags, string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            throw new NotImplementedException();
//        }

//        protected override ListPage<Document> ListDocumentsAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, DocumentFilter? type, string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            throw new NotImplementedException();
//        }

//        protected override ListPage<Need> ListNeedsAggregate(string id, List<string> communityEntityIds, bool currentUser, string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            throw new NotImplementedException();
//        }

//        protected override ListPage<Event> ListEventsAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending)
//        {
//            throw new NotImplementedException();
//        }

//        [HttpPost]
//        [SwaggerOperation($"Create a new project. The current user becomes a member of the project with the {nameof(AccessLevel.Owner)} role")]
//        public async override Task<IActionResult> Create([FromBody] ProjectUpsertModel model)
//        {
//            return await base.Create(model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}")]
//        [SwaggerOperation("Returns a project")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "The project data", typeof(ProjectModel))]
//        public async override Task<IActionResult> Get([SwaggerParameter("ID of the project")] string id)
//        {
//            return await base.Get(id);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/detail")]
//        [SwaggerOperation("Returns a project including detailed stats")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "The project data including detailed stats", typeof(ProjectDetailModel))]
//        public async override Task<IActionResult> GetDetail([SwaggerParameter("ID of the project")] string id)
//        {
//            return await base.GetDetail(id);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("autocomplete")]
//        [SwaggerOperation("List the basic information of projects for a given search query. Only projects accessible to the currently logged in user will be returned")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
//        public async override Task<IActionResult> Autocomplete([FromQuery] SearchModel model)
//        {
//            return await base.Autocomplete(model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [SwaggerOperation("List all accessible projects for a given search query. Only projects accessible to the currently logged in user will be returned")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ListPage<CommunityEntityMiniModel>))]
//        public async override Task<IActionResult> Search([FromQuery] SearchModel model)
//        {
//            return await base.Search(model);
//        }

//        [HttpPatch]
//        [Route("{id}")]
//        [SwaggerOperation($"Patches the specified project")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this project")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
//        public async override Task<IActionResult> Patch([SwaggerParameter("ID of the project")][FromRoute] string id, [FromBody] ProjectPatchModel model)
//        {
//            return await base.Patch(id, model);
//        }

//        [HttpPut]
//        [Route("{id}")]
//        [SwaggerOperation($"Updates the specified project")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this project")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
//        public async override Task<IActionResult> Update([SwaggerParameter("ID of the project")][FromRoute] string id, [FromBody] ProjectUpsertModel model)
//        {
//            return await base.Update(id, model);
//        }

//        [HttpDelete]
//        [Route("{id}")]
//        [SwaggerOperation($"Deletes the specified project and removes all of the project's associations from communities and nodes")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to delete this project")]
//        public async override Task<IActionResult> Delete([SwaggerParameter("ID of the project")][FromRoute] string id)
//        {
//            return await base.Delete(id);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("for/paper")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
//        public async Task<IActionResult> ListForExternalId([FromQuery][SwaggerParameter("External ID")] string externalID)
//        {
//            var projects = _projectService.ListForPaperExternalId(CurrentUserId, WebUtility.UrlDecode(externalID));
//            var projectModels = projects.Select(_mapper.Map<CommunityEntityMiniModel>);
//            return Ok(projectModels);
//        }

//        [HttpPost]
//        [Route("keywords")]
//        [SwaggerOperation($"Creates a new keyword")]
//        [SwaggerResponse((int)HttpStatusCode.Conflict, "The keyword already exists")]
//        public async Task<IActionResult> CreateTag([FromBody] TextValueModel model)
//        {
//            var interest = _tagService.GetTag(model.Value);
//            if (interest != null)
//                return Conflict();

//            interest = new Tag { Text = model.Value };
//            await InitCreationAsync(interest);
//            await _tagService.CreateTagAsync(interest);
//            return Ok();
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("keywords")]
//        [SwaggerOperation($"Returns keywords")]
//        public async Task<IActionResult> GetTags([FromQuery] SearchModel model)
//        {
//            var skills = _tagService.GetTags(model.Search, model.Page, model.PageSize);
//            var skillModels = skills.Select(_mapper.Map<TextValueModel>);
//            return Ok(skillModels);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/communities")]
//        [Route("{id}/workspaces")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this project's content", typeof(string))]
//        [SwaggerResponse((int)HttpStatusCode.OK, "Workspace data", typeof(List<WorkspaceModel>))]
//        public async Task<IActionResult> GetSpaces([SwaggerParameter("ID of the project")] string id, [FromQuery] SearchModel model)
//        {
//            return await GetCommunitiesAsync(id, model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/nodes")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this project's content", typeof(string))]
//        public async Task<IActionResult> GetNodes([SwaggerParameter("ID of the project")][FromRoute] string id, [FromQuery] SearchModel model)
//        {
//            return await GetNodesAsync(id, model);
//        }

//        [AllowAnonymous]
//        [HttpGet]
//        [Route("{id}/organizations")]
//        [SwaggerResponse((int)HttpStatusCode.NotFound, "No project was found for that id")]
//        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this project's content", typeof(string))]
//        public async Task<IActionResult> GetOrganizations([SwaggerParameter("ID of the project")][FromRoute] string id, [FromQuery] SearchModel model)
//        {
//            return await GetOrganizationsAsync(id, model);
//        }

//        [HttpGet]
//        [Route("{id}/proposals")]
//        [SwaggerOperation($"Lists all proposals submitted by the specified project")]
//        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<ProposalModel>))]
//        public async Task<IActionResult> GetProposals([SwaggerParameter("ID of the project")][FromRoute] string id)
//        {
//            var project = _projectService.Get(id, CurrentUserId);
//            if (project == null)
//                return NotFound();

//            if (!project.Permissions.Contains(Data.Enum.Permission.ListProposals))
//                return Forbid();

//            var proposals = _proposalService.ListForProject(id);
//            var proposalModels = proposals.Select(_mapper.Map<ProposalModel>);
//            return Ok(proposalModels);
//        }
//    }
//}