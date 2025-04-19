using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlackNet.AspNetCore;

namespace Jogl.Server.Slack.Extensions
{
    public static class Extensions
    {
        public static void AddSlack(this IServiceCollection serviceCollection, IConfiguration configuration, Action<AspNetSlackServiceConfiguration>? configure = default)
        {
            serviceCollection.AddSlackNet(c =>
            {
                c.UseAppLevelToken(configuration["Slack:AppLevelToken"]);
                c.UseSigningSecret(configuration["Slack:SigningSecret"]);
                if (configure != null)
                    configure(c);
            });

            serviceCollection.AddSingleton<ISlackService, SlackService>();
        }

        public static IApplicationBuilder UseSlack(this IApplicationBuilder app, Action<SlackEndpointConfiguration>? configure = null)
        {
            app.UseSlackNet(configure);
            return app;
        }
    }
}