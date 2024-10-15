using Jogl.Server.Business;
using Jogl.Server.DB;
using Jogl.Server.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Mailer
{
    public class Check
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventAttendanceRepository _eventAttendanceRepository;
        private readonly IFeedEntityService _feedEntityService;
        private readonly ICalendarService _calendarService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public Check(IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IFeedEntityService feedEntityService, ICalendarService calendarService, IUserRepository userRepository, ILoggerFactory loggerFactory)
        {
            _eventRepository = eventRepository;
            _eventAttendanceRepository = eventAttendanceRepository;
            _feedEntityService = feedEntityService;
            _calendarService = calendarService;
            _userRepository = userRepository;
            _logger = loggerFactory.CreateLogger<Check>();
        }

        [Function("Check")]
        public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
        {
            var futureEvents = _eventRepository.List(e => (DateTime.UtcNow.AddDays(-1) <= e.Start || DateTime.UtcNow.AddDays(-1) <= e.End) && !e.Deleted);
            _feedEntityService.PopulateFeedEntities(futureEvents);
            foreach (var ev in futureEvents)
            {
                var externalCalendarId = await _calendarService.GetJoglCalendarAsync();
                var attendees = await _calendarService.ListAttendeesAsync(externalCalendarId, ev.ExternalId);
                var joglAttendees = _eventAttendanceRepository.List(ea => ea.EventId == ev.Id.ToString() && !ea.Deleted);
                foreach (var ea in joglAttendees)
                {
                    if (!string.IsNullOrEmpty(ea.UserId))
                    {
                        var user = _userRepository.Get(ea.UserId);
                        ea.UserEmail = user?.Email ?? ea.UserEmail;
                    }

                    if (string.IsNullOrEmpty(ea.UserEmail))
                        continue;

                    var attendee = attendees.SingleOrDefault(a => a.Email == ea.UserEmail);
                    if (attendee == null)
                        continue;

                    if (attendee.Status != ea.Status)
                    {
                        ea.Status = attendee.Status;
                        await _eventAttendanceRepository.UpdateAsync(ea);
                        _logger.LogInformation($"Updated the attendance status on event {ea.EventId} for {attendee.Email} to {ea.Status}");
                    }
                }
            }
        }
    }
}
