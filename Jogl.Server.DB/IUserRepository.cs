using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IUserRepository : IRepository<User>
    {
        [Obsolete]
        User GetForEmail(string email);

        Task SetVerifiedAsync(string userId);
        Task SetPasswordAsync(string userId, string passwordHash, byte[] salt);
    }
}