using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IUserRepository : IRepository<User>
    {
        [Obsolete]
        User GetForEmail(string email);

        Task SetStatusAsync(string userId, UserStatus status);
        Task SetPasswordAsync(string userId, string passwordHash, byte[] salt);

        IFluentQuery<User> QueryWithMembershipData(string searchValue, List<string> communityEntityIds);
    }
}