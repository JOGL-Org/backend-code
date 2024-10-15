using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace Jogl.Server.Sockets
{
    public class WebSocketMiddleware : IMiddleware
    {
        private readonly IWebSocketHandler _handler;
        private readonly ILogger<WebSocketMiddleware> _logger;
        public WebSocketMiddleware(IWebSocketHandler handler, ILogger<WebSocketMiddleware> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await next(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await _handler.OnConnected(socket);

            try
            {
                await Receive(socket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        await _handler.ReceiveAsync(socket, result, buffer);
                        return;
                    }

                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _handler.OnDisconnected(socket);
                        return;
                    }

                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                await _handler.OnDisconnected(socket);
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                        cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}