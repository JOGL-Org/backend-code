using Jogl.Server.DB;
using Jogl.Server.Sockets;
using Jogl.Server.WebSocketService;
using Jogl.Server.ServiceBus;
using Jogl.Server.WebSocketService.Sockets;
using Jogl.Server.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("*")
                 .AllowAnyMethod()
                 .AllowAnyHeader();
        });
});

//data access
builder.Services.AddTransient<IChannelRepository, ChannelRepository>();
builder.Services.AddTransient<ICallForProposalRepository, CallForProposalRepository>();
builder.Services.AddTransient<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddTransient<INodeRepository, NodeRepository>();
builder.Services.AddTransient<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddTransient<IFeedRepository, FeedRepository>();
builder.Services.AddTransient<IInvitationRepository, InvitationRepository>();
builder.Services.AddTransient<IMembershipRepository, MembershipRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserVerificationCodeRepository, UserVerificationCodeRepository>();
builder.Services.AddTransient<IDocumentRepository, DocumentRepository>();
builder.Services.AddTransient<IFolderRepository, FolderRepository>();
builder.Services.AddTransient<IImageRepository, ImageRepository>();
builder.Services.AddTransient<INeedRepository, NeedRepository>();
builder.Services.AddTransient<IContentEntityRepository, ContentEntityRepository>();
builder.Services.AddTransient<IReactionRepository, ReactionRepository>();
builder.Services.AddTransient<ICommentRepository, CommentRepository>();
builder.Services.AddTransient<IRelationRepository, RelationRepository>();
builder.Services.AddTransient<IUserFollowingRepository, UserFollowingRepository>();
builder.Services.AddTransient<ICommunityEntityFollowingRepository, CommunityEntityFollowingRepository>();
builder.Services.AddTransient<ICommunityEntityInvitationRepository, CommunityEntityInvitationRepository>();
builder.Services.AddTransient<ISkillRepository, SkillRepository>();
builder.Services.AddTransient<ITagRepository, TagRepository>();
builder.Services.AddTransient<IPaperRepository, PaperRepository>();
builder.Services.AddTransient<IResourceRepository, ResourceRepository>();
builder.Services.AddTransient<IOnboardingQuestionnaireInstanceRepository, OnboardingQuestionnaireInstanceRepository>();
builder.Services.AddTransient<INotificationRepository, NotificationRepository>();
builder.Services.AddTransient<IUserFeedRecordRepository, UserFeedRecordRepository>();
builder.Services.AddTransient<IUserContentEntityRecordRepository, UserContentEntityRecordRepository>();
builder.Services.AddTransient<IMentionRepository, MentionRepository>();
builder.Services.AddTransient<IProposalRepository, ProposalRepository>();
builder.Services.AddTransient<IEventRepository, EventRepository>();
builder.Services.AddTransient<IEventAttendanceRepository, EventAttendanceRepository>();
builder.Services.AddTransient<IWaitlistRecordRepository, WaitlistRecordRepository>();
builder.Services.AddTransient<IPushNotificationTokenRepository, PushNotificationTokenRepository>();

//sockets
builder.Services.AddSockets<JoglWebSocketGateway, IWebSocketGateway>();
builder.Services.AddHostedService<Service>();
//builder.Services.AddApplicationInsightsTelemetry();

//service bus
builder.Services.AddTransient<IServiceBusProxy, AzureServiceBusProxy>();

//add secrets
builder.Configuration.AddKeyVault();

var app = builder.Build();

//enable CORS
app.UseCors();

//configure websockets
app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();


app.Run();