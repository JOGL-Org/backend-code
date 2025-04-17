using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.GitHub.Extensions
{
    public static class Extensions
    {
        public static void AddGithub(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IGitHubFacade, GitHubFacade>();
        }
    }
}