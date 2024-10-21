using Azure;
using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.Events;
using Jogl.Server.Notifications;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;

namespace Jogl.Server.Business
{
    public class EventService : BaseService, IEventService
    {
        private readonly ICalendarService _calendarService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly INotificationFacade _notificationFacade;

        public EventService(ICalendarService calendarService, ICommunityEntityService communityEntityService, IEmailService emailService, INotificationService notificationService, INotificationFacade notificationFacade, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _calendarService = calendarService;
            _communityEntityService = communityEntityService;
            _emailService = emailService;
            _notificationService = notificationService;
            _notificationFacade = notificationFacade;
        }

        public async Task<string> CreateAsync(Event ev)
        {
            //create feed
            var feed = new Feed()
            {
                CreatedUTC = ev.CreatedUTC,
                CreatedByUserId = ev.CreatedByUserId,
                Type = FeedType.Event,
            };

            var id = await _feedRepository.CreateAsync(feed);

            //mark feed write
            await _userFeedRecordRepository.SetFeedWrittenAsync(ev.CreatedByUserId, id, DateTime.UtcNow);

            //create document
            ev.Id = ObjectId.Parse(id);

            var externalCalendarId = await _calendarService.GetJoglCalendarAsync();
            var communityEntity = _communityEntityService.Get(ev.CommunityEntityId);
            var organizerAttendances = ev.Attendances.Where(ea => !string.IsNullOrEmpty(ea.UserId) && ea.AccessLevel == AttendanceAccessLevel.Admin);
            var organizerUserIds = organizerAttendances.Select(ea => ea.UserId).ToList();
            var organizers = _userRepository.Get(organizerUserIds);

            ev.CommunityEntity = communityEntity;
            ev.ExternalId = await _calendarService.CreateEventAsync(externalCalendarId, ev, organizers);
            await _eventRepository.CreateAsync(ev);
            await _notificationFacade.NotifyCreatedAsync(ev);

            //ensure invite list initialized
            if (ev.Attendances == null)
                ev.Attendances = new List<EventAttendance>();

            //ensure default invite present
            var creatorInvite = ev.Attendances.SingleOrDefault(ea => ea.UserId == ev.CreatedByUserId);
            if (creatorInvite == null)
            {
                creatorInvite = new EventAttendance
                {
                    UserId = ev.CreatedByUserId,
                    AccessLevel = AttendanceAccessLevel.Admin,
                };

                ev.Attendances.Add(creatorInvite);
            }

            creatorInvite.Status = AttendanceStatus.Yes;

            //process invites
            foreach (var ea in ev.Attendances)
            {
                ea.EventId = id;
                ea.CreatedByUserId = ev.CreatedByUserId;
                ea.CreatedUTC = ev.CreatedUTC;
            }

            await UpsertAttendancesAsync(ev.Attendances, ev.CreatedByUserId, true);

            //return id
            return id;
        }

        public Event Get(string eventId)
        {
            return _eventRepository.Get(eventId);
        }

        public Event Get(string eventId, string currentUserId)
        {
            var ev = _eventRepository.Get(eventId);
            if (ev == null)
                return null;

            var eventAttendances = _eventAttendanceRepository.List(a => a.EventId == eventId && !a.Deleted);
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);

            EnrichEventData(new List<Event> { ev }, eventAttendances, currentUserMemberships, currentUserId);
            ev.Path = _feedEntityService.GetPath(ev, currentUserId);
            RecordListing(currentUserId, ev);
            return ev;
        }

        public ListPage<Event> List(string currentUserId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var events = _eventRepository.SearchSort(search, sortKey, ascending);
            var eventAttendances = _eventAttendanceRepository.List(a => events.Select(e => e.Id.ToString()).ToList().Contains(a.EventId) && !a.Deleted);
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            var filteredEvents = GetFilteredEvents(events, eventAttendances, currentUserMemberships, currentUserId);

            var filteredEventPage = GetPage(filteredEvents, page, pageSize);
            EnrichEventData(filteredEventPage, eventAttendances, currentUserMemberships, currentUserId);

            return new ListPage<Event>(filteredEventPage, filteredEvents.Count);
        }

