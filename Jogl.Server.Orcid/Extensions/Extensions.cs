using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Orcid.Extensions
{
    public static class Extensions
    {
        public static void AddOrcid(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IOrcidFacade, OrcidFacade>();
        }
    }
}