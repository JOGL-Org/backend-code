using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.URL.Extensions
{
    public static class Extensions
    {
        public static void AddUrls(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IUrlService, UrlService>();
        }
    }
}