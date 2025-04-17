using Jogl.Server.Arxiv.Extensions;
using Jogl.Server.Auth.Extensions;
using Jogl.Server.Email.Extensions;
using Jogl.Server.Events.Extensions;
using Jogl.Server.GitHub.Extensions;
using Jogl.Server.Orcid.Extensions;
using Jogl.Server.SemanticScholar.Extensions;
using Jogl.Server.HuggingFace.Extensions;
using Jogl.Server.Notifications.Extensions;
using Jogl.Server.PubMed.Extensions;
using Jogl.Server.DB.Extensions;
using Jogl.Server.Storage.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Jogl.Server.URL.Extensions;

namespace Jogl.Server.Business.Extensions
{
    public static class Extensions
    {
        public static void AddBusiness(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICommunityEntityService, CommunityEntityService>();
            serviceCollection.AddTransient<ICommunityEntityInvitationService, CommunityEntityInvitationService>();
            serviceCollection.AddTransient<ICommunityEntityMembershipService, CommunityEntityMembershipService>();
            serviceCollection.AddTransient<IChannelService, ChannelService>();
            serviceCollection.AddTransient<ICallForProposalService, CallForProposalService>();
            serviceCollection.AddTransient<IWorkspaceService, WorkspaceService>();
            serviceCollection.AddTransient<INodeService, NodeService>();
            serviceCollection.AddTransient<IOrganizationService, OrganizationService>();
            serviceCollection.AddTransient<IMembershipService, MembershipService>();
            serviceCollection.AddTransient<IInvitationService, InvitationService>();
            serviceCollection.AddTransient<IUserService, UserService>();
            serviceCollection.AddTransient<IUserVerificationService, UserVerificationService>();
            serviceCollection.AddTransient<IDocumentService, DocumentService>();
            serviceCollection.AddTransient<IImageService, ImageService>();
            serviceCollection.AddTransient<INeedService, NeedService>();
            serviceCollection.AddTransient<IContentService, ContentService>();
            serviceCollection.AddTransient<ITagService, TagService>();
            serviceCollection.AddTransient<IAccessService, AccessService>();
            serviceCollection.AddTransient<IPaperService, PaperService>();
            serviceCollection.AddTransient<IResourceService, ResourceService>();
            serviceCollection.AddTransient<IProposalService, ProposalService>();
            serviceCollection.AddTransient<INotificationService, NotificationService>();
            serviceCollection.AddTransient<IEventService, EventService>();
            serviceCollection.AddTransient<IEntityService, EntityService>();
            serviceCollection.AddTransient<IFeedEntityService, FeedEntityService>();
            serviceCollection.AddTransient<IRelationService, RelationService>();

            serviceCollection.AddRepositories();

            serviceCollection.AddUrls();
            serviceCollection.AddEmail();
            serviceCollection.AddNotifications();
            serviceCollection.AddAuth();
            serviceCollection.AddStorage();
            serviceCollection.AddEvents();

            serviceCollection.AddHuggingFace();
            serviceCollection.AddGithub();
            serviceCollection.AddArxiv();
            serviceCollection.AddPubmed();
            serviceCollection.AddOpenAlex();
            serviceCollection.AddOrcid();
            serviceCollection.AddSemanticScholar();
        }
    }
}