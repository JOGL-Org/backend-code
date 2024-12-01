using Jogl.Server.AI;
using Jogl.Server.DB.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Jogl.Server.Configuration;
using Jogl.Server.Notifications;
using Jogl.Server.ServiceBus;
using Jogl.Server.DB.Context;

var host = new HostBuilder()
    //  .ConfigureFunctionsWorkerDefaults()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appSettings.json", false, true)
            .AddJsonFile($"appSettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development"}.json", false)
            .AddEnvironmentVariables()
            .AddKeyVault();
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddScoped<IOperationContext, OperationContext>();
        services.AddRepositories();

        //notifications
        services.AddTransient<INotificationFacade, ServiceBusNotificationFacade>();

        //service bus
        services.AddTransient<IServiceBusProxy, AzureServiceBusProxy>();

        //ai service
        services.AddTransient<IAIService, ClaudeAIService>();
    })
    .Build();

host.Run();
