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
using Syncfusion.DocIO.DLS;

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
        private readonly IMembershipService _membershipService;
        private readonly IConfiguration _configuration;

        public CommunityEntityController(IContentService contentService, ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IInvitationService invitationService, IMembershipService membershipService, IConfiguration configuration, IMapper mapper, ILogger<EntityController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _contentService = contentService;
            _communityEntityService = communityEntityService;
            _feedEntityService = feedEntityService;
            _invitationService = invitationService;
            _membershipService = membershipService;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation("Gets community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The community entity data", typeof(CommunityEntityMiniModel))]
        public async Task<IActionResult> Get([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var entity = _communityEntityService.GetEnriched(id, CurrentUserId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var model = _mapper.Map<CommunityEntityMiniModel>(entity);
            return Ok(model);
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
            var entity = _communityEntityService.GetEnriched(id, CurrentUserId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.Read))
                return Forbid();

            var members = _membershipService.ListForEntity(CurrentUserId, id, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var memberModels = members.Select(_mapper.Map<MemberModel>);
            return Ok(memberModels);
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

        [HttpGet]
        [Route("{id}/join/key")]
        [SwaggerOperation("Gets an invitation key for a community entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The invitation key", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        public async Task<IActionResult> GetJoinKey([FromRoute] string id)
        {
            var communityEntity = _communityEntityService.Get(id);
            if (communityEntity == null)
                return NotFound();

            var key = await _invitationService.GetInvitationKeyForEntityAsync(id);
            return Ok(key);
        }

        [HttpPost]
        [Route("{id}/join/key/{key}")]
        [SwaggerOperation("Joins a community entity using an invitation key")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The corresponding feed data", typeof(FeedModel))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No matching invitation key found")]
        public async Task<IActionResult> JoinWithKey([FromRoute] string id, [FromRoute] string key)
        {
            var invitation = _invitationService.GetInvitationKey(id, key);
            if (invitation == null)
                return NotFound();

            var feed = _feedEntityService.GetFeed(id);
            var communityEntity = _communityEntityService.Get(feed.Id.ToString());

            var membership = new Membership
            {
                CommunityEntity = communityEntity,
                CommunityEntityId = id,
                CommunityEntityType = communityEntity.Type,
                UserId = CurrentUserId,
                AccessLevel = AccessLevel.Member,
            };

            await InitCreationAsync(membership);
            await _membershipService.AddMembersAsync(new List<Membership> { membership });

            var feedModel = _mapper.Map<FeedModel>(feed);
            return Ok(feedModel);
        }

        [HttpGet]
        [Route("{id}/onboarding/{userId}")]
        [SwaggerOperation("List onboarding responses for a community entity and a user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "onboarding questionnaire responses", typeof(OnboardingQuestionnaireInstanceModel))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id or no user was found")]
        public async Task<IActionResult> GetOnboardingData([SwaggerParameter("ID of the entity")][FromRoute] string id, [FromRoute] string userId)
        {
            var communityEntity = _communityEntityService.GetEnriched(id, CurrentUserId);
            if (communityEntity == null)
                return NotFound();

            if (!communityEntity.Permissions.Contains(Permission.Manage))
                return Forbid();

            var onboardingData = _membershipService.GetOnboardingInstance(id, userId);
            if (onboardingData == null)
                return NotFound();

            var onboardingInstanceModel = _mapper.Map<OnboardingQuestionnaireInstanceModel>(onboardingData);
            return Ok(onboardingInstanceModel);
        }

        [HttpPost]
        [Route("{id}/onboarding")]
        [SwaggerOperation("Posts onboarding responses for a community entity for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The onboarding questionnaire responses were saved successfully")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        public async Task<IActionResult> UploadOnboardingData([SwaggerParameter("ID of the entity")][FromRoute] string id, [FromBody] OnboardingQuestionnaireInstanceUpsertModel model)
        {
            var communityEntity = _communityEntityService.GetEnriched(id, CurrentUserId);
            if (communityEntity == null)
                return NotFound();

            var onboardingData = _mapper.Map<OnboardingQuestionnaireInstance>(model);
            onboardingData.CommunityEntityId = id;
            onboardingData.UserId = CurrentUserId;
            onboardingData.CompletedUTC = DateTime.UtcNow;

            await _membershipService.UpsertOnboardingInstanceAsync(onboardingData);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/onboarding/complete")]
        [SwaggerOperation("Records the completion of the onboarding workflow for a community entity for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The onboarding completion was recorded successfully")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        public async Task<IActionResult> UploadOnboardingFinished([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var communityEntity = _communityEntityService.GetEnriched(id, CurrentUserId);
            if (communityEntity == null)
                return NotFound();

            var membership = _membershipService.Get(id, CurrentUserId);
            if (membership == null)
                return NotFound();

            membership.OnboardedUTC = DateTime.UtcNow;
            await _membershipService.UpdateAsync(membership);

            return Ok();
        }


        [HttpPost]
        [Route("{id}/leave")]
        [SwaggerOperation("Leaves an entity on behalf of the currently logged in user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user has successfully left the entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The entity does not exist or the user is not a member")]
        public async Task<IActionResult> Leave([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var communityEntity = _communityEntityService.GetEnriched(id, CurrentUserId);
            if (communityEntity == null)
                return NotFound();

            var membership = _membershipService.Get(id, CurrentUserId);
            if (membership == null)
                return NotFound();

            await _membershipService.DeleteAsync(membership);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/request")]
        [SwaggerOperation("Creates a request to join an entity on behalf of the currently logged in user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "ID of the new invitation object", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The entity does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "A request or invitation already exists for the entity and user")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to request to join the community entity")]
        public async Task<IActionResult> Request([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var communityEntity = _communityEntityService.GetEnriched(id, CurrentUserId);
            if (communityEntity == null)
                return NotFound();

            if (communityEntity.Permissions.Contains(Permission.Request))
                return Forbid();

            var existingInvitation = _invitationService.GetForUserAndEntity(CurrentUserId, id);
            if (existingInvitation != null)
                return Conflict();

            var invitation = new Invitation
            {
                InviteeUserId = CurrentUserId,
                CommunityEntityId = id,
                CommunityEntityType = communityEntity.Type,
                Entity = communityEntity,
                Type = InvitationType.Request
            };

            await InitCreationAsync(invitation);
            var invitationId = await _invitationService.CreateAsync(invitation);
            return Ok(invitationId);
        }

        [HttpPost]
        [Route("{id}/join")]
        [SwaggerOperation("Joins an entity on behalf of the currently logged in user. Only works for community entities with the Open membership access level. Creates a membership record immediately, no invitation is created.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "ID of the new membership object", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The entity does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "A request or invitation already exists for the entity and user")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the right to join the community entity")]
        public async Task<IActionResult> Join([SwaggerParameter("ID of the entity")][FromRoute] string id)
        {
            var communityEntity = _communityEntityService.GetEnriched(id, CurrentUserId);
            if (communityEntity == null)
                return NotFound();

            if (!communityEntity.Permissions.Contains(Permission.Join))
                return Forbid();

            var existingInvitation = _invitationService.GetForUserAndEntity(CurrentUserId, id);
            if (existingInvitation != null)
                return Conflict();

            var membership = new Membership
            {
                AccessLevel = AccessLevel.Member,
                UserId = CurrentUserId,
                CommunityEntityId = id,
                CommunityEntityType = communityEntity.Type,
                CommunityEntity = communityEntity
            };

            await InitCreationAsync(membership);

            var membershipId = await _membershipService.CreateAsync(membership);
            return Ok(membershipId);
        }

        [HttpPost]
        [Route("invite/mass")]
        [SwaggerOperation("Mass invite a batch of users to a set of community entities")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The users were successfully invited")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The currently logged-in user does not have the rights to invite people to the specified entity")]
        public async Task<IActionResult> InviteUsersMass([FromBody] InvitationMassUpsertModel model)
        {
            //var entity = _communityEntityService.GetEnriched(id, CurrentUserId);
            //if (entity == null)
            //    return NotFound();

            //if (!entity.Permissions.Contains(Permission.Manage))
            //    return Forbid();

            //var invitations = invitationModels.Select(i => new Invitation
            //{
            //    CommunityEntityId = id,
            //    CommunityEntityType = entity.Type,
            //    Entity = entity,
            //    Type = InvitationType.Invitation,
            //    InviteeEmail = i.Email,
            //    InviteeUserId = i.UserId,
            //    AccessLevel = i.AccessLevel,
            //    Status = InvitationStatus.Pending,
            //}).ToList();

            //await InitCreationAsync(invitations);
            //var redirectUrl = $"{_configuration["App:URL"]}/signup";

            //await _invitationService.CreateMultipleAsync(invitations, redirectUrl);
            return Ok();
        }
    }
}