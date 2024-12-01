using Jogl.Server.DB.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jogl.Server.DB.Extensions
{
    public static class InitializerExtensions
    {
        public static void AddInitialization(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IInitializer, Initializer>();

            var assembly = typeof(Initializer).Assembly;
            var initializerImplementations = assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IRepository)) && !t.IsAbstract).ToList();
            foreach (var initializerImplementation in initializerImplementations)
            {
                serviceCollection.AddTransient(typeof(IRepository), initializerImplementation);
            }
        }

        public static async Task InitializeDBAsync(this IHost host)
        {
            var initializer = host.Services.CreateScope().ServiceProvider.GetService<IInitializer>();
            await initializer.InitializeAsync();
        }
    }
}