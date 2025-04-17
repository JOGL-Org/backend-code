using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IUserService
    {
        Task<string> CreateAsync(User user, string password = "");
        User Get(string userId);
        User GetDetail(string userId, string currentUserId);
        User GetForEmail(string email, bool includeDeleted = false);
        User GetForWallet(string wallet, bool includeDeleted = false);
        User GetForUsername(string username, bool includeDeleted = false);
        ListPage<User> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending);
        long Count(string userId, string search);
        List<User> ListForEntity(string userId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending);
        ListPage<User> ListForNode(string userId, string nodeId, List<string> communityEntityIds, string search, int page, int pageSize, SortKey sortKey, bool sortAscending);
        long CountForNode(string userId, string nodeId, string search);
        List<User> ListEcosystem(string currentUserId, string entityId, string search, int page, int pageSize);
        List<User> AutocompleteForEntity(string entityId, string search, int page, int pageSize);
        List<User> Autocomplete(string search, int page, int pageSize);

        Task UpdateAsync(User user);
        Task UpdateOnboardingStatusAsync(User user);
        Task DeleteAsync(User user);

        Task ResetPasswordAsync(string email, string url);
        Task<bool> ResetPasswordConfirmAsync(string email, string code, string newPassword);
        Task StartOneTimeLoginAsync(string email);
        Task<string> GetOnetimeLoginCodeAsync(string email);
        Task<bool> VerifyOneTimeLoginAsync(string email, string code);
        Task SetPasswordAsync(string userId, string password);
        Task SetActiveAsync(User user);
        Task SetArchivedAsync(User user);

        Task SendMessageAsync(string userIdFrom, string userIdTo, string appUrl, string subject, string text);

        UserFollowing GetFollowing(string userIdFrom, string userIdTo);
        Task<string> CreateFollowingAsync(UserFollowing following);
        Task DeleteFollowingAsync(string followingId);
        List<User> GetFollowed(string followerId, string currentUserId, string search, int page, int pageSize, bool loadDetails = false);
        List<User> GetFollowers(string followedId, string currentUserId, string search, int page, int pageSize, bool loadDetails = false);

        Task CreateSkillAsync(TextValue skill);
        List<TextValue> GetSkills(string search, int page, int pageSize);
        TextValue GetSkill(string value);

        string GetUniqueUsername(string firstName, string lastName);

        Task CreateWaitlistRecordAsync(WaitlistRecord record);
        Task SendContactEmailAsync(UserContact contact);

        Task UpsertPushNotificationTokenAsync(string token, string userId);

        List<CommunityEntity> ListCommunityEntitiesForNodeUsers(string currentUserId, string nodeId, string search);

        Task<string> ImportUserAsync(string firstName, string lastName, string email);
    }
}