        public long Count(string currentUserId, string search)
        {
            var events = _eventRepository.Search(search);
            var eventAttendances = _eventAttendanceRepository.List(a => events.Select(e => e.Id.ToString()).ToList().Contains(a.EventId) && !a.Deleted);
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            var filteredEvents = GetFilteredEvents(events, eventAttendances, currentUserMemberships, currentUserId);

            return filteredEvents.Count;
        }

        public List<Event> ListForEntity(string entityId, string currentUserId, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var events = _eventRepository.SearchListSort(e => e.CommunityEntityId == entityId && (from == null || e.Start > from) && (to == null || e.Start < to) && !e.Deleted, sortKey, ascending, search);
            //var ownAttendances = _eventAttendanceRepository.List(a => a.CommunityEntityId == entityId && a.Status == AttendanceStatus.Yes && !a.Deleted);
            //var linkedEvents = _eventRepository.Get(ownAttendances.Select(a => a.EventId).ToList());

            //var events = ownEvents.Union(linkedEvents);
            var eventAttendances = _eventAttendanceRepository.List(a => events.Select(e => e.Id.ToString()).ToList().Contains(a.EventId) && !a.Deleted);
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);

            var filteredEvents = GetFilteredEvents(events, eventAttendances, currentUserMemberships, currentUserId, tags ?? new List<EventTag>(), page, pageSize);
            EnrichEventData(filteredEvents, eventAttendances, currentUserMemberships, currentUserId);
            RecordListings(currentUserId, filteredEvents);

            return filteredEvents;
        }

        public ListPage<Event> ListForNode(string nodeId, string currentUserId, List<CommunityEntityType> types, List<string> communityEntityIds, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);

            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var events = _eventRepository.SearchListSort(e => entityIds.Contains(e.CommunityEntityId) && (from == null || e.Start > from) && (to == null || e.Start < to) && !e.Deleted, sortKey, ascending, search);
            var eventAttendances = _eventAttendanceRepository.List(a => events.Select(e => e.Id.ToString()).ToList().Contains(a.EventId) && !a.Deleted);
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);

            if (currentUser)
                events = events.Where(e => IsEventForUser(e, eventAttendances.Where(ea => ea.EventId == e.Id.ToString()), currentUserId)).ToList();

            var filteredEvents = GetFilteredEvents(events, eventAttendances, currentUserMemberships, currentUserId);
            var total = filteredEvents.Count;

            var filteredEventPage = GetPage(filteredEvents, page, pageSize);
            EnrichEventData(filteredEventPage, eventAttendances, currentUserMemberships, currentUserId);
            RecordListings(currentUserId, filteredEventPage);

            return new ListPage<Event>(filteredEventPage, total);
        }

        public long CountForNode(string userId, string nodeId, string search)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);

            var events = _eventRepository.SearchList(e => entityIds.Contains(e.CommunityEntityId) && !e.Deleted, search);
            var eventAttendances = _eventAttendanceRepository.List(a => events.Select(e => e.Id.ToString()).ToList().Contains(a.EventId) && !a.Deleted);
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == userId && !p.Deleted);

            var filteredEvents = GetFilteredEvents(events, eventAttendances, currentUserMemberships, userId);
            return filteredEvents.Count;
        }

        public List<Event> ListForUser(string userId, string currentUserId, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var currentUserAttendances = _eventAttendanceRepository.List(a => a.UserId == userId && !a.Deleted);
            var eventIds = currentUserAttendances.Select(a => a.EventId).ToList();
            var events = _eventRepository.SearchListSort(e => eventIds.Contains(e.Id.ToString()) && (from == null || e.Start > from) && (to == null || e.Start < to), sortKey, ascending, search);
            var eventAttendances = _eventAttendanceRepository.List(a => events.Select(e => e.Id.ToString()).ToList().Contains(a.EventId) && !a.Deleted);
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);

            var filteredEvents = GetFilteredEvents(events, eventAttendances, currentUserMemberships, currentUserId, tags ?? new List<EventTag>(), page, pageSize);
            EnrichEventData(filteredEvents, eventAttendances, currentUserMemberships, currentUserId);
            RecordListings(currentUserId, filteredEvents);

            return filteredEvents;
        }

        public List<Event> ListForOrganization(string organizationId, string currentUserId, List<CommunityEntityType> types, List<string> communityEntityIds, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var entityIds = GetCommunityEntityIdsForOrg(organizationId);

            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var events = _eventRepository.SearchListSort(e => entityIds.Contains(e.CommunityEntityId) && (from == null || e.Start > from) && (to == null || e.Start < to) && !e.Deleted, sortKey, ascending, search);
            var eventAttendances = _eventAttendanceRepository.List(a => events.Select(e => e.Id.ToString()).ToList().Contains(a.EventId) && !a.Deleted);
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);

            var filteredEvents = GetFilteredEvents(events, eventAttendances, currentUserMemberships, currentUserId, tags ?? new List<EventTag>(), page, pageSize);
            EnrichEventData(filteredEvents, eventAttendances, currentUserMemberships, currentUserId);
            RecordListings(currentUserId, filteredEvents);

            return filteredEvents;
        }

        public async Task UpdateAsync(Event ev)
        {
            var existingEv = _eventRepository.Get(ev.Id.ToString());
            var user = _userRepository.Get(ev.UpdatedByUserId);
            var externalCalendarId = await _calendarService.GetJoglCalendarAsync();
            var communityEntity = _communityEntityService.Get(ev.CommunityEntityId);
            var organizerAttendances = _eventAttendanceRepository.List(ea => ea.EventId == ev.Id.ToString() && !string.IsNullOrEmpty(ea.UserId) && ea.AccessLevel == AttendanceAccessLevel.Admin && !ea.Deleted);
            var organizerUserIds = organizerAttendances.Select(ea => ea.UserId).ToList();
            var organizers = _userRepository.Get(organizerUserIds);

            ev.CommunityEntity = communityEntity;
            await _calendarService.UpdateEventAsync(externalCalendarId, ev, organizers, ev.GenerateMeetLink && !existingEv.GenerateMeetLink, ev.GenerateZoomLink && !existingEv.GenerateZoomLink);
            await _eventRepository.UpdateAsync(ev);

            //if event has changed date, send out notifications
            if (ev.Start != existingEv.Start || ev.End != existingEv.End || ev.Title != existingEv.Title)
            {
                var attendances = _eventAttendanceRepository.List(ea => ea.EventId == ev.Id.ToString() && !string.IsNullOrEmpty(ea.UserId) && !ea.Deleted);
                await _notificationService.NotifyEventInviteUpdatedAsync(ev, communityEntity, user, attendances);
            }
        }

        public async Task DeleteAsync(string eventId)
        {
            var ev = _eventRepository.Get(eventId);
            var externalCalendarId = await _calendarService.GetJoglCalendarAsync();
            await _calendarService.DeleteEventAsync(externalCalendarId, ev.ExternalId);

            var attendances = _eventAttendanceRepository.List(a => a.EventId == eventId && !a.Deleted);
            await _notificationService.NotifyEventInvitesWithdrawAsync(attendances);
            foreach (var a in attendances)
            {
                await _eventAttendanceRepository.DeleteAsync(a);
            }

            await DeleteFeedAsync(eventId);
            await _eventRepository.DeleteAsync(eventId);
        }

        public async Task<string> CreateAttendanceAsync(EventAttendance attendance)
        {
            var res = await UpsertAttendancesAsync(new List<EventAttendance> { attendance }, attendance.CreatedByUserId, true);
            return res.SingleOrDefault();
        }

        public async Task<List<string>> UpsertAttendancesAsync(List<EventAttendance> attendances, string currentUserId, bool upsert)
        {
            if (!attendances.Any())
                return new List<string>();

            attendances = attendances.DistinctBy(ea => ea, new EventAttendanceEqualityComparer()).ToList();

            var eventId = attendances.Select(a => a.EventId).Distinct().Single();
            var existingAttendances = _eventAttendanceRepository.List(a => a.EventId == eventId && !a.Deleted);
            var ev = _eventRepository.Get(eventId);
            var evCommunityEntity = _communityEntityService.Get(ev.CommunityEntityId);
            var externalCalendarId = await _calendarService.GetJoglCalendarAsync();
            var user = _userRepository.Get(currentUserId);
            var userMemberships = _membershipRepository.List(m => m.UserId == currentUserId && !m.Deleted);

            //if (ev.Visibility == EventVisibility.Private)
            //    attendances = attendances.Where(a => string.IsNullOrEmpty(a.CommunityEntityId)).ToList();

            //map email invitations to Jogl users if they exist
            var emails = attendances.Where(a => !string.IsNullOrEmpty(a.UserEmail)).Select(a => a.UserEmail).Distinct().ToList();
            var directInviteEmailUsers = _userRepository.List(u => emails.Any(email => u.Email == email && !u.Deleted));
            foreach (var directInviteUser in directInviteEmailUsers)
            {
                var attendanceForUser = attendances.SingleOrDefault(a => string.Equals(a.UserEmail, directInviteUser.Email, StringComparison.CurrentCultureIgnoreCase));
                if (attendanceForUser == null)
                    continue;

                attendanceForUser.UserEmail = null;
                attendanceForUser.UserId = directInviteUser.Id.ToString();
            }

            //map community entity types
            var communityEntities = _communityEntityService.List(attendances.Where(a => !string.IsNullOrEmpty(a.CommunityEntityId)).Select(a => a.CommunityEntityId));
            foreach (var attendance in attendances.Where(a => !string.IsNullOrEmpty(a.CommunityEntityId)))
            {
                var communityEntity = communityEntities.SingleOrDefault(ce => ce.Id.ToString() == attendance.CommunityEntityId);
                if (communityEntity == null)
                    continue;

                attendance.CommunityEntityType = communityEntity.Type;
            }

            //map attendances that already exist (and need to be updated)
            foreach (var attendance in attendances)
            {
                var existingAttendance = existingAttendances.SingleOrDefault(a => a.CommunityEntityId == attendance.CommunityEntityId && a.UserId == attendance.UserId && a.UserEmail == attendance.UserEmail);
                if (existingAttendance != null)
                {
                    attendance.Id = existingAttendance.Id;
                    attendance.Status = existingAttendance.Status;
                }
            }

            //auto accept community entity invites if user is their admin or owner
            foreach (var attendance in attendances.Where(a => a.CommunityEntityId != null))
            {
                var membership = userMemberships.SingleOrDefault(m => m.CommunityEntityId == attendance.CommunityEntityId && (m.AccessLevel == AccessLevel.Admin || m.AccessLevel == AccessLevel.Owner));
                if (membership != null && attendance.Status == AttendanceStatus.Pending)
                    attendance.Status = AttendanceStatus.Yes;
            }

            //load users from db
            var users = _userRepository.Get(attendances.Where(a => !string.IsNullOrEmpty(a.UserId)).Select(a => a.UserId).ToList());
            foreach (var attendance in attendances)
            {
                var attendanceUser = users.SingleOrDefault(u => u.Id.ToString() == attendance.UserId);
                attendance.User = attendanceUser;
            }

            var userAttendances = attendances.Where(a => a.User != null || !string.IsNullOrEmpty(a.UserEmail));
            await _calendarService.InviteUserAsync(externalCalendarId, ev.ExternalId, userAttendances.ToDictionary(u => u.User?.Email ?? u.UserEmail, u => u.Status));

            var toCreate = attendances.Where(a => a.Id == ObjectId.Empty).ToList();
            var toUpdate = attendances.Where(a => a.Id != ObjectId.Empty).ToList();

            var res = new List<string>();
            if (upsert && toUpdate.Any())
                await _eventAttendanceRepository.UpdateAsync(toUpdate);

            if (toCreate.Any())
                res = await _eventAttendanceRepository.CreateAsync(toCreate);

            //raise notifications
            if (user != null)
                await _notificationService.NotifyEventInviteCreatedAsync(ev, evCommunityEntity, user, toCreate.Where(a => !string.IsNullOrEmpty(a.UserId) && a.UserId != a.CreatedByUserId).ToList());
            await _notificationFacade.NotifyInvitedAsync(toCreate.Where(a => !string.IsNullOrEmpty(a.UserId) && a.UserId != a.CreatedByUserId).ToList());

            return res;
        }
        public List<EventAttendance> ListAttendances(string eventId, AttendanceAccessLevel? level, AttendanceStatus? status, AttendanceType? type, List<string> labels, List<CommunityEntityType> communityEntityTypes, string search, int page, int pageSize)
        {
            var attendances = _eventAttendanceRepository.List(a => a.EventId == eventId &&
            (level == null || a.AccessLevel == level) &&
            (status == null || status == a.Status) &&
            (type == null || (type == AttendanceType.User && !string.IsNullOrEmpty(a.UserId)) || (type == AttendanceType.Email && !string.IsNullOrEmpty(a.UserEmail)) || (type == AttendanceType.CommunityEntity && !string.IsNullOrEmpty(a.CommunityEntityId))) &&
            (communityEntityTypes == null || !communityEntityTypes.Any() || (a.CommunityEntityType != null && communityEntityTypes.Contains(a.CommunityEntityType.Value))) &&
            (labels == null || !labels.Any() || (a.Labels != null && a.Labels.Any(l1 => labels.Any(l2 => l1 == l2)))) &&
            !a.Deleted);
            EnrichEventAttendanceData(attendances);

            return GetFilteredAttendances(attendances, search, page, pageSize)
                .OrderByDescending(ea => ea.AccessLevel)
                .ThenByDescending(ea => ea.Labels?.FirstOrDefault())
                .ThenBy(ea => GetStatusOrder(ea.Status))
                .ThenBy(ea => ea.User?.FullName ?? ea.UserEmail)
                .ToList();
        }

        private int GetStatusOrder(AttendanceStatus status)
        {
            switch (status)
            {
                case AttendanceStatus.Yes:
                    return 0;
                case AttendanceStatus.Pending:
                    return 1;
                default:
                    return 2;
            }
        }

        public long CountOrganizers(string eventId)
        {
            return _eventAttendanceRepository.Count(a => a.EventId == eventId && a.AccessLevel == AttendanceAccessLevel.Admin);
        }

        public EventAttendance GetAttendance(string attendanceId)
        {
            return _eventAttendanceRepository.Get(attendanceId);
        }

        public List<EventAttendance> GetAttendances(List<string> attendanceId)
        {
            return _eventAttendanceRepository.Get(attendanceId);
        }

        public List<EventAttendance> GetAttendancesForEvent(string eventId)
        {
            return _eventAttendanceRepository.List(a => a.EventId == eventId && !a.Deleted);
        }

        public List<EventAttendance> GetAttendancesForUser(string userId)
        {
            return _eventAttendanceRepository.List(a => a.UserId == userId && !a.Deleted);
        }

        public EventAttendance GetAttendanceForEventAndInvitee(EventAttendance attendance)
        {
            var user = _userRepository.Get(u => u.Email == attendance.UserEmail);
            if (user != null)
            {
                attendance.UserEmail = null;
                attendance.UserId = user.Id.ToString();
            };

            return _eventAttendanceRepository.Get(a => a.EventId == attendance.EventId && a.UserEmail == attendance.UserEmail && a.UserId == attendance.UserId && a.CommunityEntityId == attendance.CommunityEntityId && !a.Deleted);
        }

        public async Task UpdateAsync(EventAttendance a)
        {
            await _eventAttendanceRepository.UpdateAsync(a);
        }

        public async Task UpdateAsync(List<EventAttendance> attendances)
        {
            foreach (var a in attendances)
            {
                await _eventAttendanceRepository.UpdateAsync(a);
            }
        }

        public async Task AcceptAttendanceAsync(EventAttendance a)
        {
            var ev = _eventRepository.Get(a.EventId);
            var externalCalendarId = await _calendarService.GetJoglCalendarAsync();

            if (!string.IsNullOrEmpty(a.UserId))
            {
                var user = _userRepository.Get(a.UserId);
                await _calendarService.UpdateInvitationStatus(externalCalendarId, ev.ExternalId, user.Email, Data.AttendanceStatus.Yes);
                await _notificationService.NotifyEventInviteWithdrawAsync(a);
            }
            else if (!string.IsNullOrEmpty(a.UserEmail))
            {
                await _calendarService.UpdateInvitationStatus(externalCalendarId, ev.ExternalId, a.UserEmail, Data.AttendanceStatus.Yes);
            }

            a.Status = AttendanceStatus.Yes;
            await _eventAttendanceRepository.UpdateAsync(a);
        }

        public async Task RejectAttendanceAsync(EventAttendance a)
        {
            var ev = _eventRepository.Get(a.EventId);
            var externalCalendarId = await _calendarService.GetJoglCalendarAsync();

            if (!string.IsNullOrEmpty(a.UserId))
            {
                var user = _userRepository.Get(a.UserId);
                await _calendarService.UpdateInvitationStatus(externalCalendarId, ev.ExternalId, user.Email, Data.AttendanceStatus.No);
                await _notificationService.NotifyEventInviteWithdrawAsync(a);
            }
            else if (!string.IsNullOrEmpty(a.UserEmail))
            {
                await _calendarService.UpdateInvitationStatus(externalCalendarId, ev.ExternalId, a.UserEmail, Data.AttendanceStatus.No);
            }

            a.Status = AttendanceStatus.No;
            await _eventAttendanceRepository.UpdateAsync(a);
        }

        public async Task DeleteAttendanceAsync(string attendanceId)
        {
            var a = _eventAttendanceRepository.Get(attendanceId);
            var ev = _eventRepository.Get(a.EventId);
            var externalCalendarId = await _calendarService.GetJoglCalendarAsync();

            if (!string.IsNullOrEmpty(a.UserId))
            {
                var user = _userRepository.Get(a.UserId);
                await _calendarService.UninviteUserAsync(externalCalendarId, ev.ExternalId, user.Email);
                await _notificationService.NotifyEventInviteWithdrawAsync(a);
            }
            else if (!string.IsNullOrEmpty(a.UserEmail))
            {
                await _calendarService.UninviteUserAsync(externalCalendarId, ev.ExternalId, a.UserEmail);
            }

            await _eventAttendanceRepository.DeleteAsync(attendanceId);
        }

        public async Task DeleteAttendancesAsync(List<EventAttendance> attendances)
        {
            if (!attendances.Any())
                return;

            var attendance = attendances.First();
            var existingAttendances = _eventAttendanceRepository.List(a => a.EventId == attendance.EventId && !a.Deleted);
            var ev = _eventRepository.Get(attendance.EventId);
            var communityEntity = _communityEntityService.Get(ev.CommunityEntityId);

            var usersEmails = _userRepository.Get(attendances.Where(a => !string.IsNullOrEmpty(a.UserId)).Select(a => a.UserId).ToList()).Select(u => u.Email);
            var directInviteEmails = attendances.Where(a => !string.IsNullOrEmpty(a.UserEmail)).Select(a => a.UserEmail);
            var emails = usersEmails.Union(directInviteEmails).Distinct().ToList();

            var externalCalendarId = await _calendarService.GetJoglCalendarAsync();
            await _calendarService.UninviteUserAsync(externalCalendarId, ev.ExternalId, emails);
            await _eventAttendanceRepository.DeleteAsync(attendances.Select(a => a.Id.ToString()).ToList());
            await _notificationService.NotifyEventInvitesWithdrawAsync(attendances.Where(a => !string.IsNullOrEmpty(a.UserId)));
        }

        public async Task SendMessageToUsersAsync(string eventId, List<string> userIds, string subject, string message, string url)
        {
            var ev = _eventRepository.Get(eventId);
            var users = _userRepository.Get(userIds);
            var attendees = _eventAttendanceRepository.List(a => a.EventId == eventId && !a.Deleted);
            var filteredUsers = users.Where(u => attendees.Any(a => a.UserId == u.Id.ToString()));

            await _emailService.SendEmailAsync(filteredUsers.ToDictionary(u => u.Email, u => (object)new
            {
                first_name = u.FirstName,
                text = message,
                subject = subject,
                event_title = ev.Title,
                event_url = url,
            }), EmailTemplate.EventMessage);
        }

        public List<CommunityEntity> ListCommunityEntitiesForNodeEvents(string nodeId, string currentUserId, List<CommunityEntityType> types, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            return ListCommunityEntitiesEvents(entityIds, currentUserId, types, currentUser, tags, from, to, search, page, pageSize);
        }

        public List<CommunityEntity> ListCommunityEntitiesForOrgEvents(string organizationId, string currentUserId, List<CommunityEntityType> types, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize)
        {
            var entityIds = GetCommunityEntityIdsForOrg(organizationId);
            return ListCommunityEntitiesEvents(entityIds, currentUserId, types, currentUser, tags, from, to, search, page, pageSize);
        }

        private List<CommunityEntity> ListCommunityEntitiesEvents(IEnumerable<string> entityIds, string currentUserId, List<CommunityEntityType> types, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize)
        {
            var events = _eventRepository.SearchList(e => entityIds.Contains(e.CommunityEntityId) && (from == null || e.Start > from) && (to == null || e.Start < to) && !e.Deleted, search);
            var eventAttendances = _eventAttendanceRepository.List(a => events.Select(e => e.Id.ToString()).ToList().Contains(a.EventId) && !a.Deleted);
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);

            if (currentUser)
                events = events.Where(e => eventAttendances.Any(ea => ea.EventId == e.Id.ToString() && ea.UserId == currentUserId && ea.Status == AttendanceStatus.Yes)).ToList();

            var filteredEvents = GetFilteredEvents(events, eventAttendances, currentUserMemberships, currentUserId, tags ?? new List<EventTag>());
            EnrichEventData(filteredEvents, eventAttendances, currentUserMemberships, currentUserId);

            return GetPage(filteredEvents.Select(e => e.CommunityEntity).DistinctBy(e => e.Id), page, pageSize);
        }

        protected void EnrichEventData(IEnumerable<Event> events, IEnumerable<EventAttendance> eventAttendances, IEnumerable<Membership> currentUserMemberships, string currentUserId)
        {
            var communityEntities = _communityEntityService.List(events.Select(e => e.CommunityEntityId).Distinct());
            var contentEntities = _contentEntityRepository.List(ce => events.Any(d => d.Id.ToString() == ce.FeedId) && !ce.Deleted);
            var eventIds = events.Select(e => e.Id.ToString());
            var userFeedRecords = _userFeedRecordRepository.List(ufr => ufr.UserId == currentUserId && eventIds.Contains(ufr.FeedId) && !ufr.Deleted);
            var userContentEntityRecords = _userContentEntityRecordRepository.List(ucer => ucer.UserId == currentUserId && eventIds.Contains(ucer.FeedId) && !ucer.Deleted);
            var mentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.Unread && eventIds.Contains(m.OriginFeedId) && !m.Deleted);
            foreach (var ev in events)
            {
                var feedRecord = userFeedRecords.SingleOrDefault(ufr => ufr.FeedId == ev.Id.ToString());

                ev.CommunityEntity = communityEntities.SingleOrDefault(e => e.Id.ToString() == ev.CommunityEntityId);
                ev.AttendeeCount = eventAttendances.Count(a => a.EventId == ev.Id.ToString() && a.Status == AttendanceStatus.Yes);
                ev.InviteeCount = eventAttendances.Count(a => a.EventId == ev.Id.ToString());

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    ev.UserAttendance = eventAttendances.FirstOrDefault(a => a.EventId == ev.Id.ToString() && a.UserId == currentUserId);
                    if (ev.UserAttendance != null)
                        EnrichEntityWithCreatorData(ev.UserAttendance);
                }
                ev.PostCount = contentEntities.Count(ce => ce.FeedId == ev.Id.ToString());
                ev.NewPostCount = contentEntities.Count(ce => ce.FeedId == ev.Id.ToString() && ce.CreatedUTC > (feedRecord?.LastReadUTC ?? DateTime.MaxValue));
                ev.NewMentionCount = mentions.Count(m => m.OriginFeedId == ev.Id.ToString());
                ev.NewThreadActivityCount = contentEntities.Count(ce => ce.FeedId == ev.Id.ToString() && ce.LastActivityUTC > (userContentEntityRecords.SingleOrDefault(ucer => ucer.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue));
                ev.IsNew = feedRecord == null;
            }

            EnrichEventsWithPermissions(events, eventAttendances, currentUserMemberships, currentUserId);
            EnrichEntitiesWithCreatorData(events);
        }

        protected void EnrichEventAttendanceData(IEnumerable<EventAttendance> eventAttendances)
        {
            var users = _userRepository.Get(eventAttendances.Where(a => !string.IsNullOrEmpty(a.UserId)).Select(d => d.UserId).Distinct().ToList());
            var communityEntityIdsInvites = eventAttendances.Where(a => !string.IsNullOrEmpty(a.CommunityEntityId)).Select(d => d.CommunityEntityId);
            var communityEntityIdsOrigins = eventAttendances.Where(a => !string.IsNullOrEmpty(a.OriginCommunityEntityId)).Select(d => d.OriginCommunityEntityId);
            var communityEntityIds = communityEntityIdsInvites.Concat(communityEntityIdsOrigins).Distinct().ToList();
            var communityEntities = _communityEntityService.List(communityEntityIds);

            var memberships = _membershipRepository.ListForCommunityEntities(communityEntities.Select(c => c.Id.ToString()));

            foreach (var a in eventAttendances)
            {
                a.CommunityEntity = communityEntities.SingleOrDefault(e => e.Id.ToString() == a.CommunityEntityId);
                a.OriginCommunityEntity = communityEntities.SingleOrDefault(e => e.Id.ToString() == a.OriginCommunityEntityId);
                a.User = users.SingleOrDefault(u => u.Id.ToString() == a.UserId);

                if (a.CommunityEntity == null)
                    continue;

                a.CommunityEntity.MemberCount = memberships.Count(m => m.CommunityEntityId == a.CommunityEntityId);
                a.CommunityEntity.ParticipantCount = memberships.Count(m => m.CommunityEntityId == a.CommunityEntityId && eventAttendances.Any(a => a.UserId == m.UserId));
            }
        }
        private class EventAttendanceEqualityComparer : IEqualityComparer<EventAttendance>
        {
            // Constructor
            //public EventAttendanceEqualityComparer(Expression<Func<T, TKey>> getKey)
            //        {
            //            _KeyExpr = getKey;
            //            _CompiledFunc = _KeyExpr.Compile();
            //        }

            //        public int Compare(T obj1, T obj2)
            //        {
            //            return Comparer<TKey>.Default.Compare(_CompiledFunc(obj1), _CompiledFunc(obj2));
            //        }

            //        public bool Equals(T obj1, T obj2)
            //        {
            //            return EqualityComparer<TKey>.Default.Equals(_CompiledFunc(obj1), _CompiledFunc(obj2));
            //        }

            //        public int GetHashCode(T obj)
            //        {
            //            return EqualityComparer<TKey>.Default.GetHashCode(_CompiledFunc(obj));
            //        }
            public bool Equals(EventAttendance? x, EventAttendance? y)
            {
                return string.Equals(x.EventId, y.EventId, StringComparison.CurrentCultureIgnoreCase)
                     && string.Equals(x.UserEmail, y.UserEmail, StringComparison.CurrentCultureIgnoreCase)
                     && string.Equals(x.UserId, y.UserId, StringComparison.CurrentCultureIgnoreCase)
                     && string.Equals(x.CommunityEntityId, y.CommunityEntityId, StringComparison.CurrentCultureIgnoreCase);
            }

            public int GetHashCode([DisallowNull] EventAttendance obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}