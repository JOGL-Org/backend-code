using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Email.Extensions
{
    public static class Extensions
    {
        public static void AddEmail(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IEmailService, SendGridEmailService>();
        }
    }
}