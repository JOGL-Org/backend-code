using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.URL;
using Jogl.Server.Data.Util;
using Jogl.Server.API.Converters;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("nodes")]
    public class NodeController : BaseCommunityEntityController<Data.Node, NodeModel, NodeDetailModel, CommunityEntityMiniModel, NodeUpsertModel, NodePatchModel>
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly INodeService _nodeService;
        private readonly IOrganizationService _organizationService;
        private readonly ICallForProposalService _callForProposalService;

        public NodeController(IWorkspaceService workspaceService, INodeService nodeService, IOrganizationService organizationService, INeedService needService, ICallForProposalService callForProposalService, IInvitationService invitationService, IAccessService accessService, IMembershipService membershipService, IUserService userService, IDocumentService documentService, ICommunityEntityService communityEntityService, ICommunityEntityInvitationService communityEntityInvitationService, ICommunityEntityMembershipService communityEntityMembershipService, IContentService contentService, IEventService eventService, IChannelService channelService, IPaperService paperService, IResourceService resourceService, IUrlService urlService, IConfiguration configuration, IMapper mapper, ILogger<NodeController> logger, IEntityService entityService, IContextService contextService) : base(accessService, invitationService, membershipService, userService, documentService, communityEntityService, communityEntityInvitationService, communityEntityMembershipService, contentService, eventService, channelService, paperService, resourceService, needService, urlService, configuration, mapper, logger, entityService, contextService)
        {
            _workspaceService = workspaceService;
            _nodeService = nodeService;
            _organizationService = organizationService;
            _callForProposalService = callForProposalService;
        }

        protected override CommunityEntityType EntityType => CommunityEntityType.Node;

        protected override async Task<string> CreateEntityAsync(Data.Node node)
        {
            return await _nodeService.CreateAsync(node);
        }

        protected override async Task DeleteEntity(string id)
        {
            await _nodeService.DeleteAsync(id);
        }

        protected override Data.Node GetEntity(string id)
        {
            return _nodeService.Get(id, CurrentUserId);
        }

        protected override Data.Node GetEntityDetail(string id)
        {
            return _nodeService.GetDetail(id, CurrentUserId);
        }

        protected override List<Data.Node> Autocomplete(string userId, string search, int page, int pageSize)
        {
            return _nodeService.Autocomplete(userId, search, page, pageSize);
        }
        protected override ListPage<Node> List(string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            return _nodeService.List(CurrentUserId, search, page, pageSize, sortKey, ascending);
        }

        protected override async Task UpdateEntityAsync(Data.Node node)
        {
            await _nodeService.UpdateAsync(node);
        }

        protected override List<CommunityEntity> ListEcosystem(string id, string search, int page, int pageSize)
        {
            return _workspaceService.ListForNode(CurrentUserId, id, search, page, pageSize).Cast<CommunityEntity>().ToList();
        }

        protected override List<Workspace> ListCommunities(string id, string search, int page, int pageSize)
        {
            return _workspaceService.ListForNode(CurrentUserId, id, search, page, pageSize);
        }

        protected override List<Data.Node> ListNodes(string id, string search, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        protected override List<Organization> ListOrganizations(string id, string search, int page, int pageSize)
        {
            return _organizationService.ListForNode(CurrentUserId, id, search, page, pageSize);
        }

        protected override List<Resource> ListResources(string id, string search, int page, int pageSize)
        {
            return _resourceService.ListForNode(CurrentUserId, id, search, page, pageSize);
        }


        [HttpPost]
        [SwaggerOperation($"Create a new node. The current user becomes a member of the node with the {nameof(AccessLevel.Owner)} role")]
        public async override Task<IActionResult> Create([FromBody] NodeUpsertModel model)
        {
            return await base.Create(model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation("Returns a node")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The node data", typeof(NodeModel))]
        public async override Task<IActionResult> Get([SwaggerParameter("ID of the node")] string id)
        {
            return await base.Get(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/detail")]
        [SwaggerOperation("Returns a node including detailed stats")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The node data including detailed stats", typeof(NodeDetailModel))]
        public async override Task<IActionResult> GetDetail([SwaggerParameter("ID of the node")] string id)
        {
            return await base.GetDetail(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("autocomplete")]
        [SwaggerOperation("List the basic information of nodes for a given search query. Only nodes accessible to the currently logged in user will be returned")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async override Task<IActionResult> Autocomplete([FromQuery] SearchModel model)
        {
            return await base.Autocomplete(model);
        }

        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation("List all accessible nodes for a given search query. Only nodes accessible to the currently logged in user will be returned")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ListPage<CommunityEntityMiniModel>))]
        public async override Task<IActionResult> Search([FromQuery] SearchModel model)
        {
            return await base.Search(model);
        }

        [HttpPatch]
        [Route("{id}")]
        [SwaggerOperation($"Patches the specified node")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this node")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
        public async override Task<IActionResult> Patch([SwaggerParameter("ID of the node")][FromRoute] string id, [FromBody] NodePatchModel model)
        {
            return await base.Patch(id, model);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the specified node")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this node")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
        public async override Task<IActionResult> Update([SwaggerParameter("ID of the node")][FromRoute] string id, [FromBody] NodeUpsertModel model)
        {
            return await base.Update(id, model);
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes the specified node and removes all of the node's associations from communities and nodes")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to delete this node")]
        public async override Task<IActionResult> Delete([SwaggerParameter("ID of the node")][FromRoute] string id)
        {
            return await base.Delete(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/workspaces")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this node's content", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "Workspace data", typeof(List<WorkspaceModel>))]
        public async Task<IActionResult> GetSpaces([SwaggerParameter("ID of the node")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            return await GetCommunitiesAsync(id, model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/organizations")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this node's content", typeof(string))]
        public async Task<IActionResult> GetOrganizations([SwaggerParameter("ID of the node")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            return await GetOrganizationsAsync(id, model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/cfps")]
        [SwaggerOperation($"Lists all cfps for the specified node")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this node's contents", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CallForProposalMiniModel>))]
        public async Task<IActionResult> GetCallsForProposals([SwaggerParameter("ID of the node")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var cfps = _callForProposalService.ListForNode(CurrentUserId, id, model.Search, model.Page, model.PageSize);
            var cfpModels = cfps.Select(_mapper.Map<CallForProposalMiniModel>);
            return Ok(cfpModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/needs/aggregate")]
        [SwaggerOperation($"Lists all needs for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of needs in the specified node matching the search query visible to the current user", typeof(ListPage<NeedModel>))]
        public async Task<IActionResult> ListNeedsAggregate([SwaggerParameter("ID of the node")] string id, [ModelBinder(typeof(ListBinder))][FromQuery] List<string>? communityEntityIds, [FromQuery] FeedEntityFilter? filter, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var needs = _needService.ListForNode(CurrentUserId, id, communityEntityIds, filter, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var needModels = needs.Items.Select(_mapper.Map<NeedModel>);
            return Ok(new ListPage<NeedModel>(needModels, needs.Total));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/needs/aggregate/communityEntities")]
        [SwaggerOperation($"Lists all community entities for needs for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this node's content")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetNeedCommunityEntities([SwaggerParameter("ID of the node")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] bool currentUser, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communityEntities = _needService.ListCommunityEntitiesForNodeNeeds(id, CurrentUserId, types, currentUser, model.Search, model.Page, model.PageSize);
            var communityEntityModels = communityEntities.Select(_mapper.Map<CommunityEntityMiniModel>);
            return Ok(communityEntityModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/events/aggregate")]
        [SwaggerOperation($"Lists all events for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Event data", typeof(ListPage<EventModel>))]
        public async Task<IActionResult> ListEventsAggregate([SwaggerParameter("ID of the node")] string id, [ModelBinder(typeof(ListBinder))][FromQuery] List<string>? communityEntityIds, [FromQuery] FeedEntityFilter? filter, [FromQuery] List<EventTag> tags, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();
            
            var events = _eventService.ListForNode(id, CurrentUserId, communityEntityIds, filter, tags, from, to, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var eventModels = events.Items.Select(_mapper.Map<EventModel>);
            return Ok(new ListPage<EventModel>(eventModels, events.Total));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/events/aggregate/communityEntities")]
        [SwaggerOperation($"Lists all community entities for events for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this node's content")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetEventCommunityEntities([SwaggerParameter("ID of the node")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] bool currentUser, [FromQuery] List<EventTag> tags, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communityEntities = _eventService.ListCommunityEntitiesForNodeEvents(id, CurrentUserId, types, currentUser, tags, from, to, model.Search, model.Page, model.PageSize);
            var communityEntityModels = communityEntities.Select(_mapper.Map<CommunityEntityMiniModel>);
            return Ok(communityEntityModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/documents/aggregate")]
        [SwaggerOperation($"Lists all documents for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ListPage<DocumentModel>))]
        public async Task<IActionResult> ListDocumentsAggregate([SwaggerParameter("ID of the node")] string id, [ModelBinder(typeof(ListBinder))][FromQuery] List<string>? communityEntityIds, [FromQuery] DocumentFilter? type, [FromQuery] FeedEntityFilter? filter, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var documents = _documentService.ListForNode(CurrentUserId, id, communityEntityIds, type, filter, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var documentModels = documents.Items.Select(_mapper.Map<DocumentModel>);
            return Ok(new ListPage<DocumentModel>(documentModels, documents.Total));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/documents/aggregate/communityEntities")]
        [SwaggerOperation($"Lists all community entities for documents for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this node's content")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetDocumentCommunityEntities([SwaggerParameter("ID of the node")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] DocumentFilter? type, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communityEntities = _documentService.ListFeedEntitiesForNodeDocuments(CurrentUserId, id, types, type, model.Search, model.Page, model.PageSize);
            var communityEntityModels = communityEntities.Where(ce => ce is CommunityEntity).Select(_mapper.Map<CommunityEntityMiniModel>);
            return Ok(communityEntityModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/papers/aggregate")]
        [SwaggerOperation($"Lists all papers for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ListPage<PaperModel>))]
        public async Task<IActionResult> ListPapersAggregate([SwaggerParameter("ID of the node")] string id, [ModelBinder(typeof(ListBinder))][FromQuery] List<string>? communityEntityIds, [FromQuery] PaperType? type, [FromQuery] FeedEntityFilter? filter, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var papers = _paperService.ListForNode(CurrentUserId, id, communityEntityIds, filter, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var paperModels = papers.Items.Select(_mapper.Map<PaperModel>);
            return Ok(new ListPage<PaperModel>(paperModels, papers.Total));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/papers/aggregate/communityEntities")]
        [SwaggerOperation($"Lists all community entities for papers for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this node's content")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetPaperCommunityEntities([SwaggerParameter("ID of the node")] string id, [FromQuery] List<CommunityEntityType> types, [FromQuery] PaperType? type, [FromQuery] List<PaperTag> tags, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communityEntities = _paperService.ListCommunityEntitiesForNodePapers(CurrentUserId, id, model.Search, model.Page, model.PageSize);
            var communityEntityModels = communityEntities.Select(_mapper.Map<CommunityEntityMiniModel>);
            return Ok(communityEntityModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/users/aggregate")]
        [SwaggerOperation($"Lists all needs for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of needs in the specified node matching the search query visible to the current user", typeof(ListPage<UserMiniModel>))]
        public async Task<IActionResult> GetUsersAggregate([SwaggerParameter("ID of the node")] string id, [ModelBinder(typeof(ListBinder))][FromQuery] List<string>? communityEntityIds, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var users = _userService.ListForNode(CurrentUserId, id, communityEntityIds, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var userModels = users.Items.Select(_mapper.Map<UserMiniModel>);
            return Ok(new ListPage<UserMiniModel>(userModels, users.Total));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/users/aggregate/communityEntities")]
        [SwaggerOperation($"Lists all community entities for users for the specified node and its ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No node was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this node's content")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetUserCommunityEntities([SwaggerParameter("ID of the node")] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communityEntities = _userService.ListCommunityEntitiesForNodeUsers(id, model.Search, model.Page, model.PageSize);
            var communityEntityModels = communityEntities.Select(_mapper.Map<CommunityEntityMiniModel>);
            return Ok(communityEntityModels);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/search/totals")]
        [SwaggerOperation($"Lists the totals for a global search query")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Search result totals", typeof(SearchResultNodeModel))]
        public async Task<IActionResult> GetSearchResults([FromRoute] string id, [FromQuery] BaseSearchModel model)
        {
            var userTask = Task.Run(() => _userService.CountForNode(CurrentUserId, id, model.Search));
            var eventTask = Task.Run(() => _eventService.CountForNode(CurrentUserId, id, model.Search));
            var needTask = Task.Run(() => _needService.CountForNode(CurrentUserId, id, new List<string>(), model.Search));
            var workspaceTask = Task.Run(() => _workspaceService.CountForNode(CurrentUserId, id, model.Search));
            var docTask = Task.Run(() => _documentService.CountForNode(CurrentUserId, id, model.Search));
            var paperTask = Task.Run(() => _paperService.CountForNode(CurrentUserId, id, model.Search));

            await Task.WhenAll(userTask, eventTask, needTask, workspaceTask, docTask, paperTask);

            return Ok(new SearchResultNodeModel
            {
                UserCount = await userTask,
                EventCount = await eventTask,
                NeedCount = await needTask,
                WorkspaceCount = await workspaceTask,
                DocCount = await docTask,
                PaperCount = await paperTask,
            });
        }
    }
}