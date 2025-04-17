using Jogl.Server.ServiceBus.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Notifications.Extensions
{
    public static class Extensions
    {
        public static void AddNotifications(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<INotificationFacade, ServiceBusNotificationFacade>();

            serviceCollection.AddServiceBus();
        }
    }
}