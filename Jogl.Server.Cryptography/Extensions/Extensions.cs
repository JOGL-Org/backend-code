using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Cryptography.Extensions
{
    public static class Extensions
    {
        public static void AddCryptography(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICryptographyService, CryptographyService>();
        }
    }
}