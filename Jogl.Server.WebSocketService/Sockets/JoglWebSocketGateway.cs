using Jogl.Server.Sockets;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jogl.Server.WebSocketService.Sockets
{
    public class JoglWebSocketGateway : WebSocketHandler, IWebSocketGateway
    {
        private ConcurrentDictionary<string, List<string>> _subscribers = new ConcurrentDictionary<string, List<string>>();
        private JsonSerializerOptions _options;

        public JoglWebSocketGateway(IWebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },

            };
        }

        protected override async Task ProcessMessageAsync(string socketId, string message)
        {
            SocketClientMessage msg;
            try
            {
                msg = JsonSerializer.Deserialize<SocketClientMessage>(message, _options);
            }
            catch (Exception ex)
            {
                await SendMessageAsync(socketId, "Invalid message received");
                return;
            }

            switch (msg.Type)
            {
                case ClientMessageType.Subscribe:
                    _subscribers.AddOrUpdate(msg.TopicId, new List<string> { socketId }, (key, val) => val.Append(socketId).Distinct().ToList());
                    //await SendMessageAsync(socketId, $"You are now subscribed to {msg.TopicId}");
                    return;

                case ClientMessageType.Unsubscribe:
                    _subscribers.AddOrUpdate(msg.TopicId, new List<string> { }, (key, val) => val.Where(v => socketId != v).ToList());
                    //await SendMessageAsync(socketId, $"You are now unsubscribed from {msg.TopicId}");
                    return;
            }
        }

        public async Task SendMessageAsync(SocketServerMessage message)
        {
            var list = new List<string>();
            if (!_subscribers.TryGetValue(message.TopicId, out list))
                return;

            var msg = JsonSerializer.Serialize(message, _options);
            foreach (var socket in list)
            {
                await SendMessageAsync(socket, msg);
            }
        }
    }
}