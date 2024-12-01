using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IEventService
    {
        Task<string> CreateAsync(Event e);
        ListPage<Event> List(string currentUserId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long Count(string userId, string search);
        List<Event> ListForEntity(string entityId, string currentUserId, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        ListPage<Event> ListForNode(string nodeId, string currentUserId, List<string> communityEntityIds, FeedEntityFilter? filter, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long CountForNode(string userId, string nodeId, string search);
        List<Event> ListForOrganization(string organizationId, string currentUserId, List<CommunityEntityType> types, List<string> communityEntityIds, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        List<Event> ListForUser(string userId, string currentUserId, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        Event Get(string eventId);
        Event Get(string eventId, string currentUserId);
        Task UpdateAsync(Event e);
        Task DeleteAsync(string eventId);

        Task<string> CreateAttendanceAsync(EventAttendance attendance);
        Task<List<string>> UpsertAttendancesAsync(List<EventAttendance> attendances, string currentUserId, bool upsert);

        List<EventAttendance> ListAttendances(string eventId, AttendanceAccessLevel? level, AttendanceStatus? status, AttendanceType? type, List<string> labels, List<CommunityEntityType> communityEntityTypes, string search, int page, int pageSize);
        long CountOrganizers(string eventId);
        EventAttendance GetAttendance(string attendanceId);
        List<EventAttendance> GetAttendances(List<string> attendanceId);
        List<EventAttendance> GetAttendancesForEvent(string eventId);
        List<EventAttendance> GetAttendancesForUser(string userId);

        EventAttendance GetAttendanceForEventAndInvitee(EventAttendance attendance);
        Task UpdateAsync(EventAttendance a);
        Task UpdateAsync(List<EventAttendance> attendances);
        Task AcceptAttendanceAsync(EventAttendance a);
        Task RejectAttendanceAsync(EventAttendance a);
        Task DeleteAttendanceAsync(string attendanceId);
        Task DeleteAttendancesAsync(List<EventAttendance> attendances);

        Task SendMessageToUsersAsync(string cfpId, List<string> userIds, string subject, string message, string url);

        List<CommunityEntity> ListCommunityEntitiesForNodeEvents(string nodeId, string currentUserId, List<CommunityEntityType> types, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize);
        List<CommunityEntity> ListCommunityEntitiesForOrgEvents(string organizationId, string currentUserId, List<CommunityEntityType> types, bool currentUser, List<EventTag> tags, DateTime? from, DateTime? to, string search, int page, int pageSize);
    }
}