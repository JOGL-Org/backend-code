using System.Net.WebSockets;
using System.Text;

namespace Jogl.Server.Sockets
{
    public abstract class WebSocketHandler : IWebSocketHandler
    {
        protected readonly IWebSocketConnectionManager _webSocketConnectionManager;

        public WebSocketHandler(IWebSocketConnectionManager webSocketConnectionManager)
        {
            _webSocketConnectionManager = webSocketConnectionManager;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            _webSocketConnectionManager.AddSocket(socket);
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await _webSocketConnectionManager.RemoveSocketAsync(_webSocketConnectionManager.GetId(socket));
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                                                                  offset: 0,
                                                                  count: message.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string socketId, string message)
        {
            var socket = _webSocketConnectionManager.GetSocketById(socketId);
            if (socket != null)
                await SendMessageAsync(socket, message);
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in _webSocketConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    await SendMessageAsync(pair.Value, message);
                }
            }
        }

        public async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var socketId = _webSocketConnectionManager.GetId(socket);
            if (!result.EndOfMessage)
                throw new NotImplementedException();

            await ProcessMessageAsync(socketId, message);
        }

        protected abstract Task ProcessMessageAsync(string socketId, string message);
    }
}