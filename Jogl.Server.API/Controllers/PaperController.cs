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
using Syncfusion.DocIO.DLS;
using MongoDB.Bson;

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
        [SwaggerOperation($"Adds a new paper and/or associates it to the specified entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The entity could not be found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add papers for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The ID of the new paper", typeof(string))]
        public async Task<IActionResult> AddPaper([SwaggerParameter("ID of the entity")][FromRoute] string entityId, [FromBody] PaperUpsertModel model)
        {
            var entity = _communityEntityService.GetEnriched(entityId, CurrentUserId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Data.Enum.Permission.ManageLibrary))
                return Forbid();

            var paper = _mapper.Map<Paper>(model);
            paper.FeedId = entityId;

            await InitCreationAsync(paper);
            var paperId = await _paperService.CreateAsync(paper);

            return Ok(paperId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{entityId}/papers")]
        [SwaggerOperation($"Lists all papers for the specified entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to list papers for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The paper data", typeof(List<PaperModel>))]
        public async Task<IActionResult> GetPapers([SwaggerParameter("ID of the entity")][FromRoute] string entityId, [FromQuery] SearchModel model)
        {
            var entity = _communityEntityService.GetEnriched(entityId, CurrentUserId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var papers = _paperService.ListForEntity(CurrentUserId, entityId, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var paperModels = papers.Select(_mapper.Map<PaperModel>);
            return Ok(paperModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation($"Returns a single paper")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view the paper")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The paper data", typeof(PaperModel))]
        public async Task<IActionResult> GetPaper([FromRoute] string id)
        {
            var paper = _paperService.Get(id, CurrentUserId);
            if (paper == null)
                return NotFound();

            if (!paper.Permissions.Contains(Permission.Read))
                return Forbid();

            var paperModel = _mapper.Map<PaperModel>(paper);
            return Ok(paperModel);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the paper")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit the paper")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The paper was updated")]
        public async Task<IActionResult> UpdateDocument([FromRoute] string id, [FromBody] PaperUpsertModel model)
        {
            var existingPaper = _paperService.Get(id,CurrentUserId);
            if (existingPaper == null)
                return NotFound();

            if (!existingPaper.Permissions.Contains(Permission.Manage))
                return Forbid();

            var document = _mapper.Map<Paper>(model);
            document.Id = ObjectId.Parse(id);
            document.FeedId = existingPaper.FeedId;
            await InitUpdateAsync(document);
            await _paperService.UpdateAsync(document);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes the specified paper")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete the paper")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The paper was deleted")]
        public async Task<IActionResult> DeletePaper([FromRoute] string id)
        {
            var paper = _paperService.Get(id, CurrentUserId);
            if (!paper.Permissions.Contains(Permission.Manage))
                return Forbid();

            await _paperService.DeleteAsync(paper);
            return Ok();
        }

        [Obsolete]
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

        [Obsolete]
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