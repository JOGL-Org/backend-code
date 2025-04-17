using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Search.Extensions
{
    public static class Extensions
    {
        public static void AddSearch(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISearchService, AzureSearchService>();
        }
    }
}