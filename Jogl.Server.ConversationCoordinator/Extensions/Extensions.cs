using Microsoft.Extensions.DependencyInjection;
using Jogl.Server.ConversationCoordinator.Services;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Slack.Extensions;
using Jogl.Server.WhatsApp.Extensions;

namespace Jogl.Server.ConversationCoordinator.Extensions
{
    public static class Extensions
    {
        public static void AddConversationCoordinator(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IOutputServiceFactory, OutputServiceFactory>();
            serviceCollection.AddSingleton<ISlackOutputService, SlackOutputService>();
            serviceCollection.AddSingleton<IWhatsAppOutputService, WhatsAppOutputService>();
            serviceCollection.AddSingleton<IJOGLOutputService, JOGLOutputService>();
            serviceCollection.AddSlack(configuration);
            serviceCollection.AddWhatsApp(configuration);
        }
    }
}