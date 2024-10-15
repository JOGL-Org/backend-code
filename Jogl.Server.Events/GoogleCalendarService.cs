using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Jogl.Server.Data;
using Jogl.Server.Events.DTO;
using Jogl.Server.URL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using System.Text;
using ZoomNet;

namespace Jogl.Server.Events
{
    public class GoogleCalendarService : ICalendarService
    {
        private readonly IUrlService _urlService;
        private readonly CalendarService _calendarService;
        private readonly ResiliencePipeline _pipeline;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CalendarService> _logger;
        public GoogleCalendarService(IUrlService urlService, ResiliencePipelineProvider<string> pipelineProvider, IConfiguration configuration, ILogger<CalendarService> logger)
        {
            _urlService = urlService;
            _pipeline = pipelineProvider.GetPipeline("retry");
            _configuration = configuration;

            var credentialParameters = GetCredentialParameters();
            var cred = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(credentialParameters.ClientEmail)
               {
                   User = configuration["EventCalendar"],
                   Scopes = new[] { CalendarService.Scope.Calendar, CalendarService.Scope.CalendarEvents }
               }.FromPrivateKey(credentialParameters.PrivateKey));

            _calendarService = new CalendarService(new BaseClientService.Initializer { HttpClientInitializer = cred });
            _configuration = configuration;
            _logger = logger;
        }

        private JsonCredentialParameters GetCredentialParameters()
        {
            using (var file = new StringReader(_configuration["Google:Events"]))
            {
                var serializer = new JsonSerializer();
                return (JsonCredentialParameters)serializer.Deserialize(file, typeof(JsonCredentialParameters));
            }
        }

        public async Task<string> CreateCalendarAsync(string title)
        {
            var req = _calendarService.Calendars.Insert(new Calendar { Summary = title, Description = title });
            var res = await req.ExecuteAsync();
            return res.Id;
        }

        public async Task<string> GetJoglCalendarAsync()
        {
            var req = _calendarService.Calendars.Get(_configuration["EventCalendar"]);
            var res = await req.ExecuteAsync();
            return res.Id;
        }

        public async Task<string> CreateEventAsync(string calendarId, Data.Event ev, IEnumerable<User> organizers)
        {
            var req = _calendarService.Events.Insert(new Google.Apis.Calendar.v3.Data.Event
            {
                Summary = ev.Title,
                Description = GetEventDescription(ev, organizers),
                Start = new EventDateTime { DateTimeDateTimeOffset = ev.Start, TimeZone = ev.Timezone.Value },
                End = new EventDateTime { DateTimeDateTimeOffset = ev.End, TimeZone = ev.Timezone.Value },
                Attendees = new List<EventAttendee>(),
                ConferenceData = ev.GenerateMeetLink ? new ConferenceData { CreateRequest = new CreateConferenceRequest { RequestId = Guid.NewGuid().ToString(), ConferenceSolutionKey = new ConferenceSolutionKey() { Type = "hangoutsMeet" } } } : null,
                GuestsCanSeeOtherGuests = false,
            }, calendarId);

            req.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;
            if (ev.GenerateMeetLink)
                req.ConferenceDataVersion = 1;

            var res = await _pipeline.ExecuteAsync(async token =>
            {
                return await req.ExecuteAsync();
            });

            var videoEntryPoint = res.ConferenceData?.EntryPoints?.SingleOrDefault(ep => ep.EntryPointType == "video");
            ev.GeneratedMeetingURL = videoEntryPoint?.Uri;

            if (ev.GenerateZoomLink)
            {
                var clientId = _configuration["Zoom:ClientId"];
                var clientSecret = _configuration["Zoom:ClientSecret"];
                var accountId = _configuration["Zoom:AccountId"];
                var connectionInfo = OAuthConnectionInfo.ForServerToServer(clientId, clientSecret, accountId);
                var zoomClient = new ZoomClient(connectionInfo);
                var zoomRes = await zoomClient.Meetings.CreateScheduledMeetingAsync("filip@jogl.io", ev.Title, null, ev.Start, (int)(ev.End - ev.Start).TotalMinutes, settings: new ZoomNet.Models.MeetingSettings { ApprovalType = ZoomNet.Models.ApprovalType.None, JoinBeforeHost = true });
                ev.GeneratedMeetingURL = zoomRes?.JoinUrl;
            }

            return res.Id;
        }

