using Jogl.Server.Business;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Jogl.Server.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        services.AddTransient<IPushNotificationTokenRepository, PushNotificationTokenRepository>();
        services.AddTransient<IInvitationKeyRepository, InvitationKeyRepository>();

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
