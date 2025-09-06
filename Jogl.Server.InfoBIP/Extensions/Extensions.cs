using Jogl.Server.InfoBIP;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.InfoBIP.Extensions
{
    public static class Extensions
    {
        public static void AddInfoBIP(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IInfoBIPConversationService, InfoBIPConversationService>();
        }
    }
}