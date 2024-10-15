using AutoMapper;
using Jogl.Server.API.Mapping;
using Jogl.Server.API.Services;
using Jogl.Server.Auth;
using Jogl.Server.Business;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.Events;
using Jogl.Server.GoogleAuth;
using Jogl.Server.LinkedIn;
using Jogl.Server.Notifications;
using Jogl.Server.OpenAlex;
using Jogl.Server.Orcid;
using Jogl.Server.PubMed;
using Jogl.Server.SemanticScholar;
using Jogl.Server.Storage;
using Jogl.Server.URL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Jogl.Server.API.Test
{
    public abstract class ControllerTestBase
    {
        protected Mock<ICallForProposalRepository> _callForProposalsRepository = new Mock<ICallForProposalRepository>();
        protected Mock<IWorkspaceRepository> _workspaceRepository = new Mock<IWorkspaceRepository>();
        protected Mock<INodeRepository> _nodeRepository = new Mock<INodeRepository>();
        protected Mock<IOrganizationRepository> _organizationRepository = new Mock<IOrganizationRepository>();
        protected Mock<IFeedRepository> _feedRepository = new Mock<IFeedRepository>();
        protected Mock<IInvitationRepository> _invitationRepository = new Mock<IInvitationRepository>();
        protected Mock<IMembershipRepository> _membershipRepository = new Mock<IMembershipRepository>();
        protected Mock<IUserRepository> _userRepository = new Mock<IUserRepository>();
        protected Mock<IUserVerificationCodeRepository> _userVerificationCodeRepository = new Mock<IUserVerificationCodeRepository>();
        protected Mock<IDocumentRepository> _documentRepository = new Mock<IDocumentRepository>();
        protected Mock<IFolderRepository> _folderRepository = new Mock<IFolderRepository>();
        protected Mock<IImageRepository> _imageRepository = new Mock<IImageRepository>();
        protected Mock<INeedRepository> _needRepository = new Mock<INeedRepository>();
        protected Mock<IContentEntityRepository> _contentEntityRepository = new Mock<IContentEntityRepository>();
        protected Mock<IReactionRepository> _reactionRepository = new Mock<IReactionRepository>();
        protected Mock<ICommentRepository> _commentRepository = new Mock<ICommentRepository>();
        protected Mock<IRelationRepository> _relationRepository = new Mock<IRelationRepository>();
        protected Mock<IUserFollowingRepository> _userFollowingRepository = new Mock<IUserFollowingRepository>();
        protected Mock<ICommunityEntityFollowingRepository> _communityEntityFollowingRepository = new Mock<ICommunityEntityFollowingRepository>();
        protected Mock<ICommunityEntityInvitationRepository> _communityEntityInvitationRepository = new Mock<ICommunityEntityInvitationRepository>();
        protected Mock<ISkillRepository> _skillRepository = new Mock<ISkillRepository>();
        protected Mock<ITagRepository> _tagRepository = new Mock<ITagRepository>();
        protected Mock<IPaperRepository> _paperRepository = new Mock<IPaperRepository>();
        protected Mock<IResourceRepository> _resourceRepository = new Mock<IResourceRepository>();
        protected Mock<IOnboardingQuestionnaireInstanceRepository> _onboardingQuestionnaireInstanceRepository = new Mock<IOnboardingQuestionnaireInstanceRepository>();
        protected Mock<INotificationRepository> _notificationRepository = new Mock<INotificationRepository>();
        protected Mock<IUserFeedRecordRepository> _userFeedRecordRepository = new Mock<IUserFeedRecordRepository>();
        protected Mock<IUserContentEntityRecordRepository> _userContentEntityRecordRepository = new Mock<IUserContentEntityRecordRepository>();
        protected Mock<IMentionRepository> _mentionRepository = new Mock<IMentionRepository>();
        protected Mock<IProposalRepository> _proposalRepository = new Mock<IProposalRepository>();
        protected Mock<IEventRepository> _eventRepository = new Mock<IEventRepository>();
        protected Mock<IEventAttendanceRepository> _eventAttendanceRepository = new Mock<IEventAttendanceRepository>();
        protected Mock<IWaitlistRecordRepository> _waitlistRecordRepository = new Mock<IWaitlistRecordRepository>();
        protected Mock<IPushNotificationTokenRepository> _pushNotificationTokenRepository = new Mock<IPushNotificationTokenRepository>();

        protected Mock<IContextService> _contextService = new Mock<IContextService>();
        protected ServiceCollection _serviceCollection = new ServiceCollection();

        public virtual void Setup()
        {
            _serviceCollection.AddSingleton(_contextService.Object);

            //business
            _serviceCollection.AddTransient<ICommunityEntityService, CommunityEntityService>();
            _serviceCollection.AddTransient<ICommunityEntityInvitationService, CommunityEntityInvitationService>();
            _serviceCollection.AddTransient<ICommunityEntityMembershipService, CommunityEntityMembershipService>();
            _serviceCollection.AddTransient<IChannelService, ChannelService>();
            _serviceCollection.AddTransient<ICallForProposalService, CallForProposalService>();
            _serviceCollection.AddTransient<IWorkspaceService, WorkspaceService>();
            _serviceCollection.AddTransient<INodeService, NodeService>();
            _serviceCollection.AddTransient<IOrganizationService, OrganizationService>();
            _serviceCollection.AddTransient<IMembershipService, MembershipService>();
            _serviceCollection.AddTransient<IInvitationService, InvitationService>();
            _serviceCollection.AddTransient<IUserService, UserService>();
            _serviceCollection.AddTransient<IDocumentService, DocumentService>();
            _serviceCollection.AddTransient<IImageService, ImageService>();
            _serviceCollection.AddTransient<INeedService, NeedService>();
            _serviceCollection.AddTransient<IContentService, ContentService>();
            _serviceCollection.AddTransient<ITagService, TagService>();
            _serviceCollection.AddTransient<IAccessService, AccessService>();
            _serviceCollection.AddTransient<IPaperService, PaperService>();
            _serviceCollection.AddTransient<IResourceService, ResourceService>();
            _serviceCollection.AddTransient<IProposalService, ProposalService>();
            _serviceCollection.AddTransient<INotificationService, NotificationService>();
            _serviceCollection.AddTransient<IUrlService, UrlService>();
            _serviceCollection.AddTransient<IEventService, EventService>();
            _serviceCollection.AddTransient<IEntityService, EntityService>();
            _serviceCollection.AddTransient<IFeedEntityService, FeedEntityService>();

            //data access
            _serviceCollection.AddSingleton(_callForProposalsRepository.Object);
            _serviceCollection.AddSingleton(_workspaceRepository.Object);
            _serviceCollection.AddSingleton(_nodeRepository.Object);
            _serviceCollection.AddSingleton(_organizationRepository.Object);
            _serviceCollection.AddSingleton(_feedRepository.Object);
            _serviceCollection.AddSingleton(_invitationRepository.Object);
            _serviceCollection.AddSingleton(_membershipRepository.Object);
            _serviceCollection.AddSingleton(_userRepository.Object);
            _serviceCollection.AddSingleton(_userVerificationCodeRepository.Object);
            _serviceCollection.AddSingleton(_documentRepository.Object);
            _serviceCollection.AddSingleton(_folderRepository.Object);
            _serviceCollection.AddSingleton(_imageRepository.Object);
            _serviceCollection.AddSingleton(_needRepository.Object);
            _serviceCollection.AddSingleton(_contentEntityRepository.Object);
            _serviceCollection.AddSingleton(_reactionRepository.Object);
            _serviceCollection.AddSingleton(_commentRepository.Object);
            _serviceCollection.AddSingleton(_relationRepository.Object);
            _serviceCollection.AddSingleton(_userFollowingRepository.Object);
            _serviceCollection.AddSingleton(_communityEntityFollowingRepository.Object);
            _serviceCollection.AddSingleton(_communityEntityInvitationRepository.Object);
            _serviceCollection.AddSingleton(_skillRepository.Object);
            _serviceCollection.AddSingleton(_tagRepository.Object);
            _serviceCollection.AddSingleton(_paperRepository.Object);
            _serviceCollection.AddSingleton(_resourceRepository.Object);
            _serviceCollection.AddSingleton(_onboardingQuestionnaireInstanceRepository.Object);
            _serviceCollection.AddSingleton(_notificationRepository.Object);
            _serviceCollection.AddSingleton(_userFeedRecordRepository.Object);
            _serviceCollection.AddSingleton(_userContentEntityRecordRepository.Object);
            _serviceCollection.AddSingleton(_mentionRepository.Object);
            _serviceCollection.AddSingleton(_proposalRepository.Object);
            _serviceCollection.AddSingleton(_eventRepository.Object);
            _serviceCollection.AddSingleton(_eventAttendanceRepository.Object);
            _serviceCollection.AddSingleton(_waitlistRecordRepository.Object);
            _serviceCollection.AddSingleton(_pushNotificationTokenRepository.Object);

            _serviceCollection.AddSingleton(new Mock<IOrcidFacade>().Object);
            _serviceCollection.AddSingleton(new Mock<ISemanticScholarFacade>().Object);
            _serviceCollection.AddSingleton(new Mock<IPubMedFacade>().Object);
            _serviceCollection.AddSingleton(new Mock<IGoogleFacade>().Object);
            _serviceCollection.AddSingleton(new Mock<ILinkedInFacade>().Object);
            _serviceCollection.AddSingleton(new Mock<IOpenAlexFacade>().Object);
            _serviceCollection.AddSingleton(new Mock<IAuthService>().Object);
            _serviceCollection.AddSingleton(new Mock<IStorageService>().Object);
            _serviceCollection.AddSingleton(new Mock<ICalendarService>().Object);
            _serviceCollection.AddSingleton(new Mock<IConfiguration>().Object);
            _serviceCollection.AddSingleton(new Mock<IHttpContextAccessor>().Object);
            _serviceCollection.AddSingleton(new Mock<INotificationFacade>().Object);
            _serviceCollection.AddSingleton(new Mock<IEmailService>().Object);

            _serviceCollection.AddLogging();
            _serviceCollection.AddSingleton(provider => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfiles(provider.GetService<IHttpContextAccessor>()));
            }).CreateMapper());
        }

        protected void AssertIsReturned<T>(IActionResult result)
        {
            Assert.AreEqual(typeof(OkObjectResult), result.GetType());
            if (typeof(T).IsInterface)
                Assert.IsTrue(((OkObjectResult)result).Value.GetType().IsAssignableTo(typeof(T)));
            else
                Assert.AreEqual(typeof(T), ((OkObjectResult)result).Value.GetType());
        }

        protected void AssertIsForbidden(IActionResult result)
        {
            Assert.AreEqual(typeof(ForbidResult), result.GetType());
        }
    }
}