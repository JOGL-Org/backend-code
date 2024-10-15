using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("papers")]
    public class PaperController : BaseController
    {
        private readonly IContentService _contentService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IPaperService _paperService;
        private readonly IConfiguration _configuration;

        public PaperController(IContentService contentService, ICommunityEntityService communityEntityService, IPaperService paperService, IConfiguration configuration, IMapper mapper, ILogger<PaperController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _contentService = contentService;
            _communityEntityService = communityEntityService;
            _paperService = paperService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("{entityId}/papers")]
        [SwaggerOperation($"Adds a new paper and/or associates it to the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The community entity could not be found")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The ID of the new paper", typeof(string))]
        public async Task<IActionResult> AddPaper([SwaggerParameter("ID of the community entity")][FromRoute] string entityId, [FromBody] PaperUpsertModel model)
        {
            var entity = _communityEntityService.GetEnriched(entityId, CurrentUserId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Data.Enum.Permission.ManageLibrary))
                return Forbid();

            var paper = _mapper.Map<Paper>(model);
            await InitCreationAsync(paper);
            var paperId = await _paperService.CreateAsync(entityId, paper);

            return Ok(paperId);
        }

        [HttpPost]
        [Route("{id}/associate/{entityId}")]
        [SwaggerOperation($"Associates a paper to the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity or no paper was found for the specified id")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "The paper is already added to the community entity's library")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The currently logged user does not have the permissions to manage the community entity's library")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The paper has been associated with the entity")]
        public async Task<IActionResult> AssociatePaper([FromRoute] string id, [FromRoute] string entityId)
        {
            var entity = _communityEntityService.GetEnriched(entityId, CurrentUserId);
            if (entity == null)
                return NotFound();

            var paper = _paperService.Get(id, CurrentUserId);
            if (paper == null)
                return NotFound();

            if (!entity.Permissions.Contains(Data.Enum.Permission.ManageLibrary))
                return Forbid();

            if (paper.FeedIds.Any(eid => string.Equals(eid, entityId, StringComparison.InvariantCultureIgnoreCase)))
                return Conflict();

            await _paperService.AssociateAsync(entityId, id);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{entityId}/papers")]
        [SwaggerOperation($"Lists all papers for the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<PaperModel>))]
        public async Task<IActionResult> GetPapers([SwaggerParameter("ID of the community entity")][FromRoute] string entityId, [SwaggerParameter("The paper type")][FromQuery] PaperType? type, [FromQuery] List<PaperTag> tags, [FromQuery] SearchModel model)
        {
            var entity = _communityEntityService.GetEnriched(entityId, CurrentUserId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var papers = _paperService.ListForEntity(CurrentUserId, entityId, type, tags, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var paperModels = papers.Select(_mapper.Map<PaperModel>);
            return Ok(paperModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation($"Returns a single paper")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper was found for the paper id")]
        public async Task<IActionResult> GetPaper([FromRoute] string id)
        {
            var paper = _paperService.Get(id, CurrentUserId);
            if (paper == null)
                return NotFound();

            var paperModel = _mapper.Map<PaperModel>(paper);
            return Ok(paperModel);
        }

        [HttpDelete]
        [Route("{id}/papers/{entityId}")]
        [SwaggerOperation($"Disassociates the specified paper from the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper was found for the paper id or the paper isn't associated to the given community entity")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to manage papers for the community entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The paper was disassociated from the entity")]
        public async Task<IActionResult> DisassociatePaper([FromRoute] string id, [FromRoute] string entityId)
        {
            var entity = _communityEntityService.GetEnriched(entityId, CurrentUserId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.ManageLibrary))
                return Forbid();

            await _paperService.DisassociateAsync(entityId, id);
            return Ok();
        }
    }
}