using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.SerpAPI.Extensions
{
    public static class Extensions
    {
        public static void AddSerpAPI(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISerpAPIFacade, SerpAPIFacade>();
        }
    }
}