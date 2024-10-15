using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.DB.Initialization
{
    public static class InitializerExtensions
    {
        public static void AddInitializers(this IServiceCollection serviceCollection)
        {
            var assembly = typeof(Initializer).Assembly;
            var initializerImplementations = assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IRepository)) && !t.IsAbstract).ToList();
            foreach (var initializerImplementation in initializerImplementations)
            {
                serviceCollection.AddSingleton(typeof(IRepository), initializerImplementation);
            }
        }
    }
}