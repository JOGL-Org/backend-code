using Azure.Messaging.ServiceBus;
using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Jogl.Server.ServiceBus
{
    public class AzureServiceBusProxy : IServiceBusProxy
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureServiceBusProxy> _logger;
        public AzureServiceBusProxy(IConfiguration configuration, ILogger<AzureServiceBusProxy> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync<T>(T payload, string queueName) where T : Entity
        {
            var client = new ServiceBusClient(_configuration["Azure:ServiceBus:ConnectionString"]);
            var sender = client.CreateSender(queueName);

            await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(payload)));
        }

        public async Task SubscribeAsync<T>(string queueName, string subscriptionName, Func<T, Task> onMessage) where T : Entity
        {
            var client = new ServiceBusClient(_configuration["Azure:ServiceBus:ConnectionString"]);
            var processor = client.CreateProcessor(queueName, subscriptionName);
            processor.ProcessMessageAsync += async (ProcessMessageEventArgs e) =>
            {
                var payload = JsonSerializer.Deserialize<T>(e.Message.Body.ToString());
                await onMessage(payload);
                await e.CompleteMessageAsync(e.Message);
            };

            processor.ProcessErrorAsync += async (ProcessErrorEventArgs e) =>
            {
                _logger.LogError(e.Exception.Message);
            };

            await processor.StartProcessingAsync();
        }
    }
}