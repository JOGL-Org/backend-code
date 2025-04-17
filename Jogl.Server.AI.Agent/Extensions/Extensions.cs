using Jogl.Server.AI.Extensions;
using Jogl.Server.Search.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.AI.Agent.Extensions
{
    public static class Extensions
    {
        public static void AddAIAgent(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAgent, UserSearchAgent>();
            serviceCollection.AddAI();
            serviceCollection.AddSearch();
        }
    }
}