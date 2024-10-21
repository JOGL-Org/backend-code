using AutoMapper;
using Jogl.Server.API.Converters;
using Jogl.Server.API.Mapping;
using Jogl.Server.API.Middleware;
using Jogl.Server.Auth;
using Jogl.Server.Business;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.OpenAlex;
using Jogl.Server.Orcid;
using Jogl.Server.PubMed;
using Jogl.Server.GoogleAuth;
using Jogl.Server.SemanticScholar;
using Jogl.Server.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using Jogl.Server.Events;
using Polly;
using Polly.Retry;
using Jogl.Server.LinkedIn;
using Jogl.Server.DB.Initialization;
using Jogl.Server.API.Services;
using Jogl.Server.GitHub;
using Jogl.Server.Notifications;
using Jogl.Server.URL;
using Jogl.Server.Images;
using Jogl.Server.ServiceBus;
using Jogl.Server.Documents;
using Jogl.Server.Configuration;
using Jogl.Server.HuggingFace;
using Jogl.Server.Arxiv;

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.SchemaFilter<SchemaFilter>();
    config.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JOGL API",
        Version = "v1"
    });
    config.SupportNonNullableReferenceTypes();
    config.EnableAnnotations();
    config.ParameterFilter<QueryArrayParameterFilter>();
});

// Program.cs

builder.Services.AddTransient<TelemetryMiddleware>();
//api services
builder.Services.AddTransient<IVerificationService, CaptchaVerificationService>();
builder.Services.AddTransient<IContextService, ContextService>();
//auth services
builder.Services.AddTransient<IAuthService, AuthService>();
//business
builder.Services.AddTransient<ICommunityEntityService, CommunityEntityService>();
builder.Services.AddTransient<ICommunityEntityInvitationService, CommunityEntityInvitationService>();
builder.Services.AddTransient<ICommunityEntityMembershipService, CommunityEntityMembershipService>();
builder.Services.AddTransient<IChannelService, ChannelService>();
builder.Services.AddTransient<ICallForProposalService, CallForProposalService>();
builder.Services.AddTransient<IWorkspaceService, WorkspaceService>();
builder.Services.AddTransient<INodeService, NodeService>();
builder.Services.AddTransient<IOrganizationService, OrganizationService>();
builder.Services.AddTransient<IMembershipService, MembershipService>();
builder.Services.AddTransient<IInvitationService, InvitationService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IUserVerificationService, UserVerificationService>();
builder.Services.AddTransient<IDocumentService, DocumentService>();
builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<INeedService, NeedService>();
builder.Services.AddTransient<IContentService, ContentService>();
builder.Services.AddTransient<ITagService, TagService>();
builder.Services.AddTransient<IAccessService, AccessService>();
builder.Services.AddTransient<IPaperService, PaperService>();
builder.Services.AddTransient<IResourceService, ResourceService>();
builder.Services.AddTransient<IProposalService, ProposalService>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<IUrlService, UrlService>();
builder.Services.AddTransient<IEventService, EventService>();
builder.Services.AddTransient<IEntityService, EntityService>();
builder.Services.AddTransient<IFeedEntityService, FeedEntityService>();

//email
builder.Services.AddTransient<IEmailService, SendGridEmailService>();

//storage
builder.Services.AddTransient<IStorageService, BlobStorageService>();

//data access
builder.Services.AddTransient<IChannelRepository, ChannelRepository>();
builder.Services.AddTransient<IProjectRepository, ProjectRepository>();
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
builder.Services.AddTransient<IDraftRepository, DraftRepository>();
builder.Services.AddTransient<IEventAttendanceRepository, EventAttendanceRepository>();
builder.Services.AddTransient<IWaitlistRecordRepository, WaitlistRecordRepository>();
builder.Services.AddTransient<IPushNotificationTokenRepository, PushNotificationTokenRepository>();
builder.Services.AddTransient<IFeedIntegrationRepository, FeedIntegrationRepository>();
builder.Services.AddTransient<IPublicationRepository, PublicationRepository>();
builder.Services.AddTransient<IInitializer, Initializer>();
builder.Services.AddInitializers();

builder.Services.AddTransient<IOrcidFacade, OrcidFacade>();
builder.Services.AddTransient<ISemanticScholarFacade, SemanticScholarFacade>();
builder.Services.AddTransient<IPubMedFacade, PubMedFacade>();
builder.Services.AddTransient<IGoogleFacade, GoogleFacade>();
builder.Services.AddTransient<ILinkedInFacade, LinkedInFacade>();
builder.Services.AddTransient<IGitHubFacade, GitHubFacade>();
builder.Services.AddTransient<IArxivFacade, ArxivFacade>();
builder.Services.AddTransient<IHuggingFaceFacade, HuggingFaceFacade>();
builder.Services.AddTransient<IOpenAlexFacade, OpenAlexFacade>();

//images
builder.Services.AddTransient<IConversionService, ConversionService>();

//documents
builder.Services.AddTransient<IDocumentConverter, DocumentConverter>();

//calendar
builder.Services.AddTransient<ICalendarService, GoogleCalendarService>();

//notifications
builder.Services.AddTransient<INotificationFacade, ServiceBusNotificationFacade>();

//service bus
builder.Services.AddTransient<IServiceBusProxy, AzureServiceBusProxy>();

builder.Services.AddResiliencePipeline("retry", builder =>
{
    builder
        .AddRetry(new RetryStrategyOptions())
        .AddTimeout(TimeSpan.FromSeconds(3));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(provider => new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new MappingProfiles(provider.GetService<IHttpContextAccessor>()));
}).CreateMapper());

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(new LowercaseJsonNamingPolicy()));
    //options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = true;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:Key"])),
        ValidateLifetime = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddApplicationInsightsTelemetry();

//add secrets
builder.Configuration.AddKeyVault();

var app = builder.Build();

// initialize licenses
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(app.Configuration["Syncfusion:License"]);

// initialize DB
var dbinitializer = app.Services.GetService<IInitializer>();
dbinitializer.InitializeAsync().Wait();

//enable CORS
app.UseCors();

// configure swaggger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//enable custom middleware
app.UseMiddleware<JObjectMiddleware>();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<TelemetryMiddleware>();

app.MapControllers();
app.Run();
