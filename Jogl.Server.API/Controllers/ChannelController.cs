using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.Data.Util;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [Route("channels")]
    public class ChannelController : BaseController
    {
        private readonly IChannelService _channelService;
        private readonly IDocumentService _documentService;
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IConfiguration _configuration;

        public ChannelController(IChannelService channelService, IDocumentService documentService, IMembershipService membershipService, IUserService userService, ICommunityEntityService communityEntityService, IConfiguration configuration, IMapper mapper, ILogger<ChannelController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _channelService = channelService;
            _documentService = documentService;
            _membershipService = membershipService;
            _userService = userService;
            _communityEntityService = communityEntityService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("{entityId}/channels")]
        [SwaggerOperation($"Adds a new channel for the specified entity.")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add discussion channels for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The channel was created", typeof(string))]
        public async Task<IActionResult> AddChannel([FromRoute] string entityId, [FromBody] ChannelUpsertModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.CreateChannels, CurrentUserId))
                return Forbid();

            var e = _mapper.Map<Channel>(model);
            e.CommunityEntityId = entityId;
            await InitCreationAsync(e);
            var channelId = await _channelService.CreateAsync(e);

            return Ok(channelId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{entityId}/channels")]
        [SwaggerOperation($"Lists channels for the specified entity.")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view channels for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"channel data", typeof(List<ChannelModel>))]
        public async Task<IActionResult> GetChannels([FromRoute] string entityId, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.Read, CurrentUserId))
                return Forbid();

            var channels = _channelService.ListForEntity(CurrentUserId, entityId, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var channelModels = channels.Select(_mapper.Map<ChannelModel>);
            return Ok(channelModels);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation($"Returns a single channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the channel")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The channel data", typeof(ChannelModel))]
        public async Task<IActionResult> GetChannel([FromRoute] string id)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.Read))
                return Forbid();

            var channelModel = _mapper.Map<ChannelModel>(channel);
            return Ok(channelModel);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/detail")]
        [SwaggerOperation($"Returns a single channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the channel")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The channel data including detailed path", typeof(ChannelDetailModel))]
        public async Task<IActionResult> GetChannelDetail([FromRoute] string id)
        {
            var channel = _channelService.GetDetail(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.Read))
                return Forbid();

            var channelModel = _mapper.Map<ChannelDetailModel>(channel);
            return Ok(channelModel);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit the channel")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The channel was updated", typeof(ChannelModel))]
        public async Task<IActionResult> UpdateChannel([FromRoute] string id, [FromBody] ChannelUpsertModel model)
        {
            var existingChannel = _channelService.Get(id, CurrentUserId);
            if (existingChannel == null)
                return NotFound();

            if (!existingChannel.Permissions.Contains(Permission.Manage))
                return Forbid();

            var channel = _mapper.Map<Channel>(model);
            channel.Id = ObjectId.Parse(id);
            channel.CommunityEntityId = existingChannel.CommunityEntityId;
            await InitUpdateAsync(channel);
            await _channelService.UpdateAsync(channel);

            var updatedChannel = _channelService.Get(id, CurrentUserId);
            var updatedChannelModel = _mapper.Map<ChannelModel>(updatedChannel);

            return Ok(updatedChannelModel);
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes a channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete this channel")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The channel was deleted")]
        public async Task<IActionResult> DeleteChannel([FromRoute] string id)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.Delete))
                return Forbid();

            await _channelService.DeleteAsync(id);
            return Ok();
        }

        [HttpGet]
        [Route("{id}/members")]
        [SwaggerOperation($"Returns members for the specified channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see members for the channel")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The channel members", typeof(List<MemberModel>))]
        public async Task<IActionResult> ListMembers([FromRoute] string id, [FromQuery] SearchModel model)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.Read))
                return Forbid();

            var members = _membershipService.ListMembers(id, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var memberModels = members.Select(_mapper.Map<MemberModel>);
            return Ok(memberModels);
        }

        [HttpGet]
        [Route("{id}/users")]
        [SwaggerOperation($"Lists all eligible users to channel for a given channel, excluding users that have already joined the channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this channel's contents")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The eligible users", typeof(List<UserMiniModel>))]
        public async Task<IActionResult> ListEligibleUsers([SwaggerParameter("ID of the channel")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.Read))
                return Forbid();

            var channelMembers = _membershipService.ListMembers(id);
            var entityMembers = _membershipService.ListMembers(channel.CommunityEntityId, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var eligibleUsers = entityMembers.Select(m => m.User).Where(u => !channelMembers.Any(mu => mu.UserId == u.Id.ToString())).ToList();
            var eligibleUserModels = eligibleUsers.Select(_mapper.Map<UserMiniModel>);

            return Ok(eligibleUserModels);
        }

        [HttpPost]
        [Route("{id}/members")]
        [SwaggerOperation($"Adds a batch of users for the specified channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add members to the channel or the user isn't allowed to invite admins")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The membership data was created")]
        public async Task<IActionResult> AddMembers([FromRoute] string id, [FromBody] List<ChannelMemberUpsertModel> users)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.InviteMembers))
                return Forbid();

            if (!channel.Permissions.Contains(Permission.Manage) && users.Any(u => u.AccessLevel == SimpleAccessLevel.Admin))
                return Forbid();

            //process new members
            var memberships = users.Select(u => new Membership
            {
                CommunityEntityId = id,
                CommunityEntityType = CommunityEntityType.Channel,
                UserId = u.UserId,
                AccessLevel = _mapper.Map<AccessLevel>(u.AccessLevel)
            }).ToList();

            await InitCreationAsync(memberships);
            await _membershipService.AddMembersAsync(memberships);

            return Ok();
        }

        [HttpPut]
        [Route("{id}/members")]
        [SwaggerOperation($"Adds or updates a batch of users for the specified channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to update roles for channel members")]
        [SwaggerResponse((int)HttpStatusCode.FailedDependency, $"The operation cannot be performed, since it downgrades the only remaining admin or owner of the channel to member")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The membership data was updated")]
        public async Task<IActionResult> UpdateMembers([FromRoute] string id, [FromBody] List<ChannelMemberUpsertModel> users)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.Manage))
                return Forbid();

            //process new members
            var existingMemberships = _membershipService.ListMembers(id);
            var updatedMemberships = users.Select(u => new Membership
            {
                CommunityEntityId = id,
                CommunityEntityType = CommunityEntityType.Channel,
                UserId = u.UserId,
                AccessLevel = _mapper.Map<AccessLevel>(u.AccessLevel)
            }).ToList();

            //check that there are admins remaining
            if (!existingMemberships.Any(em => (updatedMemberships.SingleOrDefault(um => um.UserId == em.UserId)?.AccessLevel ?? em.AccessLevel) == AccessLevel.Admin))
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);

            await InitUpdateAsync(updatedMemberships);
            await _membershipService.UpdateMembersAsync(updatedMemberships);

            return Ok();
        }

        [HttpDelete]
        [Route("{id}/members")]
        [SwaggerOperation($"Removes a batch of users from the specified channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to remove members from the channel")]
        //[SwaggerResponse((int)HttpStatusCode.FailedDependency, $"The operation failed, since it removes the only remaining admin or owner of the channel")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The membership data was updated")]
        public async Task<IActionResult> RemoveMembers([FromRoute] string id, [FromBody] List<string> userIds)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.Manage))
                return Forbid();

            var memberships = _membershipService.ListMembers(id).Where(em => userIds.Contains(em.UserId)).ToList();
            await InitUpdateAsync(memberships);
            await _membershipService.RemoveMembersAsync(memberships);

            return Ok();
        }

        [HttpPost]
        [Route("{id}/members/join")]
        [SwaggerOperation($"Adds the current user to the specified channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the channel")]
        //[SwaggerResponse((int)HttpStatusCode.Conflict, $"The user is already a member")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The membership record was created")]
        public async Task<IActionResult> Join([FromRoute] string id)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.Read))
                return Forbid();

            if (!channel.Permissions.Contains(Permission.Join))
                return Forbid();

            var membership = new Membership
            {
                CommunityEntityId = id,
                CommunityEntityType = CommunityEntityType.Channel,
                UserId = CurrentUserId,
                AccessLevel = AccessLevel.Member,
            };

            await InitCreationAsync(membership);
            await _membershipService.AddMembersAsync(new Membership[] { membership }.ToList());
            return Ok();
        }

        [HttpPost]
        [Route("{id}/members/leave")]
        [SwaggerOperation($"Removes the current user from the specified channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, $"The current user isn't a member of this channel")]
        //[SwaggerResponse((int)HttpStatusCode.FailedDependency, $"The current user cannot leave, as they are the only remaining admin or owner of the channel")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The membership record was deleted")]
        public async Task<IActionResult> Leave([FromRoute] string id)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            var members = _membershipService.ListMembers(id);
            var membership = members.SingleOrDefault(m => m.UserId == CurrentUserId);
            if (membership == null)
                return Conflict();

            await InitUpdateAsync(membership);
            await _membershipService.RemoveMembersAsync(new List<Membership> { membership });

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/documents")]
        [SwaggerOperation($"Returns documents for the channel")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No channel was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the channel")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document data", typeof(ListPage<DocumentModel>))]
        public async Task<IActionResult> GetDocuments([FromRoute] string id, [FromQuery] DocumentFilter type, [FromQuery] SearchModel model)
        {
            var channel = _channelService.Get(id, CurrentUserId);
            if (channel == null)
                return NotFound();

            if (!channel.Permissions.Contains(Permission.Read))
                return Forbid();

            var documents = _documentService.ListForChannel(CurrentUserId, id, type, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var documentModels = documents.Items.Select(d => _mapper.Map<DocumentModel>(d)).ToList();
            return Ok(new ListPage<DocumentModel>(documentModels, documents.Total));
        }
    }
}