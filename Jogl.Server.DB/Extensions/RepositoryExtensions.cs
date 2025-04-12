using Jogl.Server.DB.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Jogl.Server.DB.Extensions
{
    public static class RepositoryExtensions
    {
        public static void AddRepositories(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IChannelRepository, ChannelRepository>();
            serviceCollection.AddTransient<IProjectRepository, ProjectRepository>();
            serviceCollection.AddTransient<ICallForProposalRepository, CallForProposalRepository>();
            serviceCollection.AddTransient<IWorkspaceRepository, WorkspaceRepository>();
            serviceCollection.AddTransient<INodeRepository, NodeRepository>();
            serviceCollection.AddTransient<IOrganizationRepository, OrganizationRepository>();
            serviceCollection.AddTransient<IFeedRepository, FeedRepository>();
            serviceCollection.AddTransient<IInvitationRepository, InvitationRepository>();
            serviceCollection.AddTransient<IMembershipRepository, MembershipRepository>();
            serviceCollection.AddTransient<IUserRepository, UserRepository>();
            serviceCollection.AddTransient<IUserVerificationCodeRepository, UserVerificationCodeRepository>();
            serviceCollection.AddTransient<IDocumentRepository, DocumentRepository>();
            serviceCollection.AddTransient<IFolderRepository, FolderRepository>();
            serviceCollection.AddTransient<IImageRepository, ImageRepository>();
            serviceCollection.AddTransient<INeedRepository, NeedRepository>();
            serviceCollection.AddTransient<IContentEntityRepository, ContentEntityRepository>();
            serviceCollection.AddTransient<IReactionRepository, ReactionRepository>();
            serviceCollection.AddTransient<ICommentRepository, CommentRepository>();
            serviceCollection.AddTransient<IRelationRepository, RelationRepository>();
            serviceCollection.AddTransient<IUserFollowingRepository, UserFollowingRepository>();
            serviceCollection.AddTransient<ICommunityEntityFollowingRepository, CommunityEntityFollowingRepository>();
            serviceCollection.AddTransient<ICommunityEntityInvitationRepository, CommunityEntityInvitationRepository>();
            serviceCollection.AddTransient<ISkillRepository, SkillRepository>();
            serviceCollection.AddTransient<ITagRepository, TagRepository>();
            serviceCollection.AddTransient<IPaperRepository, PaperRepository>();
            serviceCollection.AddTransient<IResourceRepository, ResourceRepository>();
            serviceCollection.AddTransient<IOnboardingQuestionnaireInstanceRepository, OnboardingQuestionnaireInstanceRepository>();
            serviceCollection.AddTransient<INotificationRepository, NotificationRepository>();
            serviceCollection.AddTransient<IUserFeedRecordRepository, UserFeedRecordRepository>();
            serviceCollection.AddTransient<IUserContentEntityRecordRepository, UserContentEntityRecordRepository>();
            serviceCollection.AddTransient<IMentionRepository, MentionRepository>();
            serviceCollection.AddTransient<IProposalRepository, ProposalRepository>();
            serviceCollection.AddTransient<IEventRepository, EventRepository>();
            serviceCollection.AddTransient<IDraftRepository, DraftRepository>();
            serviceCollection.AddTransient<IEventAttendanceRepository, EventAttendanceRepository>();
            serviceCollection.AddTransient<IWaitlistRecordRepository, WaitlistRecordRepository>();
            serviceCollection.AddTransient<IPushNotificationTokenRepository, PushNotificationTokenRepository>();
            serviceCollection.AddTransient<IFeedIntegrationRepository, FeedIntegrationRepository>();
            serviceCollection.AddTransient<IPublicationRepository, PublicationRepository>();
            serviceCollection.AddTransient<IInvitationKeyRepository, InvitationKeyRepository>();
            serviceCollection.AddTransient<IEmailRecordRepository, EmailRecordRepository>();
            serviceCollection.AddTransient<ISystemValueRepository, SystemValueRepository>();
            serviceCollection.AddTransient<IInterfaceChannelRepository, InterfaceChannelRepository>();
            serviceCollection.AddTransient<IOperationContext, OperationContext>();
        }
    }
}