using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.SemanticScholar.Extensions
{
    public static class Extensions
    {
        public static void AddSemanticScholar(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISemanticScholarFacade, SemanticScholarFacade>();
        }
    }
}