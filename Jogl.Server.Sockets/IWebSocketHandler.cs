using System.Net.WebSockets;

namespace Jogl.Server.Sockets
{
    public interface IWebSocketHandler
    {
        Task OnConnected(WebSocket socket);

        Task OnDisconnected(WebSocket socket);

        Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}