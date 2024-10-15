using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IPushNotificationTokenRepository : IRepository<PushNotificationToken>
    {
        Task UpsertTokenAsync(string userId, string token, DateTime date);
    }
}