        public async Task UpdateEventAsync(string calendarId, Data.Event ev, IEnumerable<User> organizers, bool generateMeetLink, bool generateZoomLink)
        {
            var evRequest = _calendarService.Events.Get(calendarId, ev.ExternalId);
            var externalEv = await evRequest.ExecuteAsync();
            externalEv.Summary = ev.Title;
            externalEv.Description = GetEventDescription(ev, organizers);
            externalEv.Start = new EventDateTime { DateTimeDateTimeOffset = ev.Start, TimeZone = ev.Timezone.Value };
            externalEv.End = new EventDateTime { DateTimeDateTimeOffset = ev.End, TimeZone = ev.Timezone.Value };
            externalEv.ConferenceData = generateMeetLink ? new ConferenceData { CreateRequest = new CreateConferenceRequest { RequestId = Guid.NewGuid().ToString(), ConferenceSolutionKey = new ConferenceSolutionKey() { Type = "hangoutsMeet" } } } : null;
            externalEv.GuestsCanSeeOtherGuests = false;

            var req = _calendarService.Events.Update(externalEv, calendarId, ev.ExternalId);
            req.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;
            if (generateMeetLink)
                req.ConferenceDataVersion = 1;

            var res = await _pipeline.ExecuteAsync(async token =>
            {
                return await req.ExecuteAsync();
            });

            var videoEntryPoint = res.ConferenceData?.EntryPoints?.SingleOrDefault(ep => ep.EntryPointType == "video");
            ev.GeneratedMeetingURL = videoEntryPoint?.Uri;

            if (ev.GenerateZoomLink)
            {
                var clientId = _configuration["Zoom:ClientId"];
                var clientSecret = _configuration["Zoom:ClientSecret"];
                var accountId = _configuration["Zoom:AccountId"];
                var connectionInfo = OAuthConnectionInfo.ForServerToServer(clientId, clientSecret, accountId);
                var zoomClient = new ZoomClient(connectionInfo);
                var zoomRes = await zoomClient.Meetings.CreateScheduledMeetingAsync("filip@jogl.io", ev.Title, null, ev.Start, (int)(ev.End - ev.Start).TotalMinutes, settings: new ZoomNet.Models.MeetingSettings { ApprovalType = ZoomNet.Models.ApprovalType.None, JoinBeforeHost = true });
                ev.GeneratedMeetingURL = zoomRes?.JoinUrl;
            }
        }

        public async Task DeleteEventAsync(string calendarId, string eventId)
        {
            var req = _calendarService.Events.Delete(calendarId, eventId);
            req.SendUpdates = EventsResource.DeleteRequest.SendUpdatesEnum.None;

            await _pipeline.ExecuteAsync(async token =>
            {
                return await req.ExecuteAsync();
            });

        }

        public async Task InviteUserAsync(string calendarId, string eventId, string email, AttendanceStatus status = AttendanceStatus.Pending)
        {
            await InviteUserAsync(calendarId, eventId, new Dictionary<string, AttendanceStatus> { { email, status } });
        }

        public async Task InviteUserAsync(string calendarId, string eventId, Dictionary<string, AttendanceStatus> emails)
        {
            var evRequest = _calendarService.Events.Get(calendarId, eventId);
            var ev = await evRequest.ExecuteAsync();
            if (ev.Attendees == null)
                ev.Attendees = new List<EventAttendee>();

            var suppressExternalEmails = bool.Parse(_configuration["App:SuppressExternalEmails"]);
            foreach (var emailAndStatus in emails)
            {
                var email = emailAndStatus.Key;
                var status = emailAndStatus.Value;
                if (string.IsNullOrEmpty(email))
                    continue;

                if (suppressExternalEmails && !email.EndsWith("@jogl.io"))
                    continue;

                ev.Attendees.Add(new EventAttendee { Email = email, ResponseStatus = ParseStatus(status) });
            }

            var req = _calendarService.Events.Update(ev, calendarId, eventId);
            req.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;

            await _pipeline.ExecuteAsync(async token =>
            {
                return await req.ExecuteAsync();
            });
        }

        public async Task UninviteUserAsync(string calendarId, string eventId, string email)
        {
            var evRequest = _calendarService.Events.Get(calendarId, eventId);
            var ev = await evRequest.ExecuteAsync();
            var attendee = ev.Attendees.SingleOrDefault(a => a.Email == email);
            ev.Attendees.Remove(attendee);

            var req = _calendarService.Events.Update(ev, calendarId, eventId);
            req.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;

            await _pipeline.ExecuteAsync(async token =>
            {
                return await req.ExecuteAsync();
            });
        }

        public async Task UninviteUserAsync(string calendarId, string eventId, List<string> emails)
        {
            var evRequest = _calendarService.Events.Get(calendarId, eventId);
            var ev = await evRequest.ExecuteAsync();
            foreach (var email in emails)
            {
                if (string.IsNullOrEmpty(email))
                    continue;

                var attendee = ev.Attendees.SingleOrDefault(a => a.Email == email);
                ev.Attendees.Remove(attendee);
            }

            var req = _calendarService.Events.Update(ev, calendarId, eventId);
            req.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;

            await _pipeline.ExecuteAsync(async token =>
            {
                return await req.ExecuteAsync();
            });
        }

        public async Task UpdateInvitationStatus(string calendarId, string eventId, string email, AttendanceStatus status)
        {
            var evRequest = _calendarService.Events.Get(calendarId, eventId);
            var ev = await evRequest.ExecuteAsync();
            foreach (var attendee in ev.Attendees)
            {
                if (attendee.Email == email)
                    attendee.ResponseStatus = ParseStatus(status);
            }

            var req = _calendarService.Events.Update(ev, calendarId, eventId);
            req.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;

            await _pipeline.ExecuteAsync(async token =>
            {
                return await req.ExecuteAsync();
            });
        }

        public async Task<List<Attendee>> ListAttendeesAsync(string calendarId, string eventId)
        {
            var evRequest = _calendarService.Events.Get(calendarId, eventId);
            try
            {
                var ev = await evRequest.ExecuteAsync();
                return ev.Attendees.Select(a => new Attendee { Email = a.Email, Status = ParseStatus(a.ResponseStatus) }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to retrieve google calendar event: " + ex.ToString());
                return new List<Attendee>();
            }
        }

        private AttendanceStatus ParseStatus(string statusString)
        {
            switch (statusString)
            {
                case "needsAction":
                default:
                    return AttendanceStatus.Pending;
                case "declined":
                    return AttendanceStatus.No;
                case "tentative":
                    return AttendanceStatus.Yes; //we do not have a "maybe" status in JOGL
                case "accepted":
                    return AttendanceStatus.Yes;
            }
        }

        private string ParseStatus(AttendanceStatus status)
        {
            switch (status)
            {
                default:
                case AttendanceStatus.Pending:
                    return "needsAction";
                case AttendanceStatus.No:
                    return "declined";
                case AttendanceStatus.Yes:
                    return "accepted";
            }
        }

        public async Task<Dictionary<string, string>> ListCalendarsAsync()
        {
            var cals = await _calendarService.CalendarList.List().ExecuteAsync();
            return cals.Items.ToDictionary(i => i.Id, i => i.Summary);
        }

        public async Task<Dictionary<string, string>> ListEventsForCalendarAsync(string calendarId)
        {
            var cals = await _calendarService.Events.List(calendarId).ExecuteAsync();
            return cals.Items.ToDictionary(i => i.Id, i => i.Summary);
        }

        public async Task<string> GetEventAsync(string calendarId, string eventId)
        {
            var req = _calendarService.Events.Get(calendarId, eventId);
            var res = await req.ExecuteAsync();

            return res.Id;
        }

        public string GetEventDescription(Data.Event ev, IEnumerable<User> organizers)
        {
            var containerUrl = _urlService.GetUrl(ev.CommunityEntity);
            var eventUrl = _urlService.GetUrl(ev);

            var sb = new StringBuilder();
            sb.Append($"This event is organized in <a href=\"{containerUrl}\">{ev.CommunityEntity.Title}</a>.");
            sb.Append($"The organizers are: ");
            sb.Append(string.Join(", ", organizers.Select(u => $"<a href=\"{_urlService.GetUrl(u)}\">{u.FullName}</a>")));
            sb.Append($"<br/><br/>");
            sb.Append($"Do not reply to this invite by email and do not add a note when responding. You can ask the organizers questions on the event discussion: <a href=\"{eventUrl}\">{ev.Title}</a>.");
            if (!string.IsNullOrEmpty(ev.MeetingURL))
            {
                sb.Append($"<br/><br/>");
                sb.Append($"The organizer has provided an external videoconference link. To join, click <a href=\"{ev.MeetingURL}\">here</a>");
            }

            return sb.ToString();
        }
    }
}