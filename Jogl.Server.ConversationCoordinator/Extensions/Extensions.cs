using Jogl.Server.Arxiv.Extensions;
using Jogl.Server.Auth.Extensions;
using Jogl.Server.Email.Extensions;
using Jogl.Server.Events.Extensions;
using Jogl.Server.GitHub.Extensions;
using Jogl.Server.Orcid.Extensions;
using Jogl.Server.SemanticScholar.Extensions;
using Jogl.Server.HuggingFace.Extensions;
using Jogl.Server.Notifications.Extensions;
using Jogl.Server.PubMed.Extensions;
using Jogl.Server.DB.Extensions;
using Jogl.Server.Storage.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Jogl.Server.URL.Extensions;
using Jogl.Server.Business;
using Jogl.Server.ConversationCoordinator.Services;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Slack.Extensions;

namespace Jogl.Server.ConversationCoordinator.Extensions
{
    public static class Extensions
    {
        public static void AddConversationCoordinator(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IOutputServiceFactory, OutputServiceFactory>();
            serviceCollection.AddSingleton<ISlackOutputService, SlackOutputService>();
            serviceCollection.AddSlack(configuration);
        }
    }
}