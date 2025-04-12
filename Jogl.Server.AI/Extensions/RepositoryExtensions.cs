using Jogl.Server.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.AI.Extensions
{
    public static class RepositoryExtensions
    {
        public static void AddAI(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAIService, ClaudeAIService>();
        }
    }
}