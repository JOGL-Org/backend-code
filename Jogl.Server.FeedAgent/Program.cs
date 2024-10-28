using Jogl.Server.AI;
using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Jogl.Server.Configuration;
using Jogl.Server.Notifications;
using Jogl.Server.ServiceBus;

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

        services.AddTransient<IChannelRepository, ChannelRepository>();
        services.AddTransient<ICallForProposalRepository, CallForProposalRepository>();
        services.AddTransient<IWorkspaceRepository, WorkspaceRepository>();
        services.AddTransient<INodeRepository, NodeRepository>();
        services.AddTransient<IOrganizationRepository, OrganizationRepository>();
        services.AddTransient<IFeedRepository, FeedRepository>();
        services.AddTransient<IInvitationRepository, InvitationRepository>();
        services.AddTransient<IMembershipRepository, MembershipRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IUserVerificationCodeRepository, UserVerificationCodeRepository>();
        services.AddTransient<IDocumentRepository, DocumentRepository>();
        services.AddTransient<IFolderRepository, FolderRepository>();
        services.AddTransient<IImageRepository, ImageRepository>();
        services.AddTransient<INeedRepository, NeedRepository>();
        services.AddTransient<IContentEntityRepository, ContentEntityRepository>();
        services.AddTransient<IReactionRepository, ReactionRepository>();
        services.AddTransient<ICommentRepository, CommentRepository>();
        services.AddTransient<IRelationRepository, RelationRepository>();
        services.AddTransient<IUserFollowingRepository, UserFollowingRepository>();
        services.AddTransient<ICommunityEntityFollowingRepository, CommunityEntityFollowingRepository>();
        services.AddTransient<ICommunityEntityInvitationRepository, CommunityEntityInvitationRepository>();
        services.AddTransient<ISkillRepository, SkillRepository>();
        services.AddTransient<ITagRepository, TagRepository>();
        services.AddTransient<IPaperRepository, PaperRepository>();
        services.AddTransient<IResourceRepository, ResourceRepository>();
        services.AddTransient<IOnboardingQuestionnaireInstanceRepository, OnboardingQuestionnaireInstanceRepository>();
        services.AddTransient<INotificationRepository, NotificationRepository>();
        services.AddTransient<IUserFeedRecordRepository, UserFeedRecordRepository>();
        services.AddTransient<IUserContentEntityRecordRepository, UserContentEntityRecordRepository>();
        services.AddTransient<IMentionRepository, MentionRepository>();
        services.AddTransient<IProposalRepository, ProposalRepository>();
        services.AddTransient<IEventRepository, EventRepository>();
        services.AddTransient<IEventAttendanceRepository, EventAttendanceRepository>();
        services.AddTransient<IWaitlistRecordRepository, WaitlistRecordRepository>();
        services.AddTransient<IEmailRecordRepository, EmailRecordRepository>();
        services.AddTransient<IFeedIntegrationRepository, FeedIntegrationRepository>();
        services.AddTransient<IPushNotificationTokenRepository, PushNotificationTokenRepository>();

        //notifications
        services.AddTransient<INotificationFacade, ServiceBusNotificationFacade>();

        //service bus
        services.AddTransient<IServiceBusProxy, AzureServiceBusProxy>();

        //ai service
        services.AddTransient<IAIService, ClaudeAIService>();
    })
    .Build();

host.Run();
