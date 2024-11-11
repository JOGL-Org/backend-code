using AutoMapper;
using Jogl.Server.Business;
using Jogl.Server.Data.Enum;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.API.Model;
using Jogl.Server.Data;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("communityEntities")]
    public class CommunityEntityController : BaseController
    {
        private readonly IContentService _contentService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IInvitationService _invitationService;
        private readonly IConfiguration _configuration;

        public CommunityEntityController(IContentService contentService, ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IInvitationService invitationService, IConfiguration configuration, IMapper mapper, ILogger<EntityController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _contentService = contentService;
            _communityEntityService = communityEntityService;
            _feedEntityService = feedEntityService;
            _invitationService = invitationService;
            _configuration = configuration;
        }


        [HttpPost]
        [Route("{id}/invite/batch")]
        [SwaggerOperation("Invite a batch of users to an entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The users were successfully invited")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The currently logged-in user does not have the rights to invite people to the specified entity")]
        public async Task<IActionResult> InviteUsersBatch([SwaggerParameter("ID of the entity")][FromRoute] string id, [SwaggerParameter("Users to invite")][FromBody] List<InvitationUpsertModel> invitationModels)
        {
            var entity = _communityEntityService.GetEnriched(id, CurrentUserId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Manage))
                return Forbid();

            var invitations = invitationModels.Select(i => new Invitation
            {
                CommunityEntityId = id,
                CommunityEntityType = entity.Type,
                Entity = entity,
                Type = InvitationType.Invitation,
                InviteeEmail = i.Email,
                InviteeUserId = i.UserId,
                AccessLevel = i.AccessLevel,
                Status = InvitationStatus.Pending,
            }).ToList();

            await InitCreationAsync(invitations);
            var redirectUrl = $"{_configuration["App:URL"]}/signup";

            await _invitationService.CreateMultipleAsync(invitations, redirectUrl);
            return Ok();
        }
    }
}