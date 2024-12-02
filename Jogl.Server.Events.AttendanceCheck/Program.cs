using Jogl.Server.Business;
using Jogl.Server.Configuration;
using Jogl.Server.DB.Extensions;
using Jogl.Server.DB.Context;
using Jogl.Server.Events;
using Jogl.Server.URL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(config =>
      {
          config.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile($"appSettings.json", false, true)
              .AddJsonFile($"appSettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", false)
              .AddEnvironmentVariables()
              .AddKeyVault();
      })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddTransient<IFeedEntityService, FeedEntityService>();
        services.AddTransient<ICalendarService, GoogleCalendarService>();
        services.AddTransient<IUrlService, UrlService>();

        services.AddScoped<IOperationContext, OperationContext>();
        services.AddRepositories();


        services.AddResiliencePipeline("retry", builder =>
        {
            builder
                .AddRetry(new RetryStrategyOptions())
                .AddTimeout(TimeSpan.FromSeconds(3));
        });
    })
    .Build();

host.Run();
