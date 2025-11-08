using Jogl.Server.Cryptography.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Verification.Extensions
{
    public static class Extensions
    {
        public static void AddVerification(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IUserVerificationService, UserVerificationService>();

            serviceCollection.AddCryptography();
        }
    }
}