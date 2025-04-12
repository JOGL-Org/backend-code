using Jogl.Server.AI.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.AI.Agent.Extensions
{
    public static class RepositoryExtensions
    {
        public static void AddAIAgent(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAgent, UserSearchAgent>();
            serviceCollection.AddAI();
        }
    }
}