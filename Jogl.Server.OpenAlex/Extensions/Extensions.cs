using Jogl.Server.OpenAlex;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.PubMed.Extensions
{
    public static class Extensions
    {
        public static void AddOpenAlex(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IOpenAlexFacade, OpenAlexFacade>();
        }
    }
}