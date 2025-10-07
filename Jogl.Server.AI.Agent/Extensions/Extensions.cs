using Jogl.Server.AI.Extensions;
using Jogl.Server.Business.Extensions;
using Jogl.Server.Search.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.AI.Agent.Extensions
{
    public static class Extensions
    {
        public static void AddAIAgent(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddBusiness();
            serviceCollection.AddTransient<IAgent, DefaultAgent>();
            serviceCollection.AddAI();
            serviceCollection.AddSearch();
        }
    }
}