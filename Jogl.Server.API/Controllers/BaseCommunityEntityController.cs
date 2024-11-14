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
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Jogl.Server.API.Controllers
{
    public abstract class BaseCommunityEntityController<TEntity, TModel, TDetailModel, TListModel, TUpsertModel, TPatchModel> : BaseController where TEntity : CommunityEntity where TModel : CommunityEntityModel where TDetailModel : TModel where TListModel : CommunityEntityMiniModel where TUpsertModel : CommunityEntityUpsertModel
    {
        protected readonly IAccessService _accessService;
        protected readonly IInvitationService _invitationService;
        protected readonly IMembershipService _membershipService;
        protected readonly IUserService _userService;
        protected readonly IDocumentService _documentService;
        protected readonly ICommunityEntityService _communityEntityService;
        protected readonly ICommunityEntityInvitationService _communityEntityInvitationService;
        protected readonly ICommunityEntityMembershipService _communityEntityMembershipService;
        protected readonly IPaperService _paperService;
        protected readonly IResourceService _resourceService;
        protected readonly INeedService _needService;
        protected readonly IUrlService _urlService;
        protected readonly IContentService _contentService;
        protected readonly IEventService _eventService;
        protected readonly IChannelService _channelService;
        protected readonly IConfiguration _configuration;

        protected abstract TEntity GetEntity(string id);
        protected abstract TEntity GetEntityDetail(string id);
        protected abstract Task<string> CreateEntityAsync(TEntity e);
        protected abstract ListPage<TEntity> List(string search, int page, int pageSize, SortKey sort, bool ascending);
        protected abstract List<TEntity> Autocomplete(string userId, string search, int page, int pageSize);
        protected abstract Task UpdateEntityAsync(TEntity e);
        protected abstract Task DeleteEntity(string id);
        protected abstract CommunityEntityType EntityType { get; }

        protected abstract List<CommunityEntity> ListEcosystem(string id, string search, int page, int pageSize);
        protected abstract List<Workspace> ListCommunities(string id, string search, int page, int pageSize);
        protected abstract List<Data.Node> ListNodes(string id, string search, int page, int pageSize);
        protected abstract List<Organization> ListOrganizations(string id, string search, int page, int pageSize);
        protected abstract List<Resource> ListResources(string id, string search, int page, int pageSize);
        protected abstract ListPage<Paper> ListPapersAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, PaperType? type, List<PaperTag> tags, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        protected abstract ListPage<Document> ListDocumentsAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, DocumentFilter? type, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        protected abstract ListPage<Need> ListNeedsAggregate(string id, List<string> communityEntityIds, bool currentUser, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        protected abstract ListPage<Event> ListEventsAggregate(string id, List<CommunityEntityType> types, List<string> communityEntityIds, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending);

        protected BaseCommunityEntityController(IAccessService accessService, IInvitationService invitationService, IMembershipService membershipService, IUserService userService, IDocumentService documentService, ICommunityEntityService communityEntityService, ICommunityEntityInvitationService communityEntityInvitationService, ICommunityEntityMembershipService communityEntityMembershipService, IContentService contentService, IEventService eventService, IChannelService channelService, IPaperService paperService, IResourceService resourceService, INeedService needService, IUrlService urlService, IConfiguration configuration, IMapper mapper, ILogger logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _accessService = accessService;
            _invitationService = invitationService;
            _membershipService = membershipService;
            _userService = userService;
            _documentService = documentService;
            _communityEntityService = communityEntityService;
            _communityEntityInvitationService = communityEntityInvitationService;
            _communityEntityMembershipService = communityEntityMembershipService;
            _paperService = paperService;
            _resourceService = resourceService;
            _needService = needService;
            _urlService = urlService;
            _contentService = contentService;
            _eventService = eventService;
            _channelService = channelService;
            _configuration = configuration;
        }

        public async virtual Task<IActionResult> Create([FromBody] TUpsertModel model)
        {
            var entity = _mapper.Map<TEntity>(model);
            await InitCreationAsync(entity);
            var id = await CreateEntityAsync(entity);
            return Ok(id);
        }

        public async virtual Task<IActionResult> Get(string id)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (string.IsNullOrEmpty(CurrentUserId))
                return await GetByAnonymous<TModel, CommunityEntityMiniModel>(entity);

            return await GetByUser<TModel, CommunityEntityMiniModel>(entity);
        }

        public async virtual Task<IActionResult> GetDetail(string id)
        {
            var entity = GetEntityDetail(id);
            if (entity == null)
                return NotFound();

            if (string.IsNullOrEmpty(CurrentUserId))
                return await GetByAnonymous<TDetailModel, CommunityEntityMiniDetailModel>(entity);

            return await GetByUser<TDetailModel, CommunityEntityMiniDetailModel>(entity);
        }

        protected async virtual Task<IActionResult> GetByAnonymous<TTargetModel, TTargetMiniModel>(TEntity entity) where TTargetModel : TModel where TTargetMiniModel : CommunityEntityMiniModel
        {
            if (!entity.Permissions.Contains(Permission.Read))
            {
                var entityMiniModel = _mapper.Map<TTargetMiniModel>(entity);
                entityMiniModel.UserAccessLevel = "visitor";
                entityMiniModel.UserJoiningRestrictionLevel = "forbidden";
                return Ok(entityMiniModel);
            }

            var entityModel = _mapper.Map<TTargetModel>(entity);
            entityModel.UserAccessLevel = "visitor";

            return Ok(entityModel);
        }

        protected async virtual Task<IActionResult> GetByUser<TTargetModel, TTargetMiniModel>(TEntity entity) where TTargetModel : TModel where TTargetMiniModel : CommunityEntityMiniModel
        {
            var invitation = _invitationService.Get(entity.Id.ToString(), CurrentUserId);
            var membership = _membershipService.Get(entity.Id.ToString(), CurrentUserId);
            if (!entity.Permissions.Contains(Permission.Read))
            {
                var entityMiniModel = _mapper.Map<TTargetMiniModel>(entity);
                entityMiniModel.UserAccessLevel = _accessService.GetUserAccessLevel(membership, invitation);
                entityMiniModel.UserJoiningRestrictionLevel = _accessService.GetUserJoiningRestrictionLevel(membership, CurrentUserId, entity)?.ToString()?.ToLower() ?? "forbidden";
                return Ok(entityMiniModel);
            }

            var entityModel = _mapper.Map<TTargetModel>(entity);
            entityModel.UserAccessLevel = _accessService.GetUserAccessLevel(membership, invitation);
            entityModel.UserJoiningRestrictionLevel = _accessService.GetUserJoiningRestrictionLevel(membership, CurrentUserId, entity)?.ToString()?.ToLower() ?? "forbidden";

            return Ok(entityModel);
        }

        public async virtual Task<IActionResult> Autocomplete([FromQuery] SearchModel model)
        {
            var entities = Autocomplete(CurrentUserId, model.Search, model.Page, model.PageSize);
            var models = entities.Select(_mapper.Map<CommunityEntityMiniModel>);
            return Ok(models);
        }

        public async virtual Task<IActionResult> Search([FromQuery] SearchModel model)
        {
            var entities = List(model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var models = entities.Items.Select(_mapper.Map<TListModel>);
            return Ok(new ListPage<TListModel>(models, entities.Total));
        }

        public async virtual Task<IActionResult> Patch([FromRoute] string id, [FromBody] TPatchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            var upsertModel = _mapper.Map<TUpsertModel>(entity);
            ApplyPatchModel(model, upsertModel);

            var updatedEntity = _mapper.Map<TEntity>(upsertModel);
            await InitUpdateAsync(updatedEntity);
            await UpdateEntityAsync(updatedEntity);

            return Ok(id);
        }

        public async virtual Task<IActionResult> Update([FromRoute] string id, [FromBody] TUpsertModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            model.Id = id;
            var updatedEntity = _mapper.Map<TEntity>(model);
            await InitUpdateAsync(updatedEntity);
            await UpdateEntityAsync(updatedEntity);

            return Ok(id);
        }

        public async virtual Task<IActionResult> Delete([FromRoute] string id)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            await DeleteEntity(id);
            return Ok();
        }

        [Obsolete]
        [HttpPost]
        [Route("{id}/documents")]
        [SwaggerOperation($"Adds a new document for the specified entity.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Note-typed documents have to be created and updated through the feed endpoints")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was created", typeof(string))]
        public async Task<IActionResult> AddDocument([FromRoute] string id, [FromBody] DocumentInsertModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Data.Enum.Permission.ManageDocuments))
                return Forbid();

            var document = _mapper.Map<Document>(model);
            document.FeedId = entity.FeedId;
            await InitCreationAsync(document);
            var documentId = await _documentService.CreateAsync(document);
            return Ok(documentId);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/documents")]
        [SwaggerOperation($"Lists all documents for the specified entity, not including file data")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was created", typeof(string))]
        public async Task<IActionResult> GetDocuments([FromRoute] string id, [FromQuery] string? folderId, [FromQuery] DocumentFilter? type, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var documents = _documentService.ListForEntity(CurrentUserId, id, folderId, type, model.Search, model.Page, model.PageSize);
            var documentModels = documents.Select(_mapper.Map<DocumentModel>);
            return Ok(documentModels);
        }

        [AllowAnonymous]
        [Obsolete]
        [HttpGet]
        [Route("{id}/documents/{documentId}")]
        [SwaggerOperation($"Returns a single document, including the file represented as base64")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the entity")]
        public async Task<IActionResult> GetDocument([FromRoute] string id, [FromRoute] string documentId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var document = await _documentService.GetAsync(documentId, CurrentUserId);
            if (document == null)
                return NotFound();

            if (document.FeedId != entity.FeedId)
                return NotFound();

            if (!document.Permissions.Contains(Permission.Read))
                return Forbid();

            var documentModel = _mapper.Map<DocumentModel>(document);
            return Ok(documentModel);
        }

        [Obsolete]
        [HttpGet]
        [Route("{id}/documents/draft")]
        [SwaggerOperation($"Returns a draft document for the specified container")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity was found for the specified id")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "No draft document was found for the community entity")]
        public async Task<IActionResult> GetDocumentDraft([FromRoute] string id)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var paper = _documentService.GetDraft(id, CurrentUserId);
            if (paper == null)
                return NoContent();

            var documentModel = _mapper.Map<DocumentModel>(paper);
            return Ok(documentModel);
        }

        [Obsolete]
        [HttpPut]
        [Route("{id}/documents/{documentId}")]
        [SwaggerOperation($"Updates the title and description for the document")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was updated")]
        public async Task<IActionResult> UpdateDocument([FromRoute] string id, [FromRoute] string documentId, [FromBody] DocumentUpdateModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var existingDocument = await _documentService.GetAsync(documentId, CurrentUserId, false);
            if (existingDocument == null)
                return NotFound();

            if (existingDocument.FeedId != id)
                return NotFound();

            if (!existingDocument.Permissions.Contains(Permission.Manage))
                return Forbid();

            var document = _mapper.Map<Document>(model);
            document.Id = ObjectId.Parse(documentId);
            await InitUpdateAsync(document);
            await _documentService.UpdateAsync(document);
            return Ok();
        }

        [Obsolete]
        [HttpDelete]
        [Route("{id}/documents/{documentId}")]
        [SwaggerOperation($"Deletes the specified document")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete the document")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was deleted")]
        public async Task<IActionResult> DeleteDocument([FromRoute] string id, [FromRoute] string documentId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var existingDocument = await _documentService.GetAsync(documentId, CurrentUserId, false);
            if (existingDocument == null)
                return NotFound();

            if (existingDocument.FeedId != id)
                return NotFound();

            if (!existingDocument.Permissions.Contains(Permission.Manage))
                return Forbid();

            await _documentService.DeleteAsync(documentId);
            return Ok();
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/documents/all")]
        [SwaggerOperation($"Lists all documents and folders for the specified entity, not including file data")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was created", typeof(List<BaseModel>))]
        public async Task<IActionResult> GetDocumentsAndFolders([FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var folders = _documentService.ListAllFolders(id, model.Search, model.Page, model.PageSize / 2);
            var documents = _documentService.ListAllDocuments(CurrentUserId, id, model.Search, model.Page, model.PageSize / 2);
            var folderModels = folders.Select(_mapper.Map<FolderModel>);
            var documentModels = documents.Select(_mapper.Map<DocumentModel>);

            var models = new List<BaseModel>();
            models.AddRange(folderModels);
            models.AddRange(documentModels);

            return Ok(models);
        }

        [HttpPost]
        [Route("{id}/join")]
        [SwaggerOperation("Joins an entity on behalf of the currently logged in user. Only works for community entities with the Open membership access level. Creates a membership record immediately, no invitation is created.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "ID of the new membership object", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The entity does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "A membership record already exists for the entity and user")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The entity is not open to members joining")]
        public async Task<IActionResult> Join([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var membership = _membershipService.Get(id, CurrentUserId);
            if (membership != null)
                return Conflict();

            if (!entity.Permissions.Contains(Permission.Join))
                return Forbid();

            var existingInvitation = _invitationService.GetForUserAndEntity(CurrentUserId, id);
            if (existingInvitation != null)
                return Conflict();

            membership = new Membership
            {
                AccessLevel = AccessLevel.Member,
                UserId = CurrentUserId,
                CommunityEntityId = id,
                CommunityEntityType = EntityType,
                CommunityEntity = entity
            };

            await InitCreationAsync(membership);

            var membershipId = await _membershipService.CreateAsync(membership);
            return Ok(membershipId);
        }

        [HttpPost]
        [Route("{id}/request")]
        [SwaggerOperation("Creates a request to join an entity on behalf of the currently logged in user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "ID of the new invitation object", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The entity does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "An invitation already exists for the entity and user")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The entity is not open to members requesting to join")]
        public async Task<IActionResult> Request([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var membership = _membershipService.Get(id, CurrentUserId);
            if (membership != null)
                return Conflict();

            var level = _accessService.GetUserJoiningRestrictionLevel(membership, CurrentUserId, entity);
            if (level != JoiningRestrictionLevel.Request)
                return Forbid();

            var existingInvitation = _invitationService.GetForUserAndEntity(CurrentUserId, id);
            if (existingInvitation != null)
                return Conflict();

            var invitation = new Invitation
            {
                InviteeUserId = CurrentUserId,
                CommunityEntityId = id,
                CommunityEntityType = EntityType,
                Entity = entity,
                Type = InvitationType.Request
            };

            await InitCreationAsync(invitation);
            var invitationId = await _invitationService.CreateAsync(invitation);
            return Ok(invitationId);
        }

        [HttpPost]
        [Route("{id}/invite/id/{userId}")]
        [SwaggerOperation("Invite a user to an entity using their ID")]
        [SwaggerResponse((int)HttpStatusCode.OK, "ID of the new invitation object", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The entity or user do not exist")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "An invitation already exists for the entity and user")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The currently logged-in user does not have the rights to invite people to the specified entity, or the user is inviting an owner without having the rights to manage owners")]
        public async Task<IActionResult> InviteUserViaId([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the user to invite")][FromRoute] string userId, [SwaggerParameter("Access level to invite user with")][FromBody] AccessLevel accessLevel = AccessLevel.Member)
        {
            var user = _userService.Get(userId);
            if (user == null)
                return NotFound();

            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var existingMembership = _membershipService.Get(id, userId);
            if (existingMembership != null)
                return Conflict();

            var existingInvitation = _invitationService.GetForUserAndEntity(userId, id);
            if (existingInvitation != null)
                return Conflict();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            if (accessLevel == AccessLevel.Owner && !entity.Permissions.Contains(Permission.ManageOwners))
                return Forbid();

            var invitation = new Invitation
            {
                InviteeUserId = userId,
                CommunityEntityId = id,
                CommunityEntityType = EntityType,
                Entity = entity,
                Type = InvitationType.Invitation,
                AccessLevel = accessLevel,
            };

            await InitCreationAsync(invitation);
            var invitationId = await _invitationService.CreateAsync(invitation);
            return Ok(invitationId);
        }

        [HttpPost]
        [Route("{id}/invite/email/{userEmail}")]
        [SwaggerOperation("Invite a user to an entity using their email")]
        [SwaggerResponse((int)HttpStatusCode.OK, "ID of the new invitation object", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The currently logged-in user does not have the rights to invite people to the specified entity")]
        public async Task<IActionResult> InviteUserViaEmail([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the user to invite")][FromRoute] string userEmail, [FromBody] AccessLevel accessLevel = AccessLevel.Member)
        {
            var user = _userService.GetForEmail(userEmail);
            if (user != null)
                return await InviteUserViaId(id, user.Id.ToString(), accessLevel);

            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var existingInvitation = _invitationService.GetForEmailAndEntity(userEmail, id);
            if (existingInvitation != null)
                return Conflict();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            if (accessLevel == AccessLevel.Owner && !entity.Permissions.Contains(Permission.ManageOwners))
                return Forbid();

            var invitation = new Invitation
            {
                InviteeEmail = userEmail,
                CommunityEntityId = id,
                CommunityEntityType = EntityType,
                Entity = entity,
                Type = InvitationType.Invitation,
                AccessLevel = accessLevel
            };

            await InitCreationAsync(invitation);
            var invitationId = await _invitationService.CreateAsync(invitation);
            return Ok(invitationId);
        }

        [Obsolete]
        [HttpPost]
        [Route("{id}/invite/email/batch")]
        [SwaggerOperation("Invite a user to an entity using their email")]
        [SwaggerResponse((int)HttpStatusCode.OK, "ID of the new invitation object", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The currently logged-in user does not have the rights to invite people to the specified entity")]
        public async Task<IActionResult> InviteUserViaBatch([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the user to invite")][FromBody] List<string> userEmails)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            var invitations = userEmails.Select(i => new Invitation
            {
                CommunityEntityId = id,
                CommunityEntityType = EntityType,
                Entity = entity,
                Type = InvitationType.Invitation,
                InviteeEmail = i,
                AccessLevel = AccessLevel.Member,
                Status = InvitationStatus.Pending,
            }).ToList();

            await InitCreationAsync(invitations);
            var redirectUrl = $"{_configuration["App:URL"]}/signup";

            await _invitationService.CreateMultipleAsync(invitations, redirectUrl);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/invites")]
        [SwaggerOperation("List all pending invitations for a given entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of invitations", typeof(List<InvitationModelUser>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        public async Task<IActionResult> GetInvitations([SwaggerParameter("ID of the entity")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitations = _invitationService.ListForEntity(CurrentUserId, id, model.Search, model.Page, model.PageSize, true);
            var invitationModels = invitations.Select(_mapper.Map<InvitationModelUser>);
            return Ok(invitationModels);
        }

        [HttpPost]
        [Route("{id}/invites/{invitationId}/accept")]
        [SwaggerOperation("Accept the invite on behalf of the currently logged-in user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The invitation has been accepted. The user is now a member of the project.")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to accept the invitation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the invitation does not exist")]
        public async Task<IActionResult> AcceptInvite([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the invitation")][FromRoute] string invitationId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitation = _invitationService.Get(invitationId);
            if (invitation == null || invitation.CommunityEntityId != entity.Id.ToString())
                return NotFound();

            switch (invitation.Type)
            {
                case InvitationType.Request:
                    var membership = _membershipService.Get(id, CurrentUserId);
                    if (!entity.Permissions.Contains(Permission.Manage))
                        return Forbid();

                    break;
                case InvitationType.Invitation:
                    if (invitation.InviteeUserId != CurrentUserId)
                        return Forbid();

                    break;
            }

            await InitUpdateAsync(invitation);
            await _invitationService.AcceptAsync(invitation);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/invites/{invitationId}/reject")]
        [SwaggerOperation("Reject the invite on behalf of the currently logged-in user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The invitation has been rejected")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to reject the invitation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the invitation does not exist")]
        public async Task<IActionResult> RejectInvite([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the invitation")][FromRoute] string invitationId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitation = _invitationService.Get(invitationId);
            if (invitation == null || invitation.CommunityEntityId != entity.Id.ToString())
                return NotFound();

            switch (invitation.Type)
            {
                case InvitationType.Request:
                    if (!entity.Permissions.Contains(Permission.Manage))
                        return Forbid();
                    break;
                case InvitationType.Invitation:
                    if (invitation.InviteeUserId != CurrentUserId)
                        return Forbid();
                    break;
            }

            await InitUpdateAsync(invitation);
            await _invitationService.RejectAsync(invitation);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/invites/{invitationId}/cancel")]
        [SwaggerOperation("Cancels an invite")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The invitation has been cancelled")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to cancel the invitation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the invitation does not exist")]
        public async Task<IActionResult> CancelInvite([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the invitation")][FromRoute] string invitationId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitation = _invitationService.Get(invitationId);
            if (invitation == null || invitation.CommunityEntityId != entity.Id.ToString())
                return NotFound();

            switch (invitation.Type)
            {
                case InvitationType.Request:
                    if (invitation.InviteeUserId != CurrentUserId)
                        return Forbid();

                    break;
                case InvitationType.Invitation:
                    if (!entity.Permissions.Contains(Permission.Manage))
                        return Forbid();

                    break;
            }

            await InitUpdateAsync(invitation);
            await _invitationService.RejectAsync(invitation);
            return Ok();
        }

        [HttpPost]
        [Route("invites/{invitationId}/resend")]
        [SwaggerOperation("Resends an invite")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The invitation has been resent")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "The invitation is a request and cannot be resent")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The user does not have the right to resend the invitation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the invitation does not exist")]
        public async Task<IActionResult> ResendInvite([SwaggerParameter("ID of the invitation")][FromRoute] string invitationId)
        {
            var invitation = _invitationService.Get(invitationId);
            if (invitation == null)
                return NotFound();

            if (invitation.Type == InvitationType.Request)
                return BadRequest();

            var entity = GetEntity(invitation.CommunityEntityId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            await InitUpdateAsync(invitation);
            await _invitationService.ResendAsync(invitation);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/leave")]
        [SwaggerOperation("Leaves an entity on behalf of the currently logged in user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user has successfully left the entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The entity does not exist or the user is not a member")]
        public async Task<IActionResult> Leave([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var membership = _membershipService.Get(id, CurrentUserId);
            if (membership == null)
                return NotFound();

            await _membershipService.DeleteAsync(membership.Id.ToString());
            return Ok();
        }

        [HttpGet]
        [Route("{id}/onboardingResponses/{userId}")]
        [SwaggerOperation("List onboarding responses for a community entity and a user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "onboarding questionnaire responses", typeof(OnboardingQuestionnaireInstanceModel))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or no user was found")]
        public async Task<IActionResult> GetOnboardingData([SwaggerParameter("ID of the entity")][FromRoute] string id, [FromRoute] string userId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            var onboardingData = _membershipService.GetOnboardingInstance(id, userId);
            if (onboardingData == null)
                return NotFound();

            var onboardingInstanceModel = _mapper.Map<OnboardingQuestionnaireInstanceModel>(onboardingData);
            return Ok(onboardingInstanceModel);
        }

        [HttpPost]
        [Route("{id}/onboardingResponses")]
        [SwaggerOperation("Posts onboarding responses for a community entity for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The onboarding questionnaire responses were saved successfully")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        public async Task<IActionResult> UploadOnboardingData([SwaggerParameter("ID of the entity")][FromRoute] string id, [FromBody] OnboardingQuestionnaireInstanceUpsertModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var onboardingData = _mapper.Map<OnboardingQuestionnaireInstance>(model);
            onboardingData.CommunityEntityId = id;
            onboardingData.UserId = CurrentUserId;
            onboardingData.CompletedUTC = DateTime.UtcNow;

            await _membershipService.UpsertOnboardingInstanceAsync(onboardingData);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/onboardingCompletion")]
        [SwaggerOperation("Records the completion of the onboarding workflow for a community entity for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The onboarding completion was recorded successfully")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        public async Task<IActionResult> UploadOnboardingFinished([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var membership = _membershipService.Get(id, CurrentUserId);
            if (membership == null)
                return NotFound();

            membership.OnboardedUTC = DateTime.UtcNow;
            await _membershipService.UpdateAsync(membership);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/members")]
        [SwaggerOperation("List all members for an entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the entity's members")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of members", typeof(List<MemberModel>))]
        public async Task<IActionResult> GetMembers([SwaggerParameter("ID of the entity")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var members = _membershipService.ListForEntity(CurrentUserId, id, model.Search, model.Page, model.PageSize, true);
            var memberModels = members.Select(_mapper.Map<MemberModel>);
            return Ok(memberModels);
        }

        [HttpGet]
        [Route("{id}/invitation")]
        [SwaggerOperation("Returns the pending invitation for an object for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A pending invitation", typeof(InvitationModelEntity))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No invitation is pending or no entity was found for that id")]
        public async Task<IActionResult> GetPendingInvitation([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitation = _invitationService.GetForUserAndEntity(CurrentUserId, id);
            if (invitation == null)
                return NotFound();

            invitation.Entity = entity;
            return Ok(_mapper.Map<InvitationModelEntity>(invitation));
        }

        [HttpPut]
        [Route("{id}/members/{memberId}")]
        [SwaggerOperation("Updates a member's access level for an entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The access level has been updated")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The user does not have the right to set this access level for this member")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the user is not a member of that entity")]
        public async Task<IActionResult> UpdateMembership([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the entity")][FromRoute] string memberId, [SwaggerParameter("The new access level")][FromBody] AccessLevel level)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var membership = _membershipService.Get(id, memberId);
            if (membership == null)
                return NotFound();

            switch (membership.AccessLevel)
            {
                case AccessLevel.Owner:
                    if (!entity.Permissions.Contains(Permission.ManageOwners))
                        return Forbid();
                    break;
                default:
                    if (!entity.Permissions.Contains(Permission.Manage))
                        return Forbid();
                    break;
            }

            switch (level)
            {
                case AccessLevel.Owner:
                    if (!entity.Permissions.Contains(Permission.ManageOwners))
                        return Forbid();
                    break;
                default:
                    if (!entity.Permissions.Contains(Permission.Manage))
                        return Forbid();
                    break;
            }

            membership.AccessLevel = level;
            await InitUpdateAsync(membership);
            await _membershipService.UpdateAsync(membership);
            return Ok();
        }

        [HttpPut]
        [Route("{id}/members/{memberId}/contribution")]
        [SwaggerOperation("Updates a member's contribution")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The contribution has been updated")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The user does not have the right to set the contribution for other members")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the user is not a member of that entity")]
        public async Task<IActionResult> UpdateContribution([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the user")][FromRoute] string memberId, [SwaggerParameter("The updated contribution")][FromBody] string contribution)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var membership = _membershipService.Get(id, memberId);
            if (membership == null)
                return NotFound();

            if (membership.UserId != CurrentUserId)
                return Forbid();

            membership.Contribution = contribution;
            await InitUpdateAsync(membership);
            await _membershipService.UpdateAsync(membership);
            return Ok();
        }

        [HttpPut]
        [Route("{id}/members/{memberId}/labels")]
        [SwaggerOperation("Updates a member's labels")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The labels have been updated")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to set labels for members")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the user is not a member of that entity")]
        public async Task<IActionResult> UpdateLabels([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the user")][FromRoute] string memberId, [SwaggerParameter("The updated contribution")][FromBody] List<string> labels)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var membership = _membershipService.Get(id, memberId);
            if (membership == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            membership.Labels = labels;
            await InitUpdateAsync(membership);
            await _membershipService.UpdateAsync(membership);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}/members/{memberId}")]
        [SwaggerOperation("Removes a member's access from an entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The member has been removed")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to remove members from this entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the user is not a member of that entity")]
        public async Task<IActionResult> RemoveMembership([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the entity")][FromRoute] string memberId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var membership = _membershipService.Get(id, memberId);
            if (membership == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            await _membershipService.DeleteAsync(membership.Id.ToString());
            return Ok();
        }

        [HttpPost]
        [Route("{id}/communityEntityInvite/{targetEntityId}")]
        [SwaggerOperation("Invite a community entity to become affiliated to another entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "ID of the new invitation object", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "One of the entities does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "An invitation or affiliation already exists for the entities")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "An invitation cannot be extended to a community entity of the same type")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to invite entities to the specified entity")]
        public async Task<IActionResult> InviteCommunityEntityViaId([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the target entity")][FromRoute] string targetEntityId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var targetEntity = _communityEntityService.Get(targetEntityId);
            if (targetEntity == null)
                return NotFound();

            if (entity.Id == targetEntity.Id)
                return BadRequest();

            var existingMembership = _communityEntityMembershipService.GetForSourceAndTarget(id, targetEntityId);
            if (existingMembership != null)
                return Conflict();

            var existingInvitation = _communityEntityInvitationService.GetForSourceAndTarget(id, targetEntityId);
            if (existingInvitation != null)
                return Conflict();

            var existingInvitationBackwards = _communityEntityInvitationService.GetForSourceAndTarget(targetEntityId, id);
            if (existingInvitationBackwards != null)
                return Conflict();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            var invitation = new CommunityEntityInvitation
            {
                SourceCommunityEntity = entity,
                SourceCommunityEntityId = id,
                SourceCommunityEntityType = entity.Type,
                TargetCommunityEntityId = targetEntityId,
                TargetCommunityEntityType = targetEntity.Type,
                Status = InvitationStatus.Pending,
            };

            await InitCreationAsync(invitation);
            var urlFragment = _urlService.GetUrlFragment(EntityType);
            var redirectUrl = $"{_configuration["App:URL"]}/{urlFragment}/{id}";

            var invitationId = await _communityEntityInvitationService.CreateAsync(invitation, redirectUrl);
            return Ok(invitationId);
        }


        [HttpGet]
        [Route("{id}/communityEntityInvites/incoming")]
        [SwaggerOperation("List all pending invitations for a given entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of invitations", typeof(List<CommunityEntityInvitationModelSource>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        public async Task<IActionResult> GetCommunityEntityInvitationsTarget([SwaggerParameter("ID of the entity")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitations = _communityEntityInvitationService.ListForTarget(CurrentUserId, id, model.Search, model.Page, model.PageSize);
            var invitationModels = invitations.Select(_mapper.Map<CommunityEntityInvitationModelSource>);
            return Ok(invitationModels);
        }

        [HttpGet]
        [Route("{id}/communityEntityInvites/outgoing")]
        [SwaggerOperation("List all pending invitations for a given entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of invitations", typeof(List<CommunityEntityInvitationModelSource>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        public async Task<IActionResult> GetCommunityEntityInvitationsSource([SwaggerParameter("ID of the entity")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitations = _communityEntityInvitationService.ListForSource(CurrentUserId, id, model.Search, model.Page, model.PageSize);
            var invitationModels = invitations.Select(_mapper.Map<CommunityEntityInvitationModelTarget>);
            return Ok(invitationModels);
        }

        [HttpPost]
        [Route("{id}/communityEntityInvites/{invitationId}/accept")]
        [SwaggerOperation("Accept an invite")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The invitation has been accepted")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to accept the invitation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "One of the entities does not exist or the invitation does not exist")]
        public async Task<IActionResult> AcceptCommunityEntityInvite([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the invitation")][FromRoute] string invitationId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitation = _communityEntityInvitationService.Get(invitationId);
            if (invitation == null || invitation.TargetCommunityEntityId != entity.Id.ToString())
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            await InitUpdateAsync(invitation);
            await _communityEntityInvitationService.AcceptAsync(invitation);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/communityEntityInvites/{invitationId}/reject")]
        [SwaggerOperation("Rejects an invite")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The invitation has been rejected")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to reject the invitation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "One of the entities does not exist or the invitation does not exist")]
        public async Task<IActionResult> RejectCommunityEntityInvite([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the invitation")][FromRoute] string invitationId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitation = _communityEntityInvitationService.Get(invitationId);
            if (invitation == null || invitation.TargetCommunityEntityId != entity.Id.ToString())
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            await InitUpdateAsync(invitation);
            await _communityEntityInvitationService.RejectAsync(invitation);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/communityEntityInvites/{invitationId}/cancel")]
        [SwaggerOperation("Cancels an invite")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The invitation has been cancelled")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to cancel the invitation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "One of the entities does not exist or the invitation does not exist")]
        public async Task<IActionResult> CancelCommunityEntityInvite([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the invitation")][FromRoute] string invitationId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var invitation = _communityEntityInvitationService.Get(invitationId);
            if (invitation == null || invitation.SourceCommunityEntityId != entity.Id.ToString())
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            await InitUpdateAsync(invitation);
            await _communityEntityInvitationService.RejectAsync(invitation);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/communityEntity/{targetEntityId}/remove")]
        [SwaggerOperation("Removes an affiliation of an entity to another entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The entity affiliation has been removed successfully")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the permissions to manage the community entity's affiliations")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "One of the entities does not exist or the entities are not affiliated")]
        public async Task<IActionResult> Leave([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the target entity")][FromRoute] string targetEntityId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var targetEntity = _communityEntityService.Get(targetEntityId);
            if (targetEntity == null)
                return NotFound();

            var communityEntityMembership = _communityEntityMembershipService.GetForSourceAndTarget(id, targetEntityId);
            if (communityEntityMembership == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            await _communityEntityMembershipService.DeleteAsync(communityEntityMembership.Id.ToString());
            return Ok();
        }

        [HttpPost]
        [Route("{id}/communityEntity/{targetEntityId}/link")]
        [SwaggerOperation("Creates an affiliation of an entity to another entity. On the entity, the user must be admin or owner, or the toggle for members to allow creating community entities of the respective type must be on. The user must be admin or owner on the target entity.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The entity affiliation has been created successfully")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "One of the entities does not exist or the entities are already affiliated")]
        public async Task<IActionResult> Link([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("ID of the target entity")][FromRoute] string targetEntityId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var targetEntity = _communityEntityService.GetEnriched(targetEntityId, CurrentUserId);
            if (targetEntity == null)
                return NotFound();

            var communityEntityMembership = _communityEntityMembershipService.GetForSourceAndTarget(id, targetEntityId);
            if (communityEntityMembership != null)
                return Conflict();

            switch (targetEntity.Type)
            {
                case CommunityEntityType.Workspace:
                    if (!entity.Permissions.Contains(Data.Enum.Permission.CreateWorkspaces))
                        return Forbid();
                    break;
                default:
                    if (!entity.Permissions.Contains(Data.Enum.Permission.Manage))
                        return Forbid();
                    break;
            }

            switch (entity.Type)
            {
                case CommunityEntityType.Workspace:
                    if (!targetEntity.Permissions.Contains(Data.Enum.Permission.CreateWorkspaces))
                        return Forbid();
                    break;
                default:
                    if (!targetEntity.Permissions.Contains(Data.Enum.Permission.Manage))
                        return Forbid();
                    break;
            }

            var newMembership = new Relation
            {
                SourceCommunityEntityId = entity.Id.ToString(),
                SourceCommunityEntityType = entity.Type,
                TargetCommunityEntityId = targetEntity.Id.ToString(),
                TargetCommunityEntityType = targetEntity.Type,
            };

            await InitCreationAsync(newMembership);
            await _communityEntityMembershipService.CreateAsync(newMembership);
            return Ok();
        }

        [Obsolete]
        [HttpPost]
        [Route("{id}/papers")]
        [SwaggerOperation($"Adds a new paper and/or associates it to the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The community entity could not be found")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The ID of the new paper", typeof(string))]
        public async Task<IActionResult> AddPaper([SwaggerParameter("ID of the community entity")][FromRoute] string id, [FromBody] PaperUpsertModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Data.Enum.Permission.ManageLibrary))
                return Forbid();

            var paper = _mapper.Map<Paper>(model);
            paper.FeedId = id;
            await InitCreationAsync(paper);
            var paperId = await _paperService.CreateAsync( paper);

            return Ok(paperId);
        }

        [Obsolete]
        [HttpPost]
        [Route("{id}/papers/{paperId}")]
        [SwaggerOperation($"Associates a paper to the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity or no paper was found for the specified id")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "The paper is already added to the community entity's library")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the permissions to manage the community entity's library")]
        public async Task<IActionResult> AssociatePaper([SwaggerParameter("ID of the community entity")][FromRoute] string id, [FromRoute] string paperId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var paper = _paperService.Get(paperId, CurrentUserId);
            if (paper == null)
                return NotFound();

            if (!entity.Permissions.Contains(Data.Enum.Permission.ManageLibrary))
                return Forbid();

            if (paper.FeedIds.Any(eid => string.Equals(eid, id, StringComparison.InvariantCultureIgnoreCase)))
                return Conflict();

            await _paperService.AssociateAsync(id, paperId);
            return Ok();
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/papers")]
        [SwaggerOperation($"Lists all papers for the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<PaperModel>))]
        public async Task<IActionResult> GetPapers([SwaggerParameter("ID of the community entity")][FromRoute] string id, [SwaggerParameter("The paper type")][FromQuery] PaperType? type, [FromQuery] List<PaperTag> tags, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var papers = _paperService.ListForEntity(CurrentUserId, id, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var paperModels = papers.Select(_mapper.Map<PaperModel>);
            return Ok(paperModels);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("papers/{paperId}")]
        [SwaggerOperation($"Returns a single paper")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper was found for the paper id")]
        public async Task<IActionResult> GetPaper([FromRoute] string paperId)
        {
            var paper = _paperService.Get(paperId, CurrentUserId);
            if (paper == null)
                return NotFound();

            var paperModel = _mapper.Map<PaperModel>(paper);
            return Ok(paperModel);
        }

        [Obsolete]
        [HttpGet]
        [Route("{id}/papers/draft")]
        [SwaggerOperation($"Returns a draft paper for the specified container")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity was found for the specified id")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "No draft paper was found for the community entity")]
        public async Task<IActionResult> GetPaperDraft([FromRoute] string id)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var paper = _paperService.GetDraft(id, CurrentUserId);
            if (paper == null)
                return NoContent();

            var paperModel = _mapper.Map<PaperModel>(paper);
            return Ok(paperModel);
        }

        [Obsolete]
        [HttpPut]
        [Route("{id}/papers/{paperId}/draft")]
        [SwaggerOperation($"Updates the paper draft")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Cannot update active paper")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or the paper does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit documents for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The paper draft was updated")]
        public async Task<IActionResult> UpdatePaper([FromRoute] string id, [FromRoute] string paperId, [FromBody] PaperUpsertModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var existingPaper = _paperService.Get(paperId, CurrentUserId);
            if (existingPaper == null)
                return NotFound();

            if (existingPaper.Status != ContentEntityStatus.Draft)
                return BadRequest();

            if (!existingPaper.FeedIds.Contains(id))
                return NotFound();

            if (!entity.Permissions.Contains(Data.Enum.Permission.ManageDocuments))
                return Forbid();

            var paper = _mapper.Map<Paper>(model);
            paper.Id = ObjectId.Parse(paperId);
            paper.FeedIds = existingPaper.FeedIds;
            await InitUpdateAsync(paper);
            await _paperService.UpdateAsync(paper);
            return Ok();
        }

        [Obsolete]
        [HttpDelete]
        [Route("{id}/papers/{paperId}")]
        [SwaggerOperation($"Disassociates the specified paper from the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper was found for the paper id or the paper isn't associated to the given community entity")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to manage papers for the community entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The paper was deleted")]
        public async Task<IActionResult> DisassociatePaper([FromRoute] string id, [FromRoute] string paperId)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Data.Enum.Permission.ManageLibrary))
                return Forbid();

            await _paperService.DisassociateAsync(id, paperId);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/resources")]
        [SwaggerOperation($"Adds a new resource for the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The community entity could not be found")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The ID of the new resource", typeof(string))]
        public async Task<IActionResult> AddResource([SwaggerParameter("ID of the community entity")][FromRoute] string id, [FromBody] ResourceUpsertModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Data.Enum.Permission.PostResources))
                return Forbid();

            var resource = _mapper.Map<Resource>(model);
            resource.FeedId = entity.FeedId;
            await InitCreationAsync(resource);
            var resourceId = await _resourceService.CreateAsync(resource);

            return Ok(resourceId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/resources")]
        [SwaggerOperation($"Lists all resources for the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<ResourceModel>))]
        public async Task<IActionResult> GetResources([SwaggerParameter("ID of the community entity")][FromRoute] string id,/* [SwaggerParameter("The resource type")][FromQuery] ResourceType? type, */[FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var resources = ListResources(id, model.Search, model.Page, model.PageSize);
            var resourceModels = resources.Select(_mapper.Map<ResourceModel>);
            return Ok(resourceModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("resources/{resourceId}")]
        [SwaggerOperation($"Returns a single resource")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No resource was found for the resource id or the resource doesn't exist in the given community entity type")]
        public async Task<IActionResult> GetResource([FromRoute] string resourceId)
        {
            var resource = _resourceService.Get(resourceId);
            if (resource == null)
                return NotFound();

            var entity = GetEntity(resource.FeedId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var resourceModel = _mapper.Map<ResourceModel>(resource);
            return Ok(resourceModel);
        }

        [HttpPut]
        [Route("resources/{resourceId}")]
        [SwaggerOperation($"Updates the resource")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No resource was found for the resource id or the resource doesn't exist in the given community entity type")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to edit resources for the community entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The resource was updated")]
        public async Task<IActionResult> UpdateResource([FromRoute] string resourceId, [FromBody] ResourceUpsertModel model)
        {
            var resource = _resourceService.Get(resourceId);
            if (resource == null)
                return NotFound();

            var entity = GetEntity(resource.FeedId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            var updatedResource = _mapper.Map<Resource>(model);
            await InitUpdateAsync(updatedResource);
            updatedResource.Id = ObjectId.Parse(resourceId);
            await _resourceService.UpdateAsync(updatedResource);
            return Ok();
        }

        [HttpDelete]
        [Route("resources/{resourceId}")]
        [SwaggerOperation($"Deletes the specified resource")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No resource was found for the resource id or the resource doesn't exist in the given community entity type")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to edit resources for the community entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The resource was deleted")]
        public async Task<IActionResult> DeleteResource([FromRoute] string resourceId)
        {
            var resource = _resourceService.Get(resourceId);
            if (resource == null)
                return NotFound();

            var entity = GetEntity(resource.FeedId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            await _resourceService.DeleteAsync(resourceId);
            return Ok();
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/ecosystem/communityEntities")]
        [SwaggerOperation($"Lists all ecosystem containers (projects, communities and nodes) for the given community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community entity's contents", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<EntityMiniModel>))]
        public async Task<IActionResult> GetEcosystemCommunityEntities([SwaggerParameter("ID of the community entity")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communityEntities = ListEcosystem(id, model.Search, model.Page, model.PageSize / 2);
            var communityEntityModels = communityEntities.Select(_mapper.Map<EntityMiniModel>);
            return Ok(communityEntityModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/ecosystem/users")]
        [SwaggerOperation($"Lists all ecosystem members for the given community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community entity's contents", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<EntityMiniModel>))]
        public async Task<IActionResult> GetEcosystemUsers([SwaggerParameter("ID of the community entity")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var users = _userService.ListEcosystem(CurrentUserId, id, model.Search, model.Page, model.PageSize / 2);
            var userModels = users.Select(_mapper.Map<EntityMiniModel>);
            return Ok(userModels);
        }

        [Obsolete]
        [HttpPost]
        [Route("{id}/needs")]
        [SwaggerOperation($"Adds a new need for the specified project")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add needs for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The ID of the new need", typeof(string))]
        public async Task<IActionResult> AddNeed([SwaggerParameter("ID of the project")][FromRoute] string id, [FromBody] NeedUpsertModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.PostNeed))
                return Forbid();

            var need = _mapper.Map<Need>(model);
            need.EntityId = id;
            await InitCreationAsync(need);
            var needId = await _needService.CreateAsync(need);

            return Ok(needId);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/needs")]
        [SwaggerOperation($"Lists all needs for the specified community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No community entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community entity's contents", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<NeedModel>))]
        public async Task<IActionResult> GetNeeds([SwaggerParameter("ID of the community entity")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            //var membership = _membershipService.Get(id, CurrentUserId);
            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var needs = _needService.ListForEntity(CurrentUserId, id, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var needModels = needs.Select(_mapper.Map<NeedModel>);
            //foreach (var needModel in needModels)
            //{
            //    needModel.Entity.UserAccessLevel = _accessService.GetUserAccessLevel(membership);
            //}

            return Ok(needModels);
        }

        [Obsolete]
        [HttpGet]
        [Route("needs/{needId}")]
        [SwaggerOperation($"Returns a single need")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No need was found for the need id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this community entity's contents", typeof(string))]
        public async Task<IActionResult> GetNeed([FromRoute] string needId)
        {
            var need = _needService.Get(needId, CurrentUserId);
            if (need == null)
                return NotFound();

            if (!need.Permissions.Contains(Permission.Read))
                return Forbid();

            var needModel = _mapper.Map<NeedModel>(need);
            return Ok(needModel);
        }

        [Obsolete]
        [HttpPut]
        [Route("needs/{needId}")]
        [SwaggerOperation($"Updates the need")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No need was found for the need id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to update needs on this community entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The need was updated")]
        public async Task<IActionResult> UpdateNeed([FromRoute] string needId, [FromBody] NeedUpsertModel model)
        {
            var need = _needService.Get(needId, CurrentUserId);
            if (need == null)
                return NotFound();

            if (!need.Permissions.Contains(Permission.Manage))
                return Forbid();

            var updatedNeed = _mapper.Map<Need>(model);
            await InitUpdateAsync(updatedNeed);
            updatedNeed.Id = ObjectId.Parse(needId);
            await _needService.UpdateAsync(updatedNeed);
            return Ok();
        }

        [Obsolete]
        [HttpDelete]
        [Route("needs/{needId}")]
        [SwaggerOperation($"Deletes the specified need")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No need was found for the need id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to delete needs on this community entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The need was deleted")]
        public async Task<IActionResult> DeleteNeed([FromRoute] string needId)
        {
            var need = _needService.Get(needId, CurrentUserId);
            if (need == null)
                return NotFound();

            if (!need.Permissions.Contains(Permission.Manage))
                return Forbid();

            await _needService.DeleteAsync(needId);
            return Ok();
        }

        protected async Task<IActionResult> GetPapersAggregateAsync(string id, List<CommunityEntityType> types, List<string> communityEntityIds, PaperType? type, List<PaperTag> tags, SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var papers = ListPapersAggregate(id, types, communityEntityIds, type, tags, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var paperModels = papers.Items.Select(_mapper.Map<PaperModel>);
            return Ok(new ListPage<PaperModel>(paperModels, papers.Total));
        }

        protected async Task<IActionResult> GetDocumentsAggregateAsync(string id, List<CommunityEntityType> types, List<string> communityEntityIds, DocumentFilter? type, SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var documents = ListDocumentsAggregate(id, types, communityEntityIds, type, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var documentModels = documents.Items.Select(_mapper.Map<DocumentModel>);
            return Ok(new ListPage<DocumentModel>(documentModels, documents.Total));
        }

        protected async Task<IActionResult> GetNeedsAggregateAsync(string id, List<string> communityEntityIds, bool currentUser, SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var needs = ListNeedsAggregate(id, communityEntityIds, currentUser, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var needModels = needs.Items.Select(_mapper.Map<NeedModel>);
            return Ok(new ListPage<NeedModel>(needModels, needs.Total));
        }

        protected async Task<IActionResult> GetEventsAggregateAsync(string id, List<CommunityEntityType> types, List<string> communityEntityIds, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            var events = ListEventsAggregate(id, types, communityEntityIds, currentUser, tags, from, to, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var eventModels = events.Items.Select(_mapper.Map<EventModel>);
            return Ok(new ListPage<EventModel>(eventModels, events.Total));
        }

        protected async Task<IActionResult> GetCommunitiesAsync(string id, SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var communities = ListCommunities(id, model.Search, model.Page, model.PageSize);
            var communityModels = communities.Select(_mapper.Map<CommunityEntityMiniModel>);

            return Ok(communityModels);
        }

        protected async Task<IActionResult> GetNodesAsync(string id, SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var nodes = ListNodes(id, model.Search, model.Page, model.PageSize);
            var nodeModels = nodes.Select(_mapper.Map<CommunityEntityMiniModel>);

            return Ok(nodeModels);
        }

        protected async Task<IActionResult> GetOrganizationsAsync(string id, SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            //if (!entity.Permissions.Contains(Permission.Read))
            //    return Forbid();

            var organizations = ListOrganizations(id, model.Search, model.Page, model.PageSize);
            var organizationModels = organizations.Select(_mapper.Map<CommunityEntityMiniModel>);

            return Ok(organizationModels);
        }

        [Obsolete]
        [HttpPost]
        [Route("{id}/events")]
        [SwaggerOperation($"Adds a new event for the specified entity.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add events for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The event was created", typeof(string))]
        public async Task<IActionResult> AddEvent([FromRoute] string id, [FromBody] EventUpsertModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.CreateEvents))
                return Forbid();

            var e = _mapper.Map<Event>(model);
            e.CommunityEntityId = id;
            await InitCreationAsync(e);
            var eventId = await _eventService.CreateAsync(e);

            return Ok(eventId);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/events")]
        [SwaggerOperation($"Lists events for the specified entity.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view events for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Event data", typeof(List<EventModel>))]
        public async Task<IActionResult> GetEvents([FromRoute] string id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] List<EventTag> tags, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var events = _eventService.ListForEntity(id, CurrentUserId, tags, from, to, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var eventModels = events.Select(_mapper.Map<EventModel>);
            return Ok(eventModels);
        }

        [HttpPost]
        [Route("{id}/channels")]
        [SwaggerOperation($"Adds a new channel for the specified entity.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add discussion channels for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The channel was created", typeof(string))]
        public async Task<IActionResult> AddChannel([FromRoute] string id, [FromBody] ChannelUpsertModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.CreateChannels))
                return Forbid();

            var e = _mapper.Map<Channel>(model);
            e.CommunityEntityId = id;
            await InitCreationAsync(e);
            var channelId = await _channelService.CreateAsync(e);

            return Ok(channelId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/channels")]
        [SwaggerOperation($"Lists channels for the specified entity.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view channels for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"channel data", typeof(List<ChannelModel>))]
        public async Task<IActionResult> GetChannels([FromRoute] string id, [FromQuery] SearchModel model)
        {
            var entity = GetEntity(id);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var channels = _channelService.ListForEntity(CurrentUserId, id, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var channelModels = channels.Select(_mapper.Map<ChannelModel>);
            return Ok(channelModels);
        }
    }
}