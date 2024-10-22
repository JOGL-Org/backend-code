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
using Jogl.Server.DB;
using static SkiaSharp.HarfBuzz.SKShaper;
using Jogl.Server.Data.Util;
using Syncfusion.XlsIO.Implementation.XmlSerialization;

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
        private readonly IConfiguration _configuration;

        public ChannelController(IChannelService channelService, IDocumentService documentService, IMembershipService membershipService, IUserService userService, IConfiguration configuration, IMapper mapper, ILogger<ChannelController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _channelService = channelService;
            _documentService = documentService;
            _membershipService = membershipService;
            _userService = userService;
            _configuration = configuration;
        }

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

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("migration/channels")]
        //public async Task<IActionResult> RunChannelMigrations()
        //{
        //    var projectRepo = new ProjectRepository(_configuration);
        //    var communityRepo = new WorkspaceRepository(_configuration);
        //    var nodeRepo = new NodeRepository(_configuration);

        //    var channelRepo = new ChannelRepository(_configuration);

        //    foreach (var e in projectRepo.List(p => !p.Deleted))
        //    {
        //        if (!string.IsNullOrEmpty(e.HomeChannelId))
        //            continue;

        //        e.HomeChannelId = channelRepo.Get(c => c.CommunityEntityId == e.Id.ToString() && c.Title == "General")?.Id.ToString();
        //        await projectRepo.UpdateAsync(e);
        //    }

        //    foreach (var e in communityRepo.List(p => !p.Deleted))
        //    {
        //        if (!string.IsNullOrEmpty(e.HomeChannelId))
        //            continue;

        //        e.HomeChannelId = channelRepo.Get(c => c.CommunityEntityId == e.Id.ToString() && c.Title == "General")?.Id.ToString();
        //        await communityRepo.UpdateAsync(e);
        //    }

        //    foreach (var e in nodeRepo.List(p => !p.Deleted))
        //    {
        //        if (!string.IsNullOrEmpty(e.HomeChannelId))
        //            continue;

        //        e.HomeChannelId = channelRepo.Get(c => c.CommunityEntityId == e.Id.ToString() && c.Title == "General")?.Id.ToString();
        //        await nodeRepo.UpdateAsync(e);
        //    }

        //    return Ok();
        //}

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("migration/tabs")]
        //public async Task<IActionResult> RunTabMigration()
        //{
        //    var projectRepo = new ProjectRepository(_configuration);
        //    var communityRepo = new WorkspaceRepository(_configuration);
        //    var nodeRepo = new NodeRepository(_configuration);

        //    foreach (var e in projectRepo.List(p => !p.Deleted))
        //    {
        //        e.Tabs = new List<string> { "documents", "papers", "needs", "events" };
        //        await projectRepo.UpdateAsync(e);
        //    }

        //    foreach (var e in communityRepo.List(p => !p.Deleted))
        //    {
        //        e.Tabs = new List<string> { "documents", "papers", "needs", "events" };
        //        await communityRepo.UpdateAsync(e);
        //    }

        //    foreach (var e in nodeRepo.List(p => !p.Deleted))
        //    {
        //        e.Tabs = new List<string> { "documents", "papers", "needs", "events" };
        //        await nodeRepo.UpdateAsync(e);
        //    }

        //    return Ok();
        //}

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("migration/workspaces")]
        //public async Task<IActionResult> RunWorkspaceMigration()
        //{
        //    var projectRepo = new ProjectRepository(_configuration);
        //    var communityRepo = new WorkspaceRepository(_configuration);

        //    var feedRepo = new FeedRepository(_configuration);
        //    var membershipRepo = new MembershipRepository(_configuration);
        //    var relationRepo = new RelationRepository(_configuration);
        //    var invitationRepo = new InvitationRepository(_configuration);
        //    var ceInvitationRepo = new CommunityEntityInvitationRepository(_configuration);

        //    foreach (var comm in communityRepo.List(p => true))
        //    {
        //        if (string.IsNullOrEmpty(comm.Label))
        //        {
        //            comm.Label = "Workspace";
        //            await communityRepo.UpdateAsync(comm);
        //        }
        //    }

        //    foreach (var proj in projectRepo.List(p => true))
        //    {
        //        var comm = new Workspace
        //        {
        //            Id = proj.Id,
        //            BannerId = proj.BannerId,
        //            AccessLevel = proj.AccessLevel,
        //            CFPCount = proj.CFPCount,
        //            Channels = proj.Channels,
        //            WorkspaceCount = proj.WorkspaceCount,
        //            ContentAccessOrigin = proj.ContentAccessOrigin,
        //            ContentEntityCount = proj.ContentEntityCount,
        //            ContentPrivacy = proj.ContentPrivacy,
        //            ContentPrivacyCustomSettings = proj.ContentPrivacyCustomSettings,
        //            Contribution = proj.Contribution,
        //            CreatedBy = proj.CreatedBy,
        //            CreatedByUserId = proj.CreatedByUserId,
        //            CreatedUTC = proj.CreatedUTC,
        //            Deleted = proj.Deleted,
        //            Description = proj.Description,
        //            DocumentCount = proj.DocumentCount,
        //            DocumentCountAggregate = proj.DocumentCountAggregate,
        //            FeedId = proj.FeedId,
        //            //FeedLogoId = proj.FeedLogoId,
        //            //FeedTitle = proj.FeedTitle,
        //            //FeedType = proj.FeedType,
        //            HomeChannelId = proj.HomeChannelId,
        //            Interests = proj.Interests,
        //            JoiningRestrictionLevel = proj.JoiningRestrictionLevel,
        //            JoiningRestrictionLevelCustomSettings = proj.JoiningRestrictionLevelCustomSettings,
        //            Keywords = proj.Keywords,
        //            Label = "Project",
        //            LastActivityUTC = proj.LastActivityUTC,
        //            Level = proj.Level,
        //            Links = proj.Links,
        //            ListingAccessOrigin = proj.ListingAccessOrigin,
        //            ListingPrivacy = proj.ListingPrivacy,
        //            LogoId = proj.LogoId,
        //            MemberCount = proj.MemberCount,
        //            NeedCount = proj.NeedCount,
        //            NeedCountAggregate = proj.NeedCountAggregate,
        //            NodeCount = proj.NodeCount,
        //            OnboardedUTC = proj.OnboardedUTC,
        //            Onboarding = proj.Onboarding ?? new OnboardingConfiguration
        //            {
        //                Presentation = new OnboardingPresentation { Items = new List<OnboardingPresentationItem> { } },
        //                Questionnaire = new OnboardingQuestionnaire { Items = new List<OnboardingQuestionnaireItem> { } },
        //                Rules = new OnboardingRules { Text = string.Empty }
        //            },
        //            OrganizationCount = proj.OrganizationCount,
        //            PaperCount = proj.PaperCount,
        //            PaperCountAggregate = proj.PaperCountAggregate,
        //            ParticipantCount = proj.ParticipantCount,
        //            Path = proj.Path,
        //            Permissions = proj.Permissions,
        //            PostCount = proj.PostCount,
        //            ResourceCount = proj.ResourceCount,
        //            ResourceCountAggregate = proj.ResourceCountAggregate,
        //            Settings = proj.Settings,
        //            ShortDescription = proj.ShortDescription,
        //            ShortTitle = proj.ShortTitle,
        //            Status = proj.Status,
        //            Tabs = proj.Tabs,
        //            Title = proj.Title,
        //            //Type= CommunityEntityType.Workspace,
        //            UpdatedByUserId = proj.UpdatedByUserId,
        //            UpdatedUTC = proj.UpdatedUTC,
        //        };

        //        await communityRepo.CreateAsync(comm);
        //    }

        //    foreach (var feed in feedRepo.List(f => f.Type == FeedType.Project))
        //    {
        //        feed.Type = FeedType.Workspace;
        //        await feedRepo.UpdateAsync(feed);
        //    }

        //    foreach (var membership in membershipRepo.List(m => m.CommunityEntityType == CommunityEntityType.Project))
        //    {
        //        membership.CommunityEntityType = CommunityEntityType.Workspace;
        //        await membershipRepo.UpdateAsync(membership);
        //    }

        //    foreach (var relation in relationRepo.List(r => r.SourceCommunityEntityType == CommunityEntityType.Project))
        //    {
        //        relation.SourceCommunityEntityType = CommunityEntityType.Workspace;
        //        await relationRepo.UpdateAsync(relation);
        //    }

        //    foreach (var invitation in invitationRepo.List(i => i.CommunityEntityType == CommunityEntityType.Project))
        //    {
        //        invitation.CommunityEntityType = CommunityEntityType.Workspace;
        //        await invitationRepo.UpdateAsync(invitation);
        //    }

        //    foreach (var ceInvitation in ceInvitationRepo.List(i => i.SourceCommunityEntityType == CommunityEntityType.Project))
        //    {
        //        ceInvitation.SourceCommunityEntityType = CommunityEntityType.Workspace;
        //        await ceInvitationRepo.UpdateAsync(ceInvitation);
        //    }

        //    foreach (var ceInvitation in ceInvitationRepo.List(i => i.TargetCommunityEntityType == CommunityEntityType.Project))
        //    {
        //        ceInvitation.TargetCommunityEntityType = CommunityEntityType.Workspace;
        //        await ceInvitationRepo.UpdateAsync(ceInvitation);
        //    }

        //    //ensure no public workspaces
        //    foreach (var community in communityRepo.List(i => true))
        //    {
        //        if (community.ListingPrivacy == PrivacyLevel.Public)
        //            community.ListingPrivacy = PrivacyLevel.Ecosystem;

        //        if (community.ContentPrivacy == PrivacyLevel.Public)
        //            community.ContentPrivacy = PrivacyLevel.Ecosystem;

        //        await communityRepo.UpdateAsync(community);
        //    }

        //    return Ok();
        //}
    }
}