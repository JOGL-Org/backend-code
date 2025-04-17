using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.ServiceBus.Extensions
{
    public static class Extensions
    {
        public static void AddServiceBus(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IServiceBusProxy, AzureServiceBusProxy>();
        }
    }
}