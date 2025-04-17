using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.HuggingFace.Extensions
{
    public static class Extensions
    {
        public static void AddHuggingFace(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IHuggingFaceFacade, HuggingFaceFacade>();
        }
    }
}