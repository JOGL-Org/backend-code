using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.API.Services;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("proposals")]
    public class ProposalController : BaseController
    {
        private readonly IProposalService _proposalService;
        private readonly ICallForProposalService _callForProposalService;
        private readonly IWorkspaceService _workspaceService;
        private readonly IDocumentService _documentService;
        private readonly IConfiguration _configuration;

        public ProposalController(IProposalService proposalService, ICallForProposalService callForProposalService, IWorkspaceService workspaceService, IDocumentService documentService, IConfiguration configuration, IMapper mapper, ILogger<ProposalController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _proposalService = proposalService;
            _callForProposalService = callForProposalService;
            _workspaceService = workspaceService;
            _documentService = documentService;
            _configuration = configuration;
        }

        [HttpPost]
        [SwaggerOperation($"Creates a new proposal on behalf of a project")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, $"A proposal already exists for this project and call for proposals", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, $"Either the call for proposal or the project was not found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The call for proposal is not yet open for submissions or the current user does not have permissions")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The ID of the new proposal", typeof(string))]
        public async Task<IActionResult> CreateProposal([FromBody] ProposalUpsertModel model)
        {
            var existingProposal = _proposalService.GetForProjectAndCFP(model.SourceCommunityEntityId, model.CallForProposalId);
            if (existingProposal != null)
                return Conflict();

            var cfp = _callForProposalService.Get(model.CallForProposalId, CurrentUserId);
            if (cfp == null)
                return NotFound();

            var workspace = _workspaceService.Get(model.SourceCommunityEntityId, CurrentUserId);
            if (workspace == null)
                return NotFound();

            if (!cfp.SubmissionsFrom.HasValue || cfp.SubmissionsFrom > DateTime.UtcNow)
                return Forbid();

            if (!workspace.Permissions.Contains(Data.Enum.Permission.CreateProposals))
                return Forbid();

            if (!cfp.Permissions.Contains(Data.Enum.Permission.CreateProposals))
                return Forbid();

            var proposal = _mapper.Map<Proposal>(model);
            await InitCreationAsync(proposal);
            var proposalId = await _proposalService.CreateAsync(proposal);

            return Ok(proposalId);
        }

        [HttpGet]
        [Route("{proposalId}")]
        [SwaggerOperation($"Gets a proposal")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No proposal was found for the id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user is not an author on the proposal")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ProposalModel))]
        public async Task<IActionResult> GetProposal([SwaggerParameter("ID of the proposal")][FromRoute] string proposalId)
        {
            var proposal = _proposalService.Get(CurrentUserId, proposalId);
            if (proposal == null)
                return NotFound();

            var cfp = _callForProposalService.Get(proposal.CallForProposalId, CurrentUserId);
            if (!proposal.UserIds.Contains(CurrentUserId) && (cfp == null || !cfp.Permissions.Contains(Data.Enum.Permission.ListProposals)))
                return Forbid();

            var proposalModel = _mapper.Map<ProposalModel>(proposal);
            return Ok(proposalModel);
        }

        [HttpPatch]
        [Route("{proposalId}")]
        [SwaggerOperation($"Patches a proposal")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No proposal was found for the id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user is not an author on the proposal")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ProposalModel))]
        public async Task<IActionResult> PatchProposal([FromRoute] string proposalId, ProposalPatchModel model)
        {
            var proposal = _proposalService.Get(CurrentUserId, proposalId);
            if (proposal == null)
                return NotFound();

            if (!proposal.UserIds.Contains(CurrentUserId))
                return Forbid();

            var upsertModel = _mapper.Map<ProposalUpsertModel>(proposal);
            ApplyPatchModel(model, upsertModel);

            var updatedEntity = _mapper.Map<Proposal>(upsertModel);
            await InitUpdateAsync(updatedEntity);
            await _proposalService.UpdateAsync(updatedEntity);

            return Ok();
        }

        [HttpPost]
        [Route("{proposalId}/status")]
        [SwaggerOperation($"Updates the status of a proposal")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, $"The proposal has already been submitted")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No proposal was found for the id")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "This status cannot be set")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to set this status")]
        public async Task<IActionResult> UpdateProposalStatus([FromRoute] string proposalId, [FromBody] ProposalStatus status)
        {
            var proposal = _proposalService.Get(CurrentUserId, proposalId);
            if (proposal == null)
                return NotFound();

            var cfp = _callForProposalService.Get(proposal.CallForProposalId, CurrentUserId);
            if (cfp == null)
                return NotFound();

            await InitUpdateAsync(proposal);
            switch (status)
            {
                case ProposalStatus.Draft:
                    return BadRequest();
                case ProposalStatus.Submitted:
                    if (proposal.Status != ProposalStatus.Draft)
                        return Conflict();

                    if (!proposal.UserIds.Contains(CurrentUserId))
                        return Forbid();

                    await _proposalService.JoinMembersToCFPAsync(proposal);
                    proposal.Submitted = DateTime.UtcNow;
                    break;
                case ProposalStatus.Rejected:
                case ProposalStatus.Accepted:
                    if (proposal.Status == ProposalStatus.Draft)
                        return Conflict();

                    if (!cfp.Permissions.Contains(Data.Enum.Permission.ScoreProposals))
                        return Forbid();

                    break;
            }

            proposal.Status = status;
            await _proposalService.UpdateAsync(proposal);
            return Ok(proposalId);
        }

        [HttpPost]
        [Route("{proposalId}/score")]
        [SwaggerOperation($"Scores a proposal")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No proposal was found for the id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user is not a reviewer on the cfp")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ProposalModel))]
        public async Task<IActionResult> ScoreProposal([FromRoute] string proposalId, [FromBody] decimal score)
        {
            var proposal = _proposalService.Get(CurrentUserId, proposalId);
            if (proposal == null)
                return NotFound();

            var cfp = _callForProposalService.Get(proposal.CallForProposalId, CurrentUserId);
            if (cfp == null)
                return NotFound();

            if (!cfp.Permissions.Contains(Data.Enum.Permission.ScoreProposals))
                return Forbid();

            proposal.Score = score;
            await InitUpdateAsync(proposal);
            await _proposalService.UpdateAsync(proposal);

            return Ok();
        }

        [HttpDelete]
        [Route("{proposalId}")]
        [SwaggerOperation($"Deletes the specified proposal")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No proposal was found for the id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The proposal wasn't created by the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The proposal was deleted")]
        public async Task<IActionResult> DeleteProposal([FromRoute] string proposalId)
        {
            var proposal = _proposalService.Get(CurrentUserId, proposalId);
            if (proposal == null)
                return NotFound();

            if (proposal.CreatedByUserId != CurrentUserId)
                return Forbid();

            await _proposalService.DeleteAsync(proposalId);
            return Ok();
        }


        [HttpPost]
        [Route("{proposalId}/documents")]
        [SwaggerOperation($"Adds a new document for the specified proposal.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No proposal was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add documents for the proposal")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was created", typeof(string))]
        public async Task<IActionResult> AddDocument([FromRoute] string proposalId, [FromBody] DocumentInsertModel model)
        {
            var proposal = _proposalService.Get(CurrentUserId, proposalId);
            if (proposal == null)
                return NotFound();

            var cfp = _callForProposalService.Get(proposal.CallForProposalId, CurrentUserId);
            if (!proposal.UserIds.Contains(CurrentUserId) && (cfp == null || !cfp.Permissions.Contains(Data.Enum.Permission.ListProposals)))
                return Forbid();

            var document = _mapper.Map<Document>(model);
            document.ProposalId = proposalId;
            await InitCreationAsync(document);
            var documentId = await _documentService.CreateAsync(document);
            return Ok(documentId);
        }

        [Obsolete]
        [HttpGet]
        [Route("{proposalId}/documents/{documentId}")]
        [SwaggerOperation($"Returns a single document, including the file represented as base64")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No proposal was found for that id, the document id is incorrect or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the proposal")]
        public async Task<IActionResult> GetDocument([FromRoute] string proposalId, [FromRoute] string documentId)
        {
            var proposal = _proposalService.Get(CurrentUserId, proposalId);
            if (proposal == null)
                return NotFound();

            var cfp = _callForProposalService.Get(proposal.CallForProposalId, CurrentUserId);
            if (!proposal.UserIds.Contains(CurrentUserId) && (cfp == null || !cfp.Permissions.Contains(Data.Enum.Permission.ListProposals)))
                return Forbid();

            var document = await _documentService.GetAsync(documentId, CurrentUserId);
            if (document == null)
                return NotFound();

            if (document.ProposalId != proposalId)
                return NotFound();

            var documentModel = _mapper.Map<DocumentModel>(document);
            return Ok(documentModel);
        }
    }
}