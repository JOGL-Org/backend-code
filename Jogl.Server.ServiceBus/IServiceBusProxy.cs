namespace Jogl.Server.ServiceBus
{
    public interface IServiceBusProxy
    {
        Task SendAsync<T>(T payload, string queueName);
        Task SendAsync<T>(IEnumerable<T> payload, string queueName);
        Task SubscribeAsync<T>(string queueName, string subscriptionName, Func<T, Task> onMessage);
    }
}