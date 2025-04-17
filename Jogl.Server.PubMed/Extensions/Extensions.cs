using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.PubMed.Extensions
{
    public static class Extensions
    {
        public static void AddPubmed(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IPubMedFacade, PubMedFacade>();
        }
    }
}