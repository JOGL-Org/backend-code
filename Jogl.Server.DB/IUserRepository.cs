using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IUserRepository : IRepository<User>
    {
        [Obsolete]
        User GetForEmail(string email);

        Task SetStatusAsync(string userId, UserStatus status);
        Task SetOnboardingStatusAsync(User user);
        Task SetPasswordAsync(string userId, string passwordHash, byte[] salt);

        IRepositoryQuery<User> QueryWithMembershipData(string searchValue, List<string> communityEntityIds);
    }
}