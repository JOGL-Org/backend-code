using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [Route("conversations")]
    public class ConversationController : BaseController
    {
        public ConversationController(IEntityService entityService, IContextService contextService, IMapper mapper, ILogger logger) : base(entityService, contextService, mapper, logger)
        {
        }

        [HttpPost]
        [Route("{userId}")]
        [SwaggerOperation($"Starts a new conversation with the current user and another user")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user isn't connected to the specified user")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, $"There is already a conversation between the current user and the specified user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The conversation was created", typeof(string))]
        public async Task<IActionResult> StartConversation([FromRoute] string userId)
        {
            return Ok();
        }

        [HttpGet]
        [SwaggerOperation($"Lists conversations for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Conversation data", typeof(List<ConversationModel>))]
        public async Task<IActionResult> ListConversations()
        {
            return Ok(new List<ConversationModel> {
                new ConversationModel { Id = "xxx", User = new UserMiniModel { Id = "yyy", FirstName = "Homer", LastName = "Simpson" } },
                new ConversationModel { Id = "aaa", User = new UserMiniModel { Id = "bbb", FirstName = "Marge", LastName = "Simpson" } }
            });
        }

        [HttpGet]
        [Route("{userId}")]
        [SwaggerOperation($"Returns a single conversation")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No conversation was found for the selected user and current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The conversation data", typeof(ConversationModel))]
        public async Task<IActionResult> GetConversation([FromRoute] string id)
        {
            return Ok(new ConversationModel { Id = "xxx", User = new UserMiniModel { Id = "yyy", FirstName = "Homer", LastName = "Simpson" } });
        }

        [HttpDelete]
        [Route("{userId}")]
        [SwaggerOperation($"Deletes a conversation with a user")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No conversation was found for the selected user and current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The conversation was deleted")]
        public async Task<IActionResult> DeleteConversation([FromRoute] string id)
        {
            return Ok();
        }
    }
}