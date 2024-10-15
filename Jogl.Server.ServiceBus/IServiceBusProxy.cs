using Jogl.Server.Data;

namespace Jogl.Server.ServiceBus
{
    public interface IServiceBusProxy
    {
        Task SendAsync<T>(T payload, string queueName) where T : Entity;
        Task SubscribeAsync<T>(string queueName, string subscriptionName, Func<T, Task> onMessage) where T : Entity;
    }
}