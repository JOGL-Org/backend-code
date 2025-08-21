using Jogl.Server.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.Storage.Extensions
{
    public static class Extensions
    {
        public static void AddText(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ITextService, TextService>();
        }
    }
}