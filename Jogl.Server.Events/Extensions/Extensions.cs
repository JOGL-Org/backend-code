using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace Jogl.Server.Events.Extensions
{
    public static class Extensions
    {
        public static void AddEvents(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICalendarService, GoogleCalendarService>();
            serviceCollection.AddResiliencePipeline("retry", builder =>
            {
                builder
                    .AddRetry(new RetryStrategyOptions())
                    .AddTimeout(TimeSpan.FromSeconds(3));
            });
        }
    }
}