using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Jogl.Server.Sockets
{
    public interface IWebSocketConnectionManager
    {
        WebSocket GetSocketById(string id);
        ConcurrentDictionary<string, WebSocket> GetAll();
        string GetId(WebSocket socket);
        void AddSocket(WebSocket socket);
        Task RemoveSocketAsync(string id);
    }
}