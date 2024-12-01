using Jogl.Server.Business;
using Jogl.Server.DB.Extensions;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Jogl.Server.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Jogl.Server.DB.Context;

var host = new HostBuilder()
    //  .ConfigureFunctionsWorkerDefaults()
    .ConfigureFunctionsWebApplication()
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

        services.AddScoped<IOperationContext, OperationContext>();
        services.AddRepositories();
        services.AddTransient<ICommunityEntityService, CommunityEntityService>();
        services.AddTransient<ICommunityEntityInvitationService, CommunityEntityInvitationService>();
        services.AddTransient<ICommunityEntityMembershipService, CommunityEntityMembershipService>();
        services.AddTransient<IChannelService, ChannelService>();
        services.AddTransient<ICallForProposalService, CallForProposalService>();
        services.AddTransient<IWorkspaceService, WorkspaceService>();
        services.AddTransient<INodeService, NodeService>();
        services.AddTransient<IOrganizationService, OrganizationService>();
        services.AddTransient<IMembershipService, MembershipService>();
        services.AddTransient<IInvitationService, InvitationService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IUserVerificationService, UserVerificationService>();
        services.AddTransient<IDocumentService, DocumentService>();
        services.AddTransient<IImageService, ImageService>();
        services.AddTransient<INeedService, NeedService>();
        services.AddTransient<IContentService, ContentService>();
        services.AddTransient<ITagService, TagService>();
        services.AddTransient<IAccessService, AccessService>();
        services.AddTransient<IPaperService, PaperService>();
        services.AddTransient<IResourceService, ResourceService>();
        services.AddTransient<IProposalService, ProposalService>();
        services.AddTransient<INotificationService, NotificationService>();
        services.AddTransient<IUrlService, UrlService>();
        services.AddTransient<IEventService, EventService>();
        services.AddTransient<IEntityService, EntityService>();
        services.AddTransient<IFeedEntityService, FeedEntityService>();

        //email
        services.AddTransient<IEmailService, SendGridEmailService>();

        //push notifications
        services.AddSingleton<IPushNotificationService, FirebasePushNotificationService>();
    })
    .Build();

host.Run();
