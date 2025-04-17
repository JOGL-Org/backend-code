using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Arxiv.Extensions
{
    public static class Extensions
    {
        public static void AddArxiv(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IArxivFacade, ArxivFacade>();
        }
    }
}