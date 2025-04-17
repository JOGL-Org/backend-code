using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Auth.Extensions
{
    public static class Extensions
    {
        public static void AddAuth(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAuthService, AuthService>();
            serviceCollection.AddTransient<IAuthChallengeService, AuthChallengeService>();
            serviceCollection.AddMemoryCache();
        }
    }
}