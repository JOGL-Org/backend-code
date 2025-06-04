using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.Data.Enum;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [Route("conversations")]
    public class ConversationController : BaseController
    {
        private readonly IUserConnectionService _userConnectionService;
        private readonly IConversationService _conversationService;
        private readonly IDocumentService _documentService;
        public ConversationController(IUserConnectionService userConnectionService, IConversationService conversationService, IDocumentService documentService, IEntityService entityService, IContextService contextService, IMapper mapper, ILogger<ConversationController> logger) : base(entityService, contextService, mapper, logger)
        {
            _userConnectionService = userConnectionService;
            _conversationService = conversationService;
            _documentService = documentService;
        }

        [HttpPost]
        [Route("{userId}")]
        [SwaggerOperation($"Starts a new conversation with the current user and another user")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user isn't connected to the specified user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The conversation was created", typeof(string))]
        public async Task<IActionResult> StartConversation([FromRoute] string userId)
        {
            var connection = _userConnectionService.Get(CurrentUserId, userId);
            if (connection?.Status != UserConnectionStatus.Accepted)
                return Forbid();

            var conversation = _conversationService.GetForUsers([CurrentUserId, userId], CurrentUserId);
            if (conversation != null)
                return Ok(conversation.Id.ToString());

            conversation = new Conversation
            {
                UserVisibility = new List<FeedEntityUserVisibility> {
                    new FeedEntityUserVisibility{ UserId = CurrentUserId, Visibility = FeedEntityVisibility.Comment},
                    new FeedEntityUserVisibility{ UserId = userId, Visibility= FeedEntityVisibility.Comment},
                    }
            };

            await InitCreationAsync(conversation);
            var res = await _conversationService.CreateAsync(conversation);
            return Ok(res);
        }

        [HttpGet]
        [SwaggerOperation($"Lists conversations for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Conversation data", typeof(ListPage<ConversationModel>))]
        public async Task<IActionResult> ListConversations()
        {
            var conversations = _conversationService.List(CurrentUserId, null, 0, int.MaxValue, Data.Util.SortKey.Alphabetical, false);
            var conversationModels = conversations.Items.Select(_mapper.Map<ConversationModel>);
            return Ok(new ListPage<ConversationModel>(conversationModels, conversations.Total));
        }

        [HttpGet]
        [Route("{userId}")]
        [SwaggerOperation($"Returns a single conversation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No conversation was found for the selected user and current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The conversation data", typeof(ConversationModel))]
        public async Task<IActionResult> GetConversation([FromRoute] string userId)
        {
            var conversation = _conversationService.GetForUsers([CurrentUserId, userId], CurrentUserId);
            if (conversation == null)
                return NotFound();

            var model = _mapper.Map<ConversationModel>(conversation);
            return Ok(model);
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes a conversation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No conversation was found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The user does not have the rights to delete the conversation")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The conversation was deleted")]
        public async Task<IActionResult> DeleteConversation([FromRoute] string id)
        {
            var conversation = _conversationService.Get(id, CurrentUserId);
            if (conversation == null)
                return NotFound();

            if (!conversation.Permissions.Contains(Permission.Delete))
                return Forbid();

            await _conversationService.DeleteAsync(conversation);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/documents")]
        [SwaggerOperation($"Returns documents for the conversation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No conversation was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the conversation")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document data", typeof(ListPage<DocumentModel>))]
        public async Task<IActionResult> GetDocuments([FromRoute] string id, [FromQuery] DocumentFilter type, [FromQuery] SearchModel model)
        {
            var conversation = _conversationService.Get(id, CurrentUserId);
            if (conversation == null)
                return NotFound();

            if (!conversation.Permissions.Contains(Permission.Read))
                return Forbid();

            var documents = _documentService.ListForChannel(CurrentUserId, id, type, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var documentModels = documents.Items.Select(d => _mapper.Map<DocumentModel>(d)).ToList();
            return Ok(new ListPage<DocumentModel>(documentModels, documents.Total));
        }
    }
}