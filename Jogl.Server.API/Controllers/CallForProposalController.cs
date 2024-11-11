using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.API.Services;
using Jogl.Server.Business;
using Jogl.Server.Data;
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
    [Route("CFPs")]
    public class CallForProposalController : BaseCommunityEntityController<CallForProposal, CallForProposalModel, CallForProposalDetailModel, CallForProposalMiniModel, CallForProposalUpsertModel, CallForProposalPatchModel>
    {
        private readonly ICallForProposalService _callForProposalService;
        private readonly IWorkspaceService _workspaceService;
        private readonly IProposalService _proposalService;

        public CallForProposalController(ICallForProposalService callForProposalService, IWorkspaceService workspaceService, INeedService needService, IInvitationService invitationService, IAccessService accessService, IMembershipService membershipService, IUserService userService, IDocumentService documentService, ICommunityEntityService communityEntityService, ICommunityEntityInvitationService communityEntityInvitationService, ICommunityEntityMembershipService communityEntityMembershipService, IContentService contentService, IEventService eventService, IChannelService channelService, IPaperService paperService, IResourceService resourceService, IProposalService proposalService, IUrlService urlService, IConfiguration configuration, IMapper mapper, ILogger<CallForProposalController> logger, IEntityService entityService, IContextService contextService) : base(accessService, invitationService, membershipService, userService, documentService, communityEntityService, communityEntityInvitationService, communityEntityMembershipService, contentService, eventService, channelService, paperService, resourceService, needService, urlService, configuration, mapper, logger, entityService, contextService)
        {
            _callForProposalService = callForProposalService;
            _workspaceService = workspaceService;
            _proposalService = proposalService;
        }

        protected override CommunityEntityType EntityType => CommunityEntityType.CallForProposal;

        protected override CallForProposal GetEntity(string id)
        {
            return _callForProposalService.Get(id, CurrentUserId);
        }

        protected override CallForProposal GetEntityDetail(string id)
        {
            return _callForProposalService.GetDetail(id, CurrentUserId);
        }

        protected async override Task<string> CreateEntityAsync(CallForProposal p)
        {
            return await _callForProposalService.CreateAsync(p);
        }

        protected override List<CallForProposal> Autocomplete(string userId, string search, int page, int pageSize)
        {
            return _callForProposalService.Autocomplete(userId, search, page, pageSize);
        }

        protected override ListPage<CallForProposal> List(string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            return _callForProposalService.List(CurrentUserId, search, page, pageSize, sortKey, ascending);
        }

        protected async override Task UpdateEntityAsync(CallForProposal p)
        {
            await _callForProposalService.UpdateAsync(p);
        }

        protected async override Task DeleteEntity(string id)
        {
            await _callForProposalService.DeleteAsync(id);
        }

        protected override List<CommunityEntity> ListEcosystem(string id, string search, int page, int pageSize)
        {
            return new List<CommunityEntity>();
        }

        protected override List<Workspace> ListCommunities(string id, string search, int page, int pageSize)
        {
            throw new Exception();
        }

        protected override List<Data.Node> ListNodes(string id, string search, int page, int pageSize)
        {
            throw new Exception();
        }

        protected override List<Organization> ListOrganizations(string id, string search, int page, int pageSize)
        {
            throw new Exception();
        }

        protected override List<Resource> ListResources(string id, string search, int page, int pageSize)
        {
            return _resourceService.ListForFeed(id, search, page, pageSize);
        }

        protected override ListPage<Paper> ListPapersAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, PaperType? type, List<PaperTag> tags, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            throw new NotImplementedException();
        }

        protected override ListPage<Document> ListDocumentsAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, DocumentFilter? type, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            throw new NotImplementedException();
        }

        protected override ListPage<Need> ListNeedsAggregate(string id, List<string> communityEntityIds, bool currentUser, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            throw new NotImplementedException();
        }

        protected override ListPage<Event> ListEventsAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [SwaggerOperation($"Create a new call for proposals. The current user becomes a member of the call for proposals with the {nameof(AccessLevel.Owner)} role")]
        public async override Task<IActionResult> Create([FromBody] CallForProposalUpsertModel model)
        {
            var community = _workspaceService.Get(model.ParentCommunityEntityId, CurrentUserId);
            if (community == null)
                return NotFound();

            if (!community.Permissions.Contains(Data.Enum.Permission.Manage))
                return Forbid();

            return await base.Create(model);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation("Returns a single call for proposals")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No call for proposals was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The call for proposals data", typeof(CallForProposalModel))]
        public async override Task<IActionResult> Get([SwaggerParameter("ID of the call for proposal")] string id)
        {
            return await base.Get(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/detail")]
        [SwaggerOperation("Returns a single call for proposals, including detailed stats")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No call for proposals was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The call for proposals data including detailed stats", typeof(CallForProposalDetailModel))]
        public async override Task<IActionResult> GetDetail([SwaggerParameter("ID of the call for proposal")] string id)
        {
            return await base.GetDetail(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("autocomplete")]
        [SwaggerOperation("List the basic information of calls for proposals for a given search query. Only calls for proposals accessible to the currently logged in user will be returned")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<CommunityEntityMiniModel>))]
        public async override Task<IActionResult> Autocomplete([FromQuery] SearchModel model)
        {
            return await base.Autocomplete(model);
        }

        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation("List all accessible calls for proposals for a given search query. Only calls for proposals accessible to the currently logged in user will be returned")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ListPage<CommunityEntityMiniModel>))]
        public async override Task<IActionResult> Search([FromQuery] SearchModel model)
        {
            return await base.Search(model);
        }

        [HttpPatch]
        [Route("{id}")]
        [SwaggerOperation($"Patches the specified call for proposal")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No call for proposal was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this call for proposal")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
        public async override Task<IActionResult> Patch([SwaggerParameter("ID of the call for proposals")][FromRoute] string id, [FromBody] CallForProposalPatchModel model)
        {
            return await base.Patch(id, model);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the specified call for proposal")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No call for proposal was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to edit this call for proposal")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The ID of the entity", typeof(string))]
        public async override Task<IActionResult> Update([SwaggerParameter("ID of the call for proposals")][FromRoute] string id, [FromBody] CallForProposalUpsertModel model)
        {
            return await base.Update(id, model);
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes the specified call for proposal and removes all of the call for proposal's associations from projects and communities")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No call for proposal was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to delete this call for proposal")]
        public async override Task<IActionResult> Delete([SwaggerParameter("ID of the call for proposals")][FromRoute] string id)
        {
            return await base.Delete(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/proposals")]
        [SwaggerOperation($"Lists all proposals submitted to the specified call for proposals")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No call for proposal was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to view proposals")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<ProposalModel>))]
        public async Task<IActionResult> GetProposals([SwaggerParameter("ID of the call for proposals")][FromRoute] string id)
        {
            var cfp = _callForProposalService.Get(id, CurrentUserId);
            if (cfp == null)
                return NotFound();

            var proposals = new List<Proposal>();
            if (cfp.Permissions.Contains(Data.Enum.Permission.ListProposals))
                proposals = _proposalService.ListForCFPAdmin(CurrentUserId, id);
            else
                proposals = _proposalService.ListForCFP(CurrentUserId, id);

            var proposalModels = proposals.Select(_mapper.Map<ProposalModel>);
            return Ok(proposalModels);
        }

        [HttpPost]
        [Route("{id}/message")]
        [SwaggerOperation($"Sends a message to selected cfp members")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No call for proposal was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to send messages to cfp members")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<ProposalModel>))]
        public async Task<IActionResult> Message([SwaggerParameter("ID of the call for proposals")][FromRoute] string id, [FromBody] MessageModel model)
        {
            var cfp = _callForProposalService.Get(id, CurrentUserId);
            if (cfp == null)
                return NotFound();

            if (!cfp.Permissions.Contains(Data.Enum.Permission.Manage))
                return Forbid();

            var urlFragment = _urlService.GetUrlFragment(EntityType);
            var redirectUrl = $"{_configuration["App:URL"]}/{urlFragment}/{id}";
            await _callForProposalService.SendMessageToUsersAsync(id, model.UserIds, model.Subject, model.Message, redirectUrl);

            return Ok();
        }

        [HttpPost]
        [Route("{id}/changed")]
        [SwaggerOperation($"Determines whether the template for a CFP has changed or not")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No call for proposal was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<ProposalModel>))]
        public async Task<IActionResult> HasTemplateChanged([SwaggerParameter("ID of the call for proposals")][FromRoute] string id, [FromBody] CallForProposalUpsertModel model)
        {
            var cfp = _callForProposalService.Get(id, CurrentUserId);
            if (cfp == null)
                return NotFound();

            var updatedCFP = _mapper.Map<CallForProposal>(model);
            var changed = _callForProposalService.HasTemplateChanged(cfp, updatedCFP);

            return Ok(changed);
        }
    }
}