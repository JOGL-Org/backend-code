namespace Jogl.Server.WebSocketService.Sockets
{
    public interface IWebSocketGateway
    {
        Task SendMessageAsync(SocketServerMessage socket);
    }
}