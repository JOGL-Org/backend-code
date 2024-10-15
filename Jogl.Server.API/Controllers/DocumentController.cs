using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.Data.Enum;
using Jogl.Server.Images;
using Jogl.Server.Documents;
using Jogl.Server.Data.Util;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("documents")]
    public class DocumentController : BaseController
    {
        private readonly IDocumentService _documentService;
        private readonly IFeedEntityService _feedService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IDocumentConverter _documentConverter;
        private readonly IConfiguration _configuration;

        public DocumentController(IDocumentService documentService, IFeedEntityService feedService, ICommunityEntityService communityEntityService, IDocumentConverter documentConverter, IConfiguration configuration, IMapper mapper, ILogger<DocumentController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _documentService = documentService;
            _feedService = feedService;
            _communityEntityService = communityEntityService;
            _documentConverter = documentConverter;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("{entityId}/documents")]
        [SwaggerOperation($"Adds a new document for the specified feed.")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was created", typeof(string))]
        public async Task<IActionResult> AddDocument([FromRoute] string entityId, [FromBody] DocumentInsertModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.ManageDocuments, CurrentUserId))
                return Forbid();

            var document = _mapper.Map<Document>(model);
            document.FeedId = entityId;

            await InitCreationAsync(document);
            var newDocumentId = await _documentService.CreateAsync(document);
            return Ok(newDocumentId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{documentId}")]
        [SwaggerOperation($"Returns a single document")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No document was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the parent entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document", typeof(DocumentModel))]
        public async Task<IActionResult> GetDocument([FromRoute] string documentId)
        {
            var document = await _documentService.GetAsync(documentId, CurrentUserId);
            if (document == null)
                return NotFound();

            if (!document.Permissions.Contains(Permission.Read))
                return Forbid();

            var documentModel = _mapper.Map<DocumentModel>(document);
            return Ok(documentModel);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{documentId}/download")]
        [SwaggerOperation($"Returns document data")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No document was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the parent entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document data as a file")]
        public async Task<IActionResult> GetDocumentData([FromRoute] string documentId)
        {
            var document = await _documentService.GetDataAsync(documentId, CurrentUserId);
            if (document == null)
                return NotFound();

            //if (!document.Permissions.Contains(Permission.Read))
            //    return Forbid();

            return File(document.Data, document.Filetype);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{entityId}/documents")]
        [SwaggerOperation($"Lists all documents for the specified entity")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document data", typeof(List<DocumentModel>))]
        public async Task<IActionResult> GetDocuments([FromRoute] string entityId, [FromQuery] string? folderId, [FromQuery] DocumentFilter? type, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.Read, CurrentUserId))
                return Forbid();

            var documents = _documentService.ListForEntity(CurrentUserId, entityId, folderId, type, model.Search, model.Page, model.PageSize);
            var documentModels = documents.Select(_mapper.Map<DocumentModel>);
            return Ok(documentModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{entityId}/documents/all")]
        [SwaggerOperation($"Lists all documents and folders for the specified entity, not including file data")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document and folder data", typeof(List<DocumentOrFolderModel>))]
        public async Task<IActionResult> GetDocumentsAndFolders([FromRoute] string entityId, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.Read, CurrentUserId))
                return Forbid();

            var folders = _documentService.ListAllFolders(entityId, model.Search, model.Page, model.PageSize / 2);
            var documents = _documentService.ListAllDocuments(CurrentUserId, entityId, model.Search, model.Page, model.PageSize / 2);
            var folderModels = folders.Select(_mapper.Map<DocumentOrFolderModel>);
            var documentModels = documents.Select(_mapper.Map<DocumentOrFolderModel>);

            var models = new List<DocumentOrFolderModel>();
            models.AddRange(folderModels);
            models.AddRange(documentModels);

            return Ok(models);
        }

        [HttpPut]
        [Route("{documentId}")]
        [SwaggerOperation($"Updates the document")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No document was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was updated")]
        public async Task<IActionResult> UpdateDocument([FromRoute] string documentId, [FromBody] DocumentUpdateModel model)
        {
            var existingDocument = _documentService.Get(documentId, CurrentUserId);
            if (existingDocument == null)
                return NotFound();

            if (!existingDocument.Permissions.Contains(Permission.Manage))
                return Forbid();

            var document = _mapper.Map<Document>(model);
            document.Id = ObjectId.Parse(documentId);
            document.FeedId = existingDocument.FeedId;
            await InitUpdateAsync(document);
            await _documentService.UpdateAsync(document);
            return Ok();
        }

        [HttpDelete]
        [Route("{documentId}")]
        [SwaggerOperation($"Deletes the specified document")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No document was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete the document")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was deleted")]
        public async Task<IActionResult> DeleteDocument([FromRoute] string documentId)
        {
            if (!_communityEntityService.HasPermission(documentId, Permission.Manage, CurrentUserId))
                return Forbid();

            await _documentService.DeleteAsync(documentId);
            return Ok();
        }

        [HttpPost]
        [Route("folders/{feedId}")]
        [SwaggerOperation($"Adds a new folder for the specified feed.")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add folders for the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The folder was created", typeof(string))]
        public async Task<IActionResult> AddFolder([FromRoute] string feedId, [FromBody] FolderUpsertModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Permission.ManageDocuments, CurrentUserId))
                return Forbid();

            var folder = _mapper.Map<Folder>(model);
            folder.FeedId = feedId;

            await InitCreationAsync(folder);
            var newFolderId = await _documentService.CreateFolderAsync(folder);
            return Ok(newFolderId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("folders/{feedId}")]
        [SwaggerOperation($"Lists all folders for the specified feed")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view folders for the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The folder data", typeof(List<FolderModel>))]
        public async Task<IActionResult> GetFolders([FromRoute] string feedId, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Data.Enum.Permission.Read, CurrentUserId))
                return Forbid();

            var folders = _documentService.ListAllFolders(feedId, model.Search, model.Page, model.PageSize);
            var folderModels = folders.Select(_mapper.Map<FolderModel>);
            return Ok(folderModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("folders/{feedId}/{parentFolderId}")]
        [SwaggerOperation($"Lists all folders in a specified parent folder, in a specified feed")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view folders for the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The folder data", typeof(List<FolderModel>))]
        public async Task<IActionResult> GetFolders([FromRoute] string feedId, [FromRoute] string parentFolderId, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(feedId, Data.Enum.Permission.Read, CurrentUserId))
                return Forbid();

            var folders = _documentService.ListFolders(feedId, parentFolderId, model.Search, model.Page, model.PageSize);
            var folderModels = folders.Select(_mapper.Map<FolderModel>);
            return Ok(folderModels);
        }

        [HttpPut]
        [Route("folders/{folderId}")]
        [SwaggerOperation($"Updates the folder")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid folder setup specified")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No folder was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit folders for the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The folder was updated")]
        public async Task<IActionResult> UpdateFolder([FromRoute] string folderId, [FromBody] FolderUpsertModel model)
        {
            if (model.ParentFolderId == folderId)
                return BadRequest();

            var existingFolder = _documentService.GetFolder(folderId);
            if (existingFolder == null)
                return NotFound();

            if (!_communityEntityService.HasPermission(existingFolder.FeedId, Data.Enum.Permission.ManageDocuments, CurrentUserId))
                return Forbid();

            var folder = _mapper.Map<Folder>(model);
            folder.Id = ObjectId.Parse(folderId);

            await InitUpdateAsync(folder);
            await _documentService.UpdateFolderAsync(folder);
            return Ok();
        }

        [HttpDelete]
        [Route("folders/{folderId}")]
        [SwaggerOperation($"Deletes the specified folder")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No folder was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete folders for the feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The folder was deleted")]
        public async Task<IActionResult> DeleteFolder([FromRoute] string folderId)
        {
            var existingFolder = _documentService.GetFolder(folderId);
            if (existingFolder == null)
                return NotFound();

            if (!_communityEntityService.HasPermission(existingFolder.FeedId, Data.Enum.Permission.ManageDocuments, CurrentUserId))
                return Forbid();

            await _documentService.DeleteFolderAsync(folderId);
            return Ok();
        }

        [HttpPost]
        [Route("convert/pdf")]
        public async Task<IActionResult> ConvertToPDF([FromBody] DocumentConversionUpsertModel model)
        {
            var conversion = _mapper.Map<FileData>(model);
            var file = _documentConverter.ConvertDocumentToPDF(conversion);
            var pdfData = _mapper.Map<string>(file);

            //refactor this into mapping etc
            return Ok($"data:application/pdf;base64,{pdfData}");
        }
    }
}