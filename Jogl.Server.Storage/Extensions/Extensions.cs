using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Storage.Extensions
{
    public static class Extensions
    {
        public static void AddStorage(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IStorageService, BlobStorageService>();
        }
    }
}