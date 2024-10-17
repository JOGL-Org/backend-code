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
    [ApiController]
    [Route("events")]
    public class EventController : BaseController
    {
        private readonly IEventService _eventService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IUserService _userService;
        private readonly IDocumentService _documentService;
        private readonly IMembershipService _membershipService;
        private readonly IWorkspaceService _workspaceService;
        private readonly INodeService _nodeService;
        private readonly IConfiguration _configuration;

        public EventController(IEventService eventService, ICommunityEntityService communityEntityService, IUserService userService, IDocumentService documentService, IMembershipService membershipService, IWorkspaceService workspaceService, INodeService nodeService, IConfiguration configuration, IMapper mapper, ILogger<EventController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _eventService = eventService;
            _communityEntityService = communityEntityService;
            _userService = userService;
            _documentService = documentService;
            _membershipService = membershipService;
            _workspaceService = workspaceService;
            _nodeService = nodeService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("{entityId}/events")]
        [SwaggerOperation($"Adds a new event for the specified community entity.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No entity was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add events for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The event was created", typeof(string))]
        public async Task<IActionResult> AddEvent([FromRoute] string entityId, [FromBody] EventUpsertModel model)
        {
            var entity = _communityEntityService.Get(entityId);
            if (entity == null)
                return NotFound();

            if (!entity.Permissions.Contains(Permission.CreateEvents))
                return Forbid();

            var ev = _mapper.Map<Event>(model);
            ev.CommunityEntityId = entityId;

            await InitCreationAsync(ev);
            var id = await _eventService.CreateAsync(ev);
            return Ok(id);
        }

        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation("List all accessible events for a given search query. Only events accessible to the currently logged in user will be returned")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of user events", typeof(ListPage<EventModel>))]
        public async Task<IActionResult> Search([FromQuery] SearchModel model)
        {
            var events = _eventService.List(CurrentUserId, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var models = events.Items.Select(_mapper.Map<EventModel>);
            return Ok(new ListPage<EventModel>(models, events.Total));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerOperation($"Returns a single event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the event")]
        public async Task<IActionResult> GetEvent([FromRoute] string id)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Read))
                return Forbid();

            var eventModel = _mapper.Map<EventModel>(ev);
            return Ok(eventModel);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{entityId}/events")]
        [SwaggerOperation($"Lists all events for the specified entity")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view events for the entity")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The event data", typeof(List<EventModel>))]
        public async Task<IActionResult> GetEvents([FromRoute] string entityId, [FromQuery] List<EventTag> tags, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] SearchModel model)
        {
            if (!_communityEntityService.HasPermission(entityId, Permission.Read, CurrentUserId))
                return Forbid();

            var events = _eventService.ListForEntity(CurrentUserId, entityId, tags, from, to, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var eventModels = events.Select(_mapper.Map<EventModel>);
            return Ok(eventModels);
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The event was updated")]
        public async Task<IActionResult> UpdateEvent([FromRoute] string id, [FromBody] EventUpsertModel model)
        {
            var existingEvent = _eventService.Get(id, CurrentUserId);
            if (existingEvent == null)
                return NotFound();

            if (!existingEvent.Permissions.Contains(Permission.Manage))
                return Forbid();

            var ev = _mapper.Map<Event>(model);
            ev.Id = ObjectId.Parse(id);
            ev.CommunityEntityId = existingEvent.CommunityEntityId;
            ev.ExternalId = existingEvent.ExternalId;
            await InitUpdateAsync(ev);
            await _eventService.UpdateAsync(ev);

            //process new attendances (invites)
            if (model.Attendances != null)
            {
                var attendances = model.Attendances.Select(_mapper.Map<EventAttendance>).ToList();
                PopulateAttendancesWithEventId(attendances, id);

                await InitCreationAsync(attendances);
                await InitUpdateAsync(attendances);
                await _eventService.UpsertAttendancesAsync(attendances, CurrentUserId, true);
            }

            //return updated data
            var updatedEv = _eventService.Get(id, CurrentUserId);
            var updatedEvModel = _mapper.Map<EventModel>(updatedEv);

            return Ok(updatedEvModel);
        }

        private void PopulateAttendancesWithEventId(IEnumerable<EventAttendance> attendances, string id)
        {
            foreach (var attendance in attendances)
            {
                attendance.EventId = id;
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerOperation($"Deletes an event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete this event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The event was deleted")]
        public async Task<IActionResult> DeleteEvent([FromRoute] string id)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Delete))
                return Forbid();

            await _eventService.DeleteAsync(id);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/attendances")]
        [SwaggerOperation($"Returns attendances for the specified event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see attendees for the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The event attendances", typeof(List<EventAttendanceModel>))]
        public async Task<IActionResult> ListAttendances([FromRoute] string id, [FromQuery] AttendanceAccessLevel? level, [FromQuery] AttendanceStatus? status, [FromQuery] AttendanceType? type, [FromQuery] List<string> labels, [FromQuery] List<CommunityEntityType> communityEntityTypes, [FromQuery] SearchModel model)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Read))
                return Forbid();

            var attendances = _eventService.ListAttendances(id, level, status, type, labels, communityEntityTypes, model.Search, model.Page, model.PageSize);
            var attendanceModels = attendances.Select(_mapper.Map<EventAttendanceModel>);
            return Ok(attendanceModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/ecosystem/communityEntities")]
        [SwaggerOperation($"Lists all ecosystem containers (projects, communities and nodes) for the given event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this event's contents", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<EntityMiniModel>))]
        public async Task<IActionResult> GetEcosystemCommunityEntities([SwaggerParameter("ID of the event")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            var entity = _communityEntityService.Get(id);
            List<CommunityEntity> communityEntities;
            switch (entity.Type)
            {
                case CommunityEntityType.Project:
                case CommunityEntityType.Workspace:
                    communityEntities = _nodeService.ListForCommunity(CurrentUserId, id, model.Search, model.Page, model.PageSize).Cast<CommunityEntity>().ToList();
                    break;

                case CommunityEntityType.Node:
                    communityEntities = _workspaceService.ListForNode(CurrentUserId, id, model.Search, model.Page, model.PageSize).Cast<CommunityEntity>().ToList();
                    break;
                default:
                    communityEntities = new List<CommunityEntity>();
                    break;
            }
            var communityEntityModels = communityEntities.Select(_mapper.Map<EntityMiniModel>);
            return Ok(communityEntityModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/ecosystem/users")]
        [SwaggerOperation($"Lists all ecosystem members for the given event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have rights to view this event's contents", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<EntityMiniModel>))]
        public async Task<IActionResult> GetEcosystemUsers([SwaggerParameter("ID of the event")][FromRoute] string id, [FromQuery] SearchModel model)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            var users = _userService.ListEcosystem(CurrentUserId, ev.CommunityEntityId, model.Search, model.Page, model.PageSize);
            var userModels = users.Select(_mapper.Map<EntityMiniModel>);
            return Ok(userModels);
        }

        [HttpPost]
        [Route("{id}/invite")]
        [SwaggerOperation($"Invites the user, email address or community entity to the specified event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to invite to the event")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, $"The user, email or community entity is already invited")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, $"Either the invite is empty (neither user id, community entity id nor email is populated) or the event is private and the community entity id is popuated")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The attendance was created")]
        public async Task<IActionResult> Invite([FromRoute] string id, [FromBody] EventAttendanceUpsertModel model)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            if (string.IsNullOrEmpty(model.UserId) && string.IsNullOrEmpty(model.UserEmail) && string.IsNullOrEmpty(model.CommunityEntityId))
                return BadRequest();

            //if (ev.Visibility == EventVisibility.Private && !string.IsNullOrEmpty(model.CommunityEntityId))
            //    return BadRequest();

            var attendance = _mapper.Map<EventAttendance>(model);
            attendance.EventId = id;

            var existingAttendance = _eventService.GetAttendanceForEventAndInvitee(attendance);
            if (existingAttendance != null)
                return Conflict();

            await InitCreationAsync(attendance);
            var attendanceId = await _eventService.CreateAttendanceAsync(attendance);

            return Ok(attendanceId);
        }

        [HttpPost]
        [Route("{id}/invite/batch")]
        [SwaggerOperation($"Invites a batch of users, email addresses or community entities to the specified event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to invite to the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The attendances were created")]
        public async Task<IActionResult> InviteBatch([FromRoute] string id, [FromBody] List<EventAttendanceUpsertModel> models)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            var attendances = models.Select(_mapper.Map<EventAttendance>).ToList();
            PopulateAttendancesWithEventId(attendances, id);

            await InitCreationAsync(attendances);
            await InitUpdateAsync(attendances);
            await _eventService.UpsertAttendancesAsync(attendances, CurrentUserId, false);

            return Ok();
        }

        [Obsolete]
        [HttpPost]
        [Route("{id}/invite/batch/communityEntity")]
        [SwaggerOperation($"Invites all members of a specific community entity or entities to an event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to invite to the event or to manage one of the community entities")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The attendances were created")]
        public async Task<IActionResult> InviteBatchCommunityEntityMembers([FromRoute] string id, [FromBody] List<string> communityEntityIds)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            var entities = communityEntityIds.Select(id => _communityEntityService.GetEnriched(id, CurrentUserId)).ToList();
            if (entities.Any(e => !e.Permissions.Contains(Permission.Manage)))
                return Forbid();

            var members = _membershipService.ListForEntities(CurrentUserId, communityEntityIds);
            var existingAttendances = _eventService.GetAttendancesForEvent(id);
            var attendances = members.DistinctBy(m => m.UserId).Select(m => new EventAttendance
            {
                AccessLevel = existingAttendances.FirstOrDefault(ea => ea.UserId == m.UserId)?.AccessLevel ?? AttendanceAccessLevel.Member,
                Labels = existingAttendances.FirstOrDefault(ea => ea.UserId == m.UserId)?.Labels,
                EventId = id,
                OriginCommunityEntityId = m.CommunityEntityId,
                Status = AttendanceStatus.Pending,
                UserId = m.UserId,
            }).ToList();

            await InitCreationAsync(attendances);
            await InitUpdateAsync(attendances);
            await _eventService.UpsertAttendancesAsync(attendances, CurrentUserId, false);

            return Ok();
        }

        [HttpPost]
        [Route("{id}/attend")]
        [SwaggerOperation($"Invites the current user to the specified event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the event")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, $"The user is already invited")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The attendance was created")]
        public async Task<IActionResult> Attend([FromRoute] string id, [FromQuery] string? originCommunityEntityId)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Read))
                return Forbid();

            var attendance = new EventAttendance
            {
                EventId = id,
                UserId = CurrentUserId,
                OriginCommunityEntityId = originCommunityEntityId,
                Status = AttendanceStatus.Yes
            };

            var existingAttendance = _eventService.GetAttendanceForEventAndInvitee(attendance);
            if (existingAttendance != null)
                return Conflict();

            await InitCreationAsync(attendance);
            var attendanceId = await _eventService.CreateAttendanceAsync(attendance);

            return Ok(attendanceId);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("{id}/attend/email")]
        [SwaggerOperation($"Invites the current user to the specified event (via email)")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to see the event")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, $"The email is already invited")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The attendance was created")]
        public async Task<IActionResult> AttendEmail([FromRoute] string id, [FromBody] EmailModel model)
        {
            var ev = _eventService.Get(id);
            if (ev == null)
                return NotFound();

            if (!_communityEntityService.HasPermission(id, Permission.Read, CurrentUserId))
                return Forbid();

            var attendance = new EventAttendance
            {
                EventId = id,
                UserId = CurrentUserId,
                UserEmail = model.Email,
                Status = AttendanceStatus.Yes
            };

            var existingAttendance = _eventService.GetAttendanceForEventAndInvitee(attendance);
            if (existingAttendance != null)
                return Conflict();

            await InitCreationAsync(attendance);
            var attendanceId = await _eventService.CreateAttendanceAsync(attendance);

            return Ok(attendanceId);
        }

        [HttpPost]
        [Route("attendances/{attendanceId}/accept")]
        [SwaggerOperation($"Accepts the event invitation on behalf of a user or community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No pending invitation was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to accept the invite")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The invite was accepted")]
        public async Task<IActionResult> AcceptInvite([FromRoute] string attendanceId)
        {
            var a = _eventService.GetAttendance(attendanceId);
            if (a == null)
                return NotFound();

            if (!string.IsNullOrEmpty(a.UserId))
            {
                if (a.UserId != CurrentUserId)
                    return Forbid();
            }
            else if (!string.IsNullOrEmpty(a.CommunityEntityId))
            {
                if (!_communityEntityService.HasPermission(a.CommunityEntityId, Permission.Manage, CurrentUserId))
                    return Forbid();
            }
            else
            {
                //email invitations are never accepted via this endpoint
                return Forbid();
            }

            await InitUpdateAsync(a);
            await _eventService.AcceptAttendanceAsync(a);
            return Ok();
        }

        [HttpPost]
        [Route("attendances/{attendanceId}/reject")]
        [SwaggerOperation($"Rejects the event invitation on behalf of a user or community entity")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No pending invitation was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to reject the invite")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The invite was rejected")]
        public async Task<IActionResult> RejectInvite([FromRoute] string attendanceId)
        {
            var a = _eventService.GetAttendance(attendanceId);
            if (a == null)
                return NotFound();

            if (!string.IsNullOrEmpty(a.UserId))
            {
                if (a.UserId != CurrentUserId)
                    return Forbid();
            }
            else if (!string.IsNullOrEmpty(a.CommunityEntityId))
            {
                if (!_communityEntityService.HasPermission(a.CommunityEntityId, Permission.Manage, CurrentUserId))
                    return Forbid();
            }
            else
            {
                //email invitations are never rejected via this endpoint
                return Forbid();
            }

            await InitUpdateAsync(a);
            await _eventService.RejectAttendanceAsync(a);
            return Ok();
        }

        [HttpDelete]
        [Route("attendances/{attendanceId}")]
        [SwaggerOperation($"Removes an attendance")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No attendance was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete attendees from the event")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, $"You are trying to remove the last organizer")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The attendance was deleted")]
        public async Task<IActionResult> RemoveAttendance([FromRoute] string attendanceId)
        {
            var a = _eventService.GetAttendance(attendanceId);
            if (a == null)
                return NotFound();

            var ev = _eventService.Get(a.EventId, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            var attendanceCount = _eventService.CountOrganizers(a.EventId);
            if (a.AccessLevel == AttendanceAccessLevel.Admin && attendanceCount <= 1)
                return BadRequest();

            await _eventService.DeleteAttendanceAsync(attendanceId);
            return Ok();
        }



        [HttpDelete]
        [Route("attendances/batch")]
        [SwaggerOperation($"Removes attendances in batch")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete attendees from the event")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, $"You can only update attendee access level within one event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The attendances were deleted")]
        public async Task<IActionResult> RemoveAttendanceBatch([FromBody] List<string> attendanceIds)
        {
            var attendances = _eventService.GetAttendances(attendanceIds);
            if (attendances.Count == 0)
                return Ok();

            var eventIds = attendances.Select(a => a.EventId).Distinct();
            if (eventIds.Count() > 1)
                return BadRequest();

            var ev = _eventService.Get(eventIds.Single(), CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            await _eventService.DeleteAttendancesAsync(attendances);
            return Ok();
        }

        [HttpPost]
        [Route("attendances/{attendanceId}/accessLevel")]
        [SwaggerOperation($"Updates the access level of an attendance")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No attendance was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to manage access level for the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The access level was set")]
        public async Task<IActionResult> SetAttendanceAccessLevel([FromRoute] string attendanceId, [FromBody] EventAttendanceAccessLevelModel model)
        {
            var a = _eventService.GetAttendance(attendanceId);
            if (a == null)
                return NotFound();

            var ev = _eventService.Get(a.EventId, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            a.AccessLevel = model.AccessLevel;
            await InitUpdateAsync(a);
            await _eventService.UpdateAsync(a);
            return Ok();
        }

        [HttpPost]
        [Route("attendances/{attendanceId}/accessLevel/batch")]
        [SwaggerOperation($"Updates the access level of a batch of attendees")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, $"You can only update attendee access level within one event")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to manage attendee access level for the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The access level was set")]
        public async Task<IActionResult> SetAttendanceAccessLevelBatch([FromBody] EventAttendanceLevelBatchModel model)
        {
            var attendances = _eventService.GetAttendances(model.AttendanceIds);
            if (attendances.Count == 0)
                return Ok();

            var eventIds = attendances.Select(a => a.EventId).Distinct();
            if (eventIds.Count() > 1)
                return BadRequest();

            var ev = _eventService.Get(eventIds.Single(), CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            foreach (var attendance in attendances)
            {
                attendance.AccessLevel = model.AccessLevel;
            }

            await InitUpdateAsync(attendances);
            await _eventService.UpdateAsync(attendances);
            return Ok();
        }

        [HttpPost]
        [Route("attendances/{attendanceId}/labels")]
        [SwaggerOperation($"Updates the labels of an attendee")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No attendance was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to manage attendee labels for the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The labels were set")]
        public async Task<IActionResult> SetAttendanceLabels([FromRoute] string attendanceId, [FromBody] List<string> labels)
        {
            var a = _eventService.GetAttendance(attendanceId);
            if (a == null)
                return NotFound();

            var ev = _eventService.Get(a.EventId, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            a.Labels = labels;
            await InitUpdateAsync(a);
            await _eventService.UpdateAsync(a);
            return Ok();
        }

        [HttpPost]
        [Route("attendances/{attendanceId}/labels/batch")]
        [SwaggerOperation($"Updates the labels of a batch of attendees")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, $"You can only update attendee labels within one event")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to manage attendee labels for the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The labels were set")]
        public async Task<IActionResult> SetAttendanceLabelsBatch([FromBody] EventAttendanceLabelBatchModel model)
        {
            var attendances = _eventService.GetAttendances(model.AttendanceIds);
            if (attendances.Count == 0)
                return Ok();

            var eventIds = attendances.Select(a => a.EventId).Distinct();
            if (eventIds.Count() > 1)
                return BadRequest();

            var ev = _eventService.Get(eventIds.Single(), CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Manage))
                return Forbid();

            foreach (var attendance in attendances)
            {
                attendance.Labels = model.Labels ?? new List<string>();
            }

            await InitUpdateAsync(attendances);
            await _eventService.UpdateAsync(attendances);
            return Ok();
        }

        [Obsolete]
        [HttpPost]
        [Route("{id}/documents")]
        [SwaggerOperation($"Adds a new document for the specified event.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to add documents for the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was created", typeof(string))]
        public async Task<IActionResult> AddDocument([FromRoute] string id, [FromBody] DocumentInsertModel model)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Data.Enum.Permission.ManageDocuments))
                return Forbid();

            var document = _mapper.Map<Document>(model);
            document.FeedId = ev.Id.ToString();
            await InitCreationAsync(document);
            var documentId = await _documentService.CreateAsync(document);
            return Ok(documentId);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/documents")]
        [SwaggerOperation($"Lists all documents for the specified event, not including file data")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Documents", typeof(string))]
        public async Task<IActionResult> GetDocuments([FromRoute] string id, [FromQuery] string? folderId, [FromQuery] DocumentFilter? type, [FromQuery] SearchModel model)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Data.Enum.Permission.Read))
                return Forbid();

            var documents = _documentService.ListForEntity(CurrentUserId, id, folderId, type, model.Search, model.Page, model.PageSize);
            var documentModels = documents.Select(_mapper.Map<DocumentModel>);
            return Ok(documentModels);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/documents/{documentId}")]
        [SwaggerOperation($"Returns a single document, including the file represented as base64")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to view documents for the event")]
        public async Task<IActionResult> GetDocument([FromRoute] string id, [FromRoute] string documentId)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            var document = await _documentService.GetDataAsync(documentId, CurrentUserId);
            if (document == null)
                return NotFound();

            if (document.FeedId != ev.Id.ToString())
                return NotFound();

            if (!ev.Permissions.Contains(Permission.Read))
                return Forbid();

            var documentModel = _mapper.Map<DocumentModel>(document);
            return Ok(documentModel);
        }

        [Obsolete]
        [HttpGet]
        [Route("{id}/documents/draft")]
        [SwaggerOperation($"Returns a draft document for the specified event")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for the specified id")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "No draft document was found for the event")]
        public async Task<IActionResult> GetDocumentDraft([FromRoute] string id)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
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
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to edit documents for the event")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was updated")]
        public async Task<IActionResult> UpdateDocument([FromRoute] string id, [FromRoute] string documentId, [FromBody] DocumentUpdateModel model)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            var existingDocument = await _documentService.GetAsync(documentId, CurrentUserId, false);
            if (existingDocument == null)
                return NotFound();

            if (existingDocument.FeedId != id)
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
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id or the document does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, $"The current user doesn't have sufficient rights to delete the document")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The document was deleted")]
        public async Task<IActionResult> DeleteDocument([FromRoute] string id, [FromRoute] string documentId)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            var existingDocument = await _documentService.GetAsync(documentId, CurrentUserId, false);
            if (existingDocument == null)
                return NotFound();

            if (existingDocument.FeedId != id)
                return NotFound();

            if (!ev.Permissions.Contains(Data.Enum.Permission.ManageDocuments))
                return Forbid();

            await _documentService.DeleteAsync(documentId);
            return Ok();
        }


        [HttpPost]
        [Route("{id}/message")]
        [SwaggerOperation($"Sends a message to selected attendees")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No event was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user does not have the rights to send messages to event attendees")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The message was successfully sent")]
        public async Task<IActionResult> Message([FromRoute] string id, [FromBody] MessageModel model)
        {
            var ev = _eventService.Get(id, CurrentUserId);
            if (ev == null)
                return NotFound();

            if (!ev.Permissions.Contains(Data.Enum.Permission.Manage))
                return Forbid();

            var redirectUrl = $"{_configuration["App:URL"]}/event/{id}";
            await _eventService.SendMessageToUsersAsync(id, model.UserIds, model.Subject, model.Message, redirectUrl);

            return Ok();
        }

    }
}
