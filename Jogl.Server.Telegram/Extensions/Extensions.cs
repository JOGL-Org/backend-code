using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Telegram.Extensions
{
    public static class Extensions
    {
        public static void AddTelegram(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ITelegramVerifier, TelegramVerifier>();
        }
    }
}