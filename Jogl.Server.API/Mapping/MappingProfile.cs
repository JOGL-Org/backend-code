using System.Text;
using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.Images;
using Jogl.Server.Orcid;
using MongoDB.Bson;

namespace Jogl.Server.API.Mapping
{
    public class MappingProfiles : Profile
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MappingProfiles(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            CreateMap<Geolocation, GeolocationModel>();
            CreateMap<GeolocationModel, Geolocation>();

            CreateMap<Timezone, TimezoneModel>();
            CreateMap<TimezoneModel, Timezone>();

            CreateMap<PrivacyLevelSetting, PrivacyLevelSettingModel>();
            CreateMap<PrivacyLevelSettingModel, PrivacyLevelSetting>();

            CreateMap<JoiningRestrictionLevelSetting, JoiningRestrictionLevelSettingModel>();
            CreateMap<JoiningRestrictionLevelSettingModel, JoiningRestrictionLevelSetting>();

            CreateMap<string, ObjectId>().ConstructUsing(id => ObjectId.Parse(id));
            CreateMap<ObjectId, string>().ConstructUsing(id => id.ToString());

            CreateMap<string, byte[]>().ConstructUsing(data => Convert.FromBase64String(data.Contains(",") ? data.Substring(data.IndexOf(",") + 1) : data));
            CreateMap<byte[], string>().ConstructUsing(data => Convert.ToBase64String(data));

            CreateMap<CommunityEntity, CommunityEntityStatModel>();
            CreateMap<CommunityEntity, CommunityEntityDetailStatModel>();

            CreateMap<AccessLevel?, string>().ConstructUsing(level => GetUserAccessLevel(level));

            //CE
            CreateMap<CommunityEntity, EntityMiniModel>()
                .ForMember(dst => dst.EntityType, opt => opt.MapFrom(src => src.FeedType))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId, true)))
                .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.LogoId)))
                .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.LogoId, true)));

            CreateMap<CommunityEntity, CommunityEntityMiniModel>()
                .ForMember(dst => dst.UserAccessLevel, opt => opt.MapFrom(src => src.AccessLevel))
                .ForMember(dst => dst.Onboarded, opt => opt.MapFrom(src => src.OnboardedUTC.HasValue))
                .ForMember(dst => dst.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId, true)))
                .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.LogoId)))
                .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.LogoId, true)))
                .ForMember(dst => dst.Access, opt => opt.MapFrom((src, dst, ctx) =>
                {
                    return new CommunityEntityPermissionModel
                    {
                        Permissions = src.Permissions
                    };
                }))
                .ForMember(dst => dst.Stats, opt => opt.MapFrom(src => src));

            CreateMap<CommunityEntity, CommunityEntityMiniDetailModel>()
              .IncludeBase<CommunityEntity, CommunityEntityMiniModel>()
              .ForMember(dst => dst.DetailStats, opt => opt.MapFrom(src => src));

            CreateMap<CommunityEntity, CommunityEntityModel>()
             .ForMember(dst => dst.UserAccessLevel, opt => opt.MapFrom(src => src.AccessLevel))
             .ForMember(dst => dst.Onboarded, opt => opt.MapFrom(src => src.OnboardedUTC.HasValue))
             .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId)))
             .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId, true)))
             .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.LogoId)))
             .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.LogoId, true)))
             .ForMember(dst => dst.Access, opt => opt.MapFrom((src, dst, ctx) =>
             {
                 return new CommunityEntityPermissionModel
                 {
                     Permissions = src.Permissions
                 };
             }))
             .ForMember(dst => dst.Stats, opt => opt.MapFrom(src => src));

            CreateMap<CommunityEntity, CommunityEntityChannelModel>()
             .IncludeBase<CommunityEntity, CommunityEntityMiniModel>();

            CreateMap<Workspace, CommunityEntityChannelModel>()
             .IncludeBase<CommunityEntity, CommunityEntityChannelModel>();

            ////project
            //CreateMap<Project, EntityMiniModel>()
            //    .IncludeBase<CommunityEntity, EntityMiniModel>();
            //CreateMap<Project, CommunityEntityModel>()
            //    .IncludeBase<CommunityEntity, CommunityEntityModel>();
            //CreateMap<Project, CommunityEntityMiniModel>()
            //    .IncludeBase<CommunityEntity, CommunityEntityMiniModel>();
            //CreateMap<Project, CommunityEntityMiniDetailModel>()
            //    .IncludeBase<CommunityEntity, CommunityEntityMiniDetailModel>();
            //CreateMap<Project, ProjectModel>()
            //    .IncludeBase<Project, CommunityEntityModel>();
            //CreateMap<Project, ProjectDetailModel>()
            //    .ForMember(dst => dst.DetailStats, opt => opt.MapFrom(src => src))
            //    .IncludeBase<Project, ProjectModel>();

            //CreateMap<Project, CommunityEntityDetailStatModel>()
            //    .IncludeBase<CommunityEntity, CommunityEntityDetailStatModel>();

            //CreateMap<Project, ProjectUpsertModel>();
            //CreateMap<ProjectUpsertModel, Project>();

            //call for proposal
            CreateMap<CallForProposal, EntityMiniModel>()
                .IncludeBase<CommunityEntity, EntityMiniModel>();
            CreateMap<CallForProposal, CommunityEntityModel>()
                .IncludeBase<CommunityEntity, CommunityEntityModel>();
            CreateMap<CallForProposal, CommunityEntityMiniModel>()
                .IncludeBase<CommunityEntity, CommunityEntityMiniModel>();
            CreateMap<CallForProposal, CommunityEntityMiniDetailModel>()
                .IncludeBase<CommunityEntity, CommunityEntityMiniDetailModel>();
            CreateMap<CallForProposal, CallForProposalModel>()
                .ForMember(dst => dst.CFPStats, opt => opt.MapFrom(src => src))
                .IncludeBase<CallForProposal, CommunityEntityModel>();
            CreateMap<CallForProposal, CallForProposalMiniModel>()
                .IncludeBase<CallForProposal, CommunityEntityMiniModel>();
            CreateMap<CallForProposal, CallForProposalDetailModel>()
                .ForMember(dst => dst.CFPDetailStats, opt => opt.MapFrom(src => src))
                .IncludeBase<CallForProposal, CallForProposalModel>();

            CreateMap<CallForProposal, CallForProposalUpsertModel>();
            CreateMap<CallForProposalUpsertModel, CallForProposal>();

            CreateMap<CallForProposal, CallForProposalStatModel>();
            CreateMap<CallForProposal, CallForProposalDetailStatModel>();

            //workspace
            CreateMap<Workspace, EntityMiniModel>()
                .IncludeBase<CommunityEntity, EntityMiniModel>();
            CreateMap<Workspace, CommunityEntityModel>()
                .IncludeBase<CommunityEntity, CommunityEntityModel>();
            CreateMap<Workspace, CommunityEntityMiniModel>()
                .IncludeBase<CommunityEntity, CommunityEntityMiniModel>();
            CreateMap<Workspace, CommunityEntityMiniDetailModel>()
                .IncludeBase<CommunityEntity, CommunityEntityMiniDetailModel>();
            CreateMap<Workspace, WorkspaceModel>()
                .IncludeBase<Workspace, CommunityEntityModel>();
            CreateMap<Workspace, WorkspaceDetailModel>()
                .ForMember(dst => dst.DetailStats, opt => opt.MapFrom(src => src))
                .IncludeBase<Workspace, WorkspaceModel>();

            CreateMap<Workspace, WorkspaceUpsertModel>();
            CreateMap<WorkspaceUpsertModel, Workspace>();

            //node
            CreateMap<Data.Node, EntityMiniModel>()
                .IncludeBase<CommunityEntity, EntityMiniModel>();
            CreateMap<Data.Node, CommunityEntityModel>()
                .IncludeBase<CommunityEntity, CommunityEntityModel>();
            CreateMap<Data.Node, CommunityEntityMiniModel>()
                .IncludeBase<CommunityEntity, CommunityEntityMiniModel>();
            CreateMap<Data.Node, CommunityEntityMiniDetailModel>()
                .IncludeBase<CommunityEntity, CommunityEntityMiniDetailModel>();
            CreateMap<Data.Node, NodeModel>()
                .IncludeBase<Data.Node, CommunityEntityModel>();
            CreateMap<Data.Node, NodeDetailModel>()
                .ForMember(dst => dst.DetailStats, opt => opt.MapFrom(src => src))
                .IncludeBase<Data.Node, NodeModel>();

            CreateMap<Data.Node, NodeUpsertModel>();
            CreateMap<NodeUpsertModel, Data.Node>();

            //organization
            CreateMap<Organization, EntityMiniModel>()
                .IncludeBase<CommunityEntity, EntityMiniModel>();
            CreateMap<Organization, CommunityEntityModel>()
                .IncludeBase<CommunityEntity, CommunityEntityModel>();
            CreateMap<Organization, CommunityEntityMiniModel>()
                .IncludeBase<CommunityEntity, CommunityEntityMiniModel>();
            CreateMap<Organization, CommunityEntityMiniDetailModel>()
                .IncludeBase<CommunityEntity, CommunityEntityMiniDetailModel>();
            CreateMap<Organization, OrganizationModel>()
                .IncludeBase<Organization, CommunityEntityModel>();
            CreateMap<Organization, OrganizationDetailModel>()
                .ForMember(dst => dst.DetailStats, opt => opt.MapFrom(src => src))
                .IncludeBase<Organization, OrganizationModel>();

            CreateMap<Organization, OrganizationUpsertModel>();
            CreateMap<OrganizationUpsertModel, Organization>();

            //onboarding
            CreateMap<OnboardingConfiguration, OnboardingConfigurationModel>();
            CreateMap<OnboardingPresentation, OnboardingPresentationModel>();
            CreateMap<OnboardingPresentationItem, OnboardingPresentationItemModel>()
                .ForMember(dst => dst.ImageUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.ImageId, true)));
            CreateMap<OnboardingQuestionnaire, OnboardingQuestionnaireModel>();
            CreateMap<OnboardingQuestionnaireItem, OnboardingQuestionnaireItemModel>();
            CreateMap<OnboardingRules, OnboardingRulesModel>();

            CreateMap<OnboardingConfigurationUpsertModel, OnboardingConfiguration>();
            CreateMap<OnboardingPresentationUpsertModel, OnboardingPresentation>();
            CreateMap<OnboardingPresentationItemUpsertModel, OnboardingPresentationItem>();
            CreateMap<OnboardingQuestionnaireModel, OnboardingQuestionnaire>();
            CreateMap<OnboardingQuestionnaireItemModel, OnboardingQuestionnaireItem>();
            CreateMap<OnboardingRulesModel, OnboardingRules>();

            CreateMap<OnboardingConfiguration, OnboardingConfigurationUpsertModel>();
            CreateMap<OnboardingPresentation, OnboardingPresentationUpsertModel>();
            CreateMap<OnboardingPresentationItem, OnboardingPresentationItemUpsertModel>();

            CreateMap<OnboardingQuestionnaireInstance, OnboardingQuestionnaireInstanceModel>();
            CreateMap<OnboardingQuestionnaireInstanceItem, OnboardingQuestionnaireInstanceItemModel>();

            CreateMap<OnboardingQuestionnaireInstanceUpsertModel, OnboardingQuestionnaireInstance>();
            CreateMap<OnboardingQuestionnaireInstanceItemModel, OnboardingQuestionnaireInstanceItem>();

            //cfp templates
            CreateMap<CallForProposalTemplate, CallForProposalTemplateModel>();
            CreateMap<CallForProposalTemplateSection, CallForProposalTemplateSectionModel>();
            CreateMap<CallForProposalTemplateQuestion, CallForProposalTemplateQuestionModel>();

            CreateMap<CallForProposalTemplateUpsertModel, CallForProposalTemplate>();
            CreateMap<CallForProposalTemplateSectionUpsertModel, CallForProposalTemplateSection>();
            CreateMap<CallForProposalTemplateQuestionUpsertModel, CallForProposalTemplateQuestion>();

            CreateMap<CallForProposalTemplate, CallForProposalTemplateUpsertModel>();
            CreateMap<CallForProposalTemplateSection, CallForProposalTemplateSectionUpsertModel>();
            CreateMap<CallForProposalTemplateQuestion, CallForProposalTemplateQuestionUpsertModel>();

            //proposal
            CreateMap<Proposal, ProposalModel>();
            CreateMap<ProposalAnswer, ProposalAnswerModel>();

            CreateMap<ProposalUpsertModel, Proposal>();
            CreateMap<ProposalAnswerUpsertModel, ProposalAnswer>();

            CreateMap<Proposal, ProposalUpsertModel>();
            CreateMap<ProposalAnswer, ProposalAnswerUpsertModel>();

            //user
            CreateMap<User, EntityMiniModel>()
              .ForMember(dst => dst.Fullname, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
              .ForMember(dst => dst.EntityType, opt => opt.MapFrom((src, dst, ctx) => FeedType.User))
              .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId)))
              .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId, true)))
              .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.AvatarId)))
              .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.AvatarId, true)));
            CreateMap<User, UserMiniModel>()
              .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId)))
              .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId, true)))
              .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.AvatarId)))
              .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.AvatarId, true)))
              .ForMember(dst => dst.Stats, opt => opt.MapFrom((src, dst, ctx) =>
              {
                  return new CommunityEntityStatModel
                  {
                      WorkspaceCount = src.CommunityCount,
                      NodeCount = src.NodeCount,
                      NeedCount = src.NeedCount,
                      OrganizationCount = src.OrganizationCount
                  };
              }));

            CreateMap<User, UserUpdateModel>();
            CreateMap<User, UserModel>()
              .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId)))
              .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId, true)))
              .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.AvatarId)))
              .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.AvatarId, true)))
              .ForMember(dst => dst.Stats, opt => opt.MapFrom((src, dst, ctx) =>
              {
                  return new CommunityEntityStatModel
                  {
                      NodeCount = src.NodeCount,
                      WorkspaceCount = src.CommunityCount,
                      NeedCount = src.NeedCount,
                      OrganizationCount = src.OrganizationCount
                  };
              }));

            CreateMap<UserCreateModel, User>();

            CreateMap<UserUpdateModel, User>();

            CreateMap<UserExperience, UserExperienceModel>();
            CreateMap<UserExperienceModel, UserExperience>();
            CreateMap<UserEducation, UserEducationModel>();
            CreateMap<UserEducationModel, UserEducation>();
            CreateMap<UserExternalAuth, UserExternalAuthModel>();
            CreateMap<UserExternalAuthModel, UserExternalAuth>();
            CreateMap<UserNotificationSettings, UserNotificationSettingsModel>();
            CreateMap<UserNotificationSettingsModel, UserNotificationSettings>();

            CreateMap<Link, LinkModel>();
            CreateMap<LinkModel, Link>();

            //feed entity visibility
            CreateMap<FeedEntityUserVisibility, FeedEntityUserVisibilityModel>();
            CreateMap<FeedEntityUserVisibilityUpsertModel, FeedEntityUserVisibility>();

            CreateMap<FeedEntityCommunityEntityVisibility, FeedEntityCommunityEntityVisibilityModel>();
            CreateMap<FeedEntityCommunityEntityVisibilityUpsertModel, FeedEntityCommunityEntityVisibility>();

            //documents
            CreateMap<Document, EntityMiniModel>()
               .ForMember(dst => dst.EntityType, opt => opt.MapFrom((src, dst, ctx) => FeedType.Document))
               .ForMember(dst => dst.Title, opt => opt.MapFrom((src, dst, ctx) => src.Name))
               .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.ImageId)))
               .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.ImageId, true)));

            CreateMap<Document, DocumentModel>()
                        .ForMember(dst => dst.Data, opt => opt.MapFrom(src => src.Data != null ? $"data:{src.Filetype};base64,{Convert.ToBase64String(src.Data)}" : null))
                        .ForMember(dst => dst.Filetype, opt => opt.MapFrom(src => src.Filetype))
                        .ForMember(dst => dst.DocumentUrl, opt => opt.MapFrom((src, dst, ctx) => GetDocumentUrl(src.Id.ToString())))
                        .ForMember(dst => dst.ImageUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.ImageId)))
                        .ForMember(dst => dst.ImageUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.ImageId, true)))
                        .ForMember(dst => dst.FeedStats, opt => opt.MapFrom((src, dst, ctx) => new FeedStatModel
                        {
                            PostCount = src.PostCount,
                            NewPostCount = src.NewPostCount,
                            NewMentionCount = src.NewMentionCount,
                            NewThreadActivityCount = src.NewThreadActivityCount,
                            CommentCount = src.CommentCount
                        }));

            CreateMap<Document, DocumentOrFolderModel>()
                        .ForMember(dst => dst.FeedStats, opt => opt.MapFrom((src, dst, ctx) => new FeedStatModel
                        {
                            PostCount = src.PostCount,
                            NewPostCount = src.NewPostCount,
                            NewMentionCount = src.NewMentionCount,
                            NewThreadActivityCount = src.NewThreadActivityCount,
                            CommentCount = src.CommentCount
                        }));

            CreateMap<DocumentInsertModel, Document>()
                        .ForMember(dst => dst.Data, opt => opt.MapFrom(src => src.Data != null ? Convert.FromBase64String(src.Data.Substring(src.Data.IndexOf(",") + 1)) : null))
                        .ForMember(dst => dst.Filetype, opt => opt.MapFrom(src => src.Data != null ? src.Data.Substring(5, src.Data.IndexOf(";") - 5) : null));
            CreateMap<DocumentUpdateModel, Document>();

            CreateMap<Folder, FolderModel>();
            CreateMap<Folder, DocumentOrFolderModel>()
                        .ForMember(dst => dst.IsFolder, opt => opt.MapFrom(src => true));
            CreateMap<FolderUpsertModel, Folder>();

            //events
            CreateMap<Event, EntityMiniModel>()
                .ForMember(dst => dst.EntityType, opt => opt.MapFrom((src, dst, ctx) => FeedType.Event))
                .ForMember(dst => dst.ShortDescription, opt => opt.MapFrom(src => src.Description))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId, true)));

            CreateMap<Event, EventModel>()
                .ForMember(dst => dst.Start, opt => opt.MapFrom((src, dst, ctx) => GetEventDateTimeLocal(src.Start, src.Timezone)))
                .ForMember(dst => dst.End, opt => opt.MapFrom((src, dst, ctx) => GetEventDateTimeLocal(src.End, src.Timezone)))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId, true)))
                .ForMember(dst => dst.FeedStats, opt => opt.MapFrom((src, dst, ctx) => new FeedStatModel
                {
                    PostCount = src.PostCount,
                    NewPostCount = src.NewPostCount,
                    NewMentionCount = src.NewMentionCount,
                    NewThreadActivityCount = src.NewThreadActivityCount,
                    CommentCount = src.CommentCount
                }));

            CreateMap<EventUpsertModel, Event>()
                .ForMember(dst => dst.Start, opt => opt.MapFrom((src, dst, ctx) => GetEventDateTimeUTC(src.Start, src.Timezone)))
                .ForMember(dst => dst.End, opt => opt.MapFrom((src, dst, ctx) => GetEventDateTimeUTC(src.End, src.Timezone)));

            CreateMap<EventAttendance, EventAttendanceModel>();
            CreateMap<EventAttendanceUpsertModel, EventAttendance>();

            //images
            CreateMap<Image, ImageModel>()
                        .ForMember(dst => dst.Data, opt => opt.MapFrom(src => $"data:{src.Filetype};base64,{Convert.ToBase64String(src.Data)}"))
                        .ForMember(dst => dst.Filetype, opt => opt.MapFrom(src => src.Filetype));

            CreateMap<Image, string>()
                        .ConstructUsing(src => GetUrl(src.Id.ToString(), true));

            CreateMap<ImageInsertModel, Image>()
                        .ForMember(dst => dst.Data, opt => opt.MapFrom(src => Convert.FromBase64String(src.Data.Substring(src.Data.IndexOf(",") + 1))))
                        .ForMember(dst => dst.Filetype, opt => opt.MapFrom(src => src.Data.Substring(5, src.Data.IndexOf(";") - 5)));


            CreateMap<ImageConversionUpsertModel, ImageConversion>();

            CreateMap<DocumentConversionUpsertModel, FileData>()
                        .ForMember(dst => dst.Data, opt => opt.MapFrom(src => Convert.FromBase64String(src.Data.Substring(src.Data.IndexOf(",") + 1))))
                        .ForMember(dst => dst.Filetype, opt => opt.MapFrom(src => src.Data.Substring(5, src.Data.IndexOf(";") - 5)));

            CreateMap<FileData, string>()
                        .ConstructUsing(src => $"data:{src.Filetype};base64,{Convert.ToBase64String(src.Data)}");

            CreateMap<byte[], string>()
                     .ConstructUsing(src => Convert.ToBase64String(src));

            CreateMap<string, FileData>()
                        .ForMember(dst => dst.Data, opt => opt.MapFrom(src => Convert.FromBase64String(src.Substring(src.IndexOf(",") + 1))))
                        .ForMember(dst => dst.Filetype, opt => opt.MapFrom(src => src.Substring(5, src.IndexOf(";") - 5)));

            //needs
            CreateMap<Need, EntityMiniModel>()
                .ForMember(dst => dst.EntityType, opt => opt.MapFrom((src, dst, ctx) => FeedType.Need))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.CommunityEntity?.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.CommunityEntity?.BannerId, true)))
                .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.CommunityEntity?.LogoId)))
                .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.CommunityEntity?.LogoId, true)));
            CreateMap<Need, NeedModel>()
                 .ForMember(dst => dst.FeedStats, opt => opt.MapFrom((src, dst, ctx) => new FeedStatModel
                 {
                     PostCount = src.PostCount,
                     NewPostCount = src.NewPostCount,
                     NewMentionCount = src.NewMentionCount,
                     NewThreadActivityCount = src.NewThreadActivityCount,
                     CommentCount = src.CommentCount
                 }));
            CreateMap<NeedUpsertModel, Need>();

            // membership
            CreateMap<User, MemberModel>()
             .ForMember(dst => dst.Stats, opt => opt.MapFrom((src, dst, ctx) =>
              {
                  return new CommunityEntityStatModel
                  {
                      WorkspaceCount = src.CommunityCount,
                      NodeCount = src.NodeCount,
                      NeedCount = src.NeedCount,
                  };
              }));

            CreateMap<Membership, MemberModel>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dst => dst.MembershipId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.User?.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.User?.BannerId, true)))
                .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.User?.AvatarId)))
                .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.User?.AvatarId, true)))
                .IncludeMembers(m => m.User);

            // invitation
            CreateMap<User, InvitationModelUser>();
            CreateMap<Invitation, InvitationModelUser>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.InviteeUserId))
                .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.InviteeEmail))
                .ForMember(dst => dst.InvitationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.User?.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.User?.BannerId, true)))
                .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.User?.AvatarId)))
                .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.User?.AvatarId, true)))
                .IncludeMembers(m => m.User);

            CreateMap<CommunityEntity, InvitationModelEntity>();
            CreateMap<Invitation, InvitationModelEntity>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.CommunityEntityId))
                .ForMember(dst => dst.InvitationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.EntityType, opt => opt.MapFrom(src => src.CommunityEntityType))
                .ForMember(dst => dst.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.Entity?.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.Entity?.BannerId, true)))
                .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.Entity?.LogoId)))
                .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.Entity?.LogoId, true)))
                .IncludeMembers(m => m.Entity);

            //community entity invitation
            CreateMap<CommunityEntity, CommunityEntityInvitationModelSource>();
            CreateMap<CommunityEntityInvitation, CommunityEntityInvitationModelSource>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.SourceCommunityEntityId))
                .ForMember(dst => dst.InvitationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.SourceCommunityEntity?.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.SourceCommunityEntity?.BannerId, true)))
                .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.SourceCommunityEntity?.LogoId)))
                .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.SourceCommunityEntity?.LogoId, true)))
                .IncludeMembers(m => m.SourceCommunityEntity);
            CreateMap<CommunityEntity, CommunityEntityInvitationModelTarget>();
            CreateMap<CommunityEntityInvitation, CommunityEntityInvitationModelTarget>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.TargetCommunityEntityId))
                .ForMember(dst => dst.InvitationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.BannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.TargetCommunityEntity?.BannerId)))
                .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.TargetCommunityEntity?.BannerId, true)))
                .ForMember(dst => dst.LogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.TargetCommunityEntity?.LogoId)))
                .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.TargetCommunityEntity?.LogoId, true)))
                .IncludeMembers(m => m.TargetCommunityEntity);

            CreateMap<Feed, FeedModel>();

            CreateMap<ContentEntity, ContentEntityModel>()
                .ForMember(dst => dst.ParentFeedEntity, opt => opt.MapFrom((src, dst, ctx) => (src.FeedEntity as Channel)?.CommunityEntity));
            CreateMap<ContentEntityOverrides, ContentEntityOverridesModel>();

            CreateMap<ContentEntityUpsertModel, ContentEntity>();
            CreateMap<ContentEntity, ContentEntityUpsertModel>();

            CreateMap<Reaction, ReactionModel>();
            CreateMap<Reaction, ReactionExtendedModel>();
            CreateMap<Reaction, ReactionUpsertModel>();
            CreateMap<ReactionUpsertModel, Reaction>();

            CreateMap<Comment, CommentModel>();
            CreateMap<Comment, CommentExtendedModel>();
            CreateMap<CommentOverrides, CommentOverridesModel>();

            CreateMap<CommentUpsertModel, Comment>();
            CreateMap<Comment, CommentUpsertModel>();

            CreateMap<Entity, ActivityRecordModel>()
                .ForMember(dst => dst.Comment, opt => opt.MapFrom((src, dst, obj, ctx) =>
                {
                    if (src is Comment)
                        return src;

                    else return null;
                }))
                .ForMember(dst => dst.Reaction, opt => opt.MapFrom((src, dst, obj, ctx) =>
                {
                    if (src is Reaction)
                        return src;

                    else return null;
                }))
               .ForMember(dst => dst.ContentEntity, opt => opt.MapFrom((src, dst, obj, ctx) =>
                {
                    if (src is ContentEntity)
                        return src;

                    else return null;
                }))
               .ForMember(dst => dst.CommunityEntity, opt => opt.MapFrom((src, dst, obj, ctx) =>
                {
                    if (src is Membership)
                        return (src as Membership).CommunityEntity;

                    else return null;
                }));

            CreateMap<TextValue, TextValueModel>();
            CreateMap<TextValueModel, TextValue>();

            CreateMap<Tag, TextValueModel>()
                .ForMember(dst => dst.Value, opt => opt.MapFrom(src => src.Text));
            CreateMap<TextValueModel, Tag>()
                .ForMember(dst => dst.Text, opt => opt.MapFrom(src => src.Value));

            //resources
            CreateMap<Resource, ResourceModel>()
                  .ForMember(dst => dst.ImageUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.ImageId)))
                  .ForMember(dst => dst.ImageUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.ImageId, true)));
            CreateMap<ResourceUpsertModel, Resource>();

            //portfolio items
            CreateMap<Paper, PortfolioItemModel>()
                  .ForMember(dst => dst.Type, opt => opt.MapFrom(src => PortfolioItemType.Paper))
                  .ForMember(dst => dst.FeedStats, opt => opt.MapFrom(src => new FeedStatModel { PostCount = src.PostCount }));
            CreateMap<Document, PortfolioItemModel>()
                  .ForMember(dst => dst.Type, opt => opt.MapFrom(src => PortfolioItemType.JoglDoc))
                  .ForMember(dst => dst.Summary, opt => opt.MapFrom(src => src.Description))
                  .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Name))
                  .ForMember(dst => dst.FeedStats, opt => opt.MapFrom(src => new FeedStatModel { PostCount = src.PostCount }));

            //papers
            CreateMap<Paper, EntityMiniModel>()
                .ForMember(dst => dst.EntityType, opt => opt.MapFrom((src, dst, ctx) => FeedType.Paper));
            CreateMap<Paper, PaperModel>()
                .ForMember(dst => dst.FeedCount, opt => opt.MapFrom((src, dst, ctx) => { return src.FeedIds?.Count() ?? 0; }))
                .ForMember(dst => dst.FeedStats, opt => opt.MapFrom((src, dst, ctx) => new FeedStatModel
                {
                    PostCount = src.PostCount,
                    NewPostCount = src.NewPostCount,
                    NewMentionCount = src.NewMentionCount,
                    NewThreadActivityCount = src.NewThreadActivityCount,
                    CommentCount = src.CommentCount
                }));
            CreateMap<PaperUpsertModel, Paper>();

            CreateMap<Work, PaperModelOrcid>()
                .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dst => dst.Abstract, opt => opt.MapFrom(src => src.Description))
                .ForMember(dst => dst.JournalTitle, opt => opt.MapFrom(src => src.JournalTitle))
                .ForMember(dst => dst.Authors, opt => opt.MapFrom(src => string.Join(", ", src.Contributors.Select(c => c.Name))))
                .ForMember(dst => dst.Type, opt => opt.MapFrom((src, dst, ctx) =>
                {
                    switch (src.WorkType)
                    {
                        case "preprint":
                            return PaperType.Preprint;
                        default:
                            return PaperType.Article;
                    }
                }))
                .ForMember(dst => dst.PublicationDate, opt => opt.MapFrom(src => src.PublicationDate))
                .ForMember(dst => dst.ExternalId, opt => opt.MapFrom((src, dst, ctx) => { return src.ExternalIds?.FirstOrDefault()?.UrlValue; }))
                .ForMember(dst => dst.ExternalUrl, opt => opt.MapFrom((src, dst, ctx) => { return src.ExternalIds?.FirstOrDefault()?.Url; }))
                .ForMember(dst => dst.Source, opt => opt.MapFrom(src => src.SourceName));

            CreateMap<SemanticScholar.DTO.SemanticPaper, PaperModelS2>()
               .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
               .ForMember(dst => dst.Journal, opt => opt.MapFrom((src, dst, ctx) => { return src.Journal?.Name; }))
               .ForMember(dst => dst.Abstract, opt => opt.MapFrom(src => src.Abstract))
               .ForMember(dst => dst.PublicationDate, opt => opt.MapFrom((src, dst, ctx) => { return src.PublicationDate ?? src.Year?.ToString(); }))
               .ForMember(dst => dst.Authors, opt => opt.MapFrom((src, dst, ctx) => { return FormatAuthors(src.Authors, a => a.Name); }))
               .ForMember(dst => dst.OpenAccessPdfUrl, opt => opt.MapFrom((src, dst, ctx) => { return src.OpenAccessPdf?.Url; }))
               .ForMember(dst => dst.ExternalId, opt => opt.MapFrom((src, dst, ctx) => { return src.ExternalIds.DOI; }));

            CreateMap<SemanticScholar.DTO.Author, PaperAuthorModelS2>()
              .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dst => dst.Papers, opt => opt.MapFrom(src => src.Papers));

            //works - OA
            CreateMap<OpenAlex.DTO.Work, PaperModelOA>()
                .ForMember(dst => dst.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dst => dst.Journal, opt => opt.MapFrom((src, dst, ctx) => { return src.PrimaryLocation?.Source?.DisplayName; }))
                .ForMember(dst => dst.Abstract, opt => opt.MapFrom((src, dst, ctx) => { return null == src.AbstractInvertedIndex ? "" : FormatOAWorkAbstract(src.AbstractInvertedIndex); }))
                .ForMember(dst => dst.PublicationDate, opt => opt.MapFrom((src, dst, ctx) => { return src.PublicationDate ?? src.PublicationYear?.ToString(); }))
                .ForMember(dst => dst.Authors, opt => opt.MapFrom((src, dst, ctx) => { return FormatAuthors(src.Authorships, a => a.Author.DisplayName); }))
                .ForMember(dst => dst.OpenAccessPdfUrl, opt => opt.MapFrom((src, dst, ctx) => { return src.PrimaryLocation?.PdfUrl ?? src.BestOaLocation?.PdfUrl; }))
                .ForMember(dst => dst.Tags, opt => opt.MapFrom((src, dst, ctx) => { return src.Keywords.Select(kw => kw.DisplayName); }))
                .ForMember(dst => dst.ExternalId, opt => opt.MapFrom((src, dst, ctx) => { return src.Doi?.Split(".org/")[1]; }))
                .ForMember(dst => dst.ExternalIdUrl, opt => opt.MapFrom((src, dst, ctx) => { return src.Doi; }));

            //articles - PM
            CreateMap<PubMed.DTO.PubmedArticle, PaperModelPM>()
              .ForMember(dst => dst.Title, opt => opt.MapFrom((src, dst, ctx) => { return src.MedlineCitation?.Article?.ArticleTitle?.Text; }))
              .ForMember(dst => dst.Journal, opt => opt.MapFrom((src, dst, ctx) => { return src.MedlineCitation?.Article?.Journal?.Title; }))
              .ForMember(dst => dst.Abstract, opt => opt.MapFrom((src, dst, ctx) => { return FormatPMArticleAbstract(src.MedlineCitation?.Article?.Abstract?.AbstractText); }))
              .ForMember(dst => dst.PublicationDate, opt => opt.MapFrom((src, dst, ctx) => { return $"{src.MedlineCitation?.Article?.ArticleDate?.Year}-{src.MedlineCitation?.Article?.ArticleDate?.Month}-{src.MedlineCitation?.Article?.ArticleDate?.Day}"; }))
              .ForMember(dst => dst.Authors, opt => opt.MapFrom((src, dst, ctx) => { return FormatAuthors(src.MedlineCitation?.Article?.AuthorList?.Author, a => $"{a?.ForeName} {a?.LastName}"); }))
              .ForMember(dst => dst.ExternalId, opt => opt.MapFrom((src, dst, ctx) => { return src.PubmedData?.ArticleIdList?.ArticleId?.FirstOrDefault(id => id.IdType == "doi")?.Text; }));

            //notifications
            CreateMap<Notification, NotificationModel>();
            CreateMap<NotificationData, NotificationDataModel>()
                .ForMember(dst => dst.EntityLogoUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.EntityLogoId)))
                .ForMember(dst => dst.EntityLogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.EntityLogoId, true)))
                .ForMember(dst => dst.EntityBannerUrl, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.EntityBannerId)))
                .ForMember(dst => dst.EntityBannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.EntityBannerId, true)));

            //feed integrations
            CreateMap<FeedIntegrationUpsertModel, FeedIntegration>();
            CreateMap<FeedIntegration, FeedIntegrationModel>();

            //user waitlist records
            CreateMap<WaitlistRecordModel, WaitlistRecord>();

            //user contact records
            CreateMap<UserContactModel, UserContact>();

            //user feed records
            CreateMap<UserFeedRecord, UserFeedRecordModel>();
            CreateMap<UserFeedRecordModel, UserFeedRecord>();

            CreateMap<NodeFeedData, NodeFeedDataModelNew>()
            .ForMember(dst => dst.Type, opt => opt.MapFrom((src, dst, ctx) => FeedType.Node))
            .ForMember(dst => dst.BannerUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.BannerId, true)))
            .ForMember(dst => dst.LogoUrlSmall, opt => opt.MapFrom((src, dst, ctx) => GetUrl(src.LogoId, true)))
            .ForMember(dst => dst.Access, opt => opt.MapFrom((src, dst, ctx) =>
            {
                return new CommunityEntityPermissionModel
                {
                    Permissions = src.Permissions
                };
            }));
            CreateMap<DiscussionStats, DiscussionStatModel>();
            CreateMap<Discussion, DiscussionModel>()
                .ForMember(dst => dst.UnreadMentions, opt => opt.MapFrom(src => src.DiscussionStats.UnreadMentions))
                .ForMember(dst => dst.UnreadPosts, opt => opt.MapFrom(src => src.DiscussionStats.UnreadPosts))
                .ForMember(dst => dst.UnreadThreads, opt => opt.MapFrom(src => src.DiscussionStats.UnreadThreads));

            CreateMap<Membership, MembershipModel>();
            CreateMap<AccessOrigin, AccessOriginModel>();

            //channels
            CreateMap<Channel, EntityMiniModel>()
                .ForMember(dst => dst.EntityType, opt => opt.MapFrom((src, dst, ctx) => FeedType.Channel));
            CreateMap<Channel, ChannelModel>()
                .ForMember(dst => dst.Stats, opt => opt.MapFrom(src => src));
            CreateMap<Channel, ChannelExtendedModel>()
              .IncludeBase<Channel, ChannelModel>();
            CreateMap<Channel, ChannelDetailModel>()
            .IncludeBase<Channel, ChannelModel>();
            CreateMap<ChannelUpsertModel, Channel>();
            CreateMap<Channel, ChannelStatModel>();

            CreateMap<ChannelMemberUpsertModel, Membership>();

            CreateMap<AccessLevel, SimpleAccessLevel>();
            CreateMap<SimpleAccessLevel, AccessLevel>();
        }

        private DateTime GetEventDateTimeUTC(DateTime date, TimezoneModel timezone)
        {
            if (timezone == null)
                return date;

            if (date.Kind != DateTimeKind.Unspecified)
                return date;

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, timezone.Value, "utc");
        }

        private DateTime GetEventDateTimeLocal(DateTime date, Timezone timezone)
        {
            if (timezone == null)
                return date;

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, timezone.Value);
        }

        protected string GetUrl(string id, bool tn = false)
        {
            if (string.IsNullOrEmpty(id))
                return id;

            var req = _httpContextAccessor.HttpContext.Request;
            if (tn)
                return req.Scheme + "://" + req.Host + "/images/" + id + "/tn";

            return req.Scheme + "://" + req.Host + "/images/" + id + "/full";
        }

        protected string GetDocumentUrl(string id)
        {
            if (string.IsNullOrEmpty(id))
                return id;

            var req = _httpContextAccessor.HttpContext.Request;
            return req.Scheme + "://" + req.Host + "/documents/" + id + "/download";
        }

        private string GetUserAccessLevel(AccessLevel? level)
        {
            if (level == null)
                return "visitor";

            return level.ToString().ToLower();
        }


        protected string FormatAuthors<T>(List<T> authors, Func<T, string> formatFunction)
        {
            return authors == null ? null : string.Join(", ", authors.Select(formatFunction));
        }

        protected string FormatOAWorkAbstract(Dictionary<string, List<int>> invertedIndex)
        {
            StringBuilder stringBuilder = new();
            int maxPosition = invertedIndex.Values.SelectMany(positions => positions).Max();

            for (int position = 0; position <= maxPosition; position++)
            {
                var wordAtPosition = invertedIndex
                    .Where(wordInfo => wordInfo.Value.Contains(position))
                    .Select(wordInfo => wordInfo.Key)
                    .SingleOrDefault();

                stringBuilder.Append($"{wordAtPosition} ");
            }

            return stringBuilder.ToString().Trim();
        }

        protected string FormatPMArticleAbstract(List<PubMed.DTO.AbstractText> abstractFragments)
        {
            if (abstractFragments?.Count > 1)
                return string.Join("\n", abstractFragments.Select(at => $"{at?.Label}: {at?.Text}"));

            return abstractFragments?.First()?.Text;
        }
    }
}
