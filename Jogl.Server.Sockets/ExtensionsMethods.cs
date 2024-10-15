using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Sockets
{
    public static class ExtensionsMethods
    {
        public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app,
                                                             PathString path,
                                                             WebSocketHandler handler)
        {
            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketMiddleware>(handler));
        }

        public static IServiceCollection AddSockets<T, IT>(this IServiceCollection services) where T : WebSocketHandler, IT where IT : class
        {
            services.AddSingleton<WebSocketMiddleware>();
            services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
            services.AddSingleton<IWebSocketHandler, T>();
            services.AddSingleton<IT, T>();
            services.AddSingleton<IT>(x => (IT)x.GetService<IWebSocketHandler>());

            return services;
        }
    }
}