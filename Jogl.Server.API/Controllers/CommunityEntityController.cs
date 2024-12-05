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
    }
}