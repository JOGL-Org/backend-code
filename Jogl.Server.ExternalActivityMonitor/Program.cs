using Jogl.Server.Arxiv;
using Jogl.Server.Business;
using Jogl.Server.Configuration;
using Jogl.Server.DB.Extensions;
using Jogl.Server.DB.Context;
using Jogl.Server.Email;
using Jogl.Server.GitHub;
using Jogl.Server.HuggingFace;
using Jogl.Server.Notifications;
using Jogl.Server.PubMed;
using Jogl.Server.ServiceBus;
using Jogl.Server.Storage;
using Jogl.Server.URL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
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
        services.AddTransient<IContentService, ContentService>();
        services.AddTransient<IStorageService, BlobStorageService>();
        services.AddTransient<IFeedEntityService, FeedEntityService>();
        services.AddTransient<ICommunityEntityService, CommunityEntityService>();
        services.AddTransient<INotificationFacade, ServiceBusNotificationFacade>();
        services.AddTransient<IServiceBusProxy, AzureServiceBusProxy>();
        services.AddTransient<INotificationService, NotificationService>();
        services.AddTransient<IEmailService, SendGridEmailService>();
        services.AddTransient<IUrlService, UrlService>();

        services.AddTransient<IGitHubFacade, GitHubFacade>();
        services.AddTransient<IHuggingFaceFacade, HuggingFaceFacade>();
        services.AddTransient<IPubMedFacade, PubMedFacade>();
        services.AddTransient<IArxivFacade, ArxivFacade>();

        //data access
        services.AddScoped<IOperationContext, OperationContext>();
        services.AddRepositories();

    })
    .Build();

host.Run();
