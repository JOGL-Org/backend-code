using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.WhatsApp.Extensions
{
    public static class Extensions
    {
        public static void AddWhatsApp(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IWhatsAppService, WhatsAppService>();
        }
    }
}