using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;

namespace Jogl.Server.Business
{
    public abstract class BaseService
    {
        protected readonly IUserFollowingRepository _followingRepository;
        protected readonly IMembershipRepository _membershipRepository;
        protected readonly IInvitationRepository _invitationRepository;
        protected readonly INeedRepository _needRepository;
        protected readonly IDocumentRepository _documentRepository;
        protected readonly IPaperRepository _paperRepository;
        protected readonly IResourceRepository _resourceRepository;
        protected readonly IRelationRepository _relationRepository;
        protected readonly ICallForProposalRepository _callForProposalsRepository;
        protected readonly IProposalRepository _proposalRepository;
        protected readonly IContentEntityRepository _contentEntityRepository;
        protected readonly ICommentRepository _commentRepository;
        protected readonly IMentionRepository _mentionRepository;
        protected readonly IReactionRepository _reactionRepository;
        protected readonly IFeedRepository _feedRepository;
        protected readonly IUserContentEntityRecordRepository _userContentEntityRecordRepository;
        protected readonly IUserFeedRecordRepository _userFeedRecordRepository;
        protected readonly IEventRepository _eventRepository;
        protected readonly IEventAttendanceRepository _eventAttendanceRepository;
        protected readonly IUserRepository _userRepository;
        protected readonly IChannelRepository _channelRepository;
        protected readonly IFeedEntityService _feedEntityService;

        public BaseService(IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService)
        {
            _followingRepository = followingRepository;
            _membershipRepository = membershipRepository;
            _invitationRepository = invitationRepository;
            _relationRepository = relationRepository;
            _needRepository = needRepository;
            _documentRepository = documentRepository;
            _paperRepository = paperRepository;
            _resourceRepository = resourceRepository;
            _proposalRepository = proposalRepository;
            _callForProposalsRepository = callForProposalsRepository;
            _contentEntityRepository = contentEntityRepository;
            _commentRepository = commentRepository;
            _mentionRepository = mentionRepository;
            _reactionRepository = reactionRepository;
            _feedRepository = feedRepository;
            _userContentEntityRecordRepository = userContentEntityRecordRepository;
            _userFeedRecordRepository = userFeedRecordRepository;
            _eventRepository = eventRepository;
            _eventAttendanceRepository = eventAttendanceRepository;
            _userRepository = userRepository;
            _channelRepository = channelRepository;
            _feedEntityService = feedEntityService;
        }

        protected const string NEEDS_MEMBER_POST = "needs_created_by_any_member";
        protected const string LIBRARY_MEMBER_MANAGE = "library_managed_by_any_member";
        protected const string DOCUMENTS_MEMBER_POST = "documents_created_by_any_member";
        //protected const string RESOURCE_MEMBER_POST = "resources_created_by_any_member";
        protected const string EVENT_MEMBER_POST = "events_created_by_any_member";
        protected const string WORKSPACE_MEMBER_CREATION = "workspace_created_by_any_member";
        protected const string CONTENT_MEMBER_POST = "content_entities_created_by_any_member";
        protected const string CHANNEL_MEMBER_POST = "channels_created_by_any_member";
        protected const string COMMENT_MEMBER_POST = "comments_created_by_any_member";
        protected const string MENTION_EVERYONE_MEMBER = "mention_everyone_by_any_member";
        protected const string INVITE_MEMBERS_MEMBER = "members_invited_by_any_member";

        protected const string LABEL_REVIEWER = "reviewer";
        protected const string LABEL_SPEAKER = "speaker";

        protected bool Can(CommunityEntity entity, IEnumerable<Membership> memberships, IEnumerable<Relation> relations, Permission permission)
        {
            var membership = memberships.SingleOrDefault(m => m.CommunityEntityId == entity.Id.ToString());
            switch (permission)
            {
                case Permission.Read:
                    return IsReadable(entity, memberships, relations);
                case Permission.Manage:
                case Permission.PostResources:
                    return membership != null && (membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.ManageOwners:
                case Permission.Delete:
                    return membership != null && membership.AccessLevel == AccessLevel.Owner;
                case Permission.PostContentEntity:
                case Permission.PostComment:
                    return membership != null && (entity.Settings?.Contains(CONTENT_MEMBER_POST) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.CreateChannels:
                    return membership != null && (entity.Settings?.Contains(CHANNEL_MEMBER_POST) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.CreateWorkspaces:
                    var userCanCreateWorkspaces = membership != null && (entity.Settings?.Contains(WORKSPACE_MEMBER_CREATION) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                    switch (entity.Type)
                    {
                        case CommunityEntityType.CallForProposal:
                            return false;
                        case CommunityEntityType.Workspace:
                            var isTopLevelWorkspace = relations.Any(r => r.SourceCommunityEntityId == entity.Id.ToString() && r.TargetCommunityEntityType == CommunityEntityType.Node);
                            return isTopLevelWorkspace && userCanCreateWorkspaces;
                        default:
                            return userCanCreateWorkspaces;
                    }
                case Permission.PostNeed:
                    return membership != null && (entity.Settings?.Contains(NEEDS_MEMBER_POST) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.ManageLibrary:
                    return membership != null && (entity.Settings?.Contains(LIBRARY_MEMBER_MANAGE) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.ManageDocuments:
                    return membership != null && (entity.Settings?.Contains(DOCUMENTS_MEMBER_POST) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.MentionEveryone:
                    return membership != null && (entity.Settings?.Contains(MENTION_EVERYONE_MEMBER) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.CreateEvents:
                    return membership != null && (entity.Settings?.Contains(EVENT_MEMBER_POST) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.ScoreProposals:
                    switch (entity.Type)
                    {
                        case CommunityEntityType.CallForProposal:
                            return membership != null && (membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                        default:
                            return false;
                    };
                case Permission.ListProposals:
                case Permission.CreateProposals:
                    switch (entity.Type)
                    {
                        case CommunityEntityType.Workspace:
                            return membership != null;
                        default:
                            return false;
                    };
                case Permission.DeleteContentEntity:
                case Permission.DeleteComment:
                    return membership != null && (membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.Join:
                    if (entity.JoiningRestrictionLevel != JoiningRestrictionLevel.Open)
                        return false;

                    switch (entity.Type)
                    {
                        case CommunityEntityType.Node:
                            return true;
                        default:
                            return relations.Any(r => r.SourceCommunityEntityId == entity.Id.ToString() && memberships.Any(m => m.CommunityEntityId == r.TargetCommunityEntityId));
                    }
                default:

                    return false;
            }
        }

        protected bool Can(Event e, IEnumerable<EventAttendance> attendances, IEnumerable<Membership> currentUserMemberships, string userId, Permission permission)
        {
            var attendance = attendances.FirstOrDefault(a => a.EventId == e.Id.ToString() && a.UserId == userId && !string.IsNullOrEmpty(a.UserId));
            switch (permission)
            {
                case Permission.Read:
                    return CanSeeEvent(e, currentUserMemberships, attendances, userId);
                case Permission.PostContentEntity:
                case Permission.PostComment:
                    return attendance != null;
                case Permission.Manage:
                case Permission.ManageDocuments:
                case Permission.DeleteContentEntity:
                case Permission.DeleteComment:
                case Permission.MentionEveryone:
                    return attendance != null && attendance.AccessLevel == AttendanceAccessLevel.Admin;
                case Permission.Delete:
                    return e.CreatedByUserId == userId && !string.IsNullOrEmpty(e.CreatedByUserId);
                default:
                    return false;
            }
        }

        private FeedEntityVisibility? GetFeedEntityVisibility(FeedEntity feedEntity, IEnumerable<Membership> memberships, string userId)
        {
            if (feedEntity.CreatedByUserId == userId && !string.IsNullOrEmpty(userId))
                return FeedEntityVisibility.Edit;

            var defaultVisibility = feedEntity.DefaultVisibility;
            var userVisibility = feedEntity.UserVisibility?.FirstOrDefault(u => u.UserId == userId)?.Visibility;
            var communityEntityVisibility = feedEntity.CommunityEntityVisibility?.FirstOrDefault(u => memberships.Any(m => m.CommunityEntityId == u.CommunityEntityId && m.UserId == userId))?.Visibility; //TODO fix case for multiple CE memberships

            var visibilities = new List<FeedEntityVisibility?> { defaultVisibility, userVisibility, communityEntityVisibility }.Where(v => v != null).ToList();
            if (!visibilities.Any())
                return null;

            var visibility = visibilities.Max();
            if (visibility > FeedEntityVisibility.View && string.IsNullOrEmpty(userId))
                visibility = FeedEntityVisibility.View;

            return visibility;
        }

        protected bool Can(Document doc, FeedEntity feedEntity, IEnumerable<Membership> currentUserMemberships, IEnumerable<EventAttendance> currentUserEventAttendances, IEnumerable<Relation> entityRelations, string userId, Permission permission)
        {
            var membership = currentUserMemberships.SingleOrDefault(m => m.CommunityEntityId == feedEntity?.Id.ToString());
            var attendance = currentUserEventAttendances.SingleOrDefault(ea => ea.EventId == feedEntity?.Id.ToString());

            switch (doc.Type)
            {
                case DocumentType.JoglDoc:
                    var docVisibility = GetFeedEntityVisibility(doc, currentUserMemberships, userId);
                    switch (permission)
                    {
                        case Permission.Manage:
                        case Permission.ManageDocuments:
                        case Permission.Delete:
                        case Permission.DeleteContentEntity:
                        case Permission.DeleteComment:
                            return docVisibility == FeedEntityVisibility.Edit;
                        case Permission.PostContentEntity:
                        case Permission.PostComment:
                            return docVisibility == FeedEntityVisibility.Edit || docVisibility == FeedEntityVisibility.Comment;
                        case Permission.Read:
                            return docVisibility != null;
                        default:
                            return false;
                    }
                default:
                    if (feedEntity == null)
                        return false;

                    switch (permission)
                    {
                        case Permission.Manage:
                        case Permission.Delete:
                            switch (feedEntity.FeedType)
                            {
                                case FeedType.Need:
                                case FeedType.Document:
                                    var parentDocVisibility = GetFeedEntityVisibility(feedEntity, currentUserMemberships, userId);
                                    return parentDocVisibility == FeedEntityVisibility.Edit;
                                case FeedType.Event:
                                    return attendance != null && attendance.AccessLevel == AttendanceAccessLevel.Admin;
                                default:
                                    var communityEntity = feedEntity as CommunityEntity;
                                    if (communityEntity == null)
                                        return false;

                                    return Can(communityEntity, currentUserMemberships, entityRelations, Permission.ManageDocuments);
                            }
                        case Permission.Read:
                            return true;
                        default:
                            return false;
                    }
            }
        }

        protected bool Can(FeedEntity e, IEnumerable<Membership> currentUserMemberships, string userId, Permission permission)
        {
            var docVisibility = GetFeedEntityVisibility(e, currentUserMemberships, userId);
            switch (permission)
            {
                case Permission.Manage:
                case Permission.ManageDocuments:
                case Permission.Delete:
                case Permission.DeleteContentEntity:
                case Permission.DeleteComment:
                    return docVisibility == FeedEntityVisibility.Edit;
                case Permission.PostContentEntity:
                case Permission.PostComment:
                    return docVisibility == FeedEntityVisibility.Edit || docVisibility == FeedEntityVisibility.Comment;
                case Permission.Read:
                    return docVisibility != null;
                default:
                    return false;
            }
        }

        protected bool Can(Channel c, Membership? membership, Membership? communityEntityMembership, Permission permission)
        {
            switch (permission)
            {
                case Permission.Read:
                    return membership != null || c.Visibility == ChannelVisibility.Open;
                case Permission.PostComment:
                    return membership != null && (c.Settings?.Contains(COMMENT_MEMBER_POST) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.PostContentEntity:
                    return membership != null && (c.Settings?.Contains(CONTENT_MEMBER_POST) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.MentionEveryone:
                    return membership != null && (c.Settings?.Contains(MENTION_EVERYONE_MEMBER) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.InviteMembers:
                    return membership != null && (c.Settings?.Contains(INVITE_MEMBERS_MEMBER) == true || membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.Manage:
                case Permission.ManageDocuments:
                case Permission.DeleteContentEntity:
                case Permission.DeleteComment:
                    return membership != null && (membership.AccessLevel == AccessLevel.Admin || membership.AccessLevel == AccessLevel.Owner);
                case Permission.Delete:
                    return membership != null && membership.AccessLevel == AccessLevel.Admin;
                case Permission.Join:
                    return communityEntityMembership != null;
                default:
                    return false;
            }
        }

        [Obsolete]
        protected IEnumerable<string> GetLinkedEntityIds(CommunityEntity entity)
        {
            switch (entity.Type)
            {
                case CommunityEntityType.Project:
                    return _relationRepository
                        .List(r => r.SourceCommunityEntityId == entity.Id.ToString() && !r.Deleted)
                        .Select(c => c.TargetCommunityEntityId);
                default:
                    return new List<string>();
            }
        }

        protected Dictionary<string, CommunityEntityType> CollectCommunityEntityTypeDictionary(Feed feed)
        {
            var res = new Dictionary<string, CommunityEntityType> { };
            CommunityEntityType type;
            if (Enum.TryParse(feed.Type.ToString(), true, out type))
                res.Add(feed.Id.ToString(), type);

            return res;
        }

        protected List<string> GetNodeIdsForCommunity(IEnumerable<Relation> allRelations, string communityId)
        {
            return GetNodeIdsForCommunities(allRelations, new List<string> { communityId });
        }

        protected List<string> GetNodeIdsForCommunities(IEnumerable<Relation> allRelations, IEnumerable<string> communityIds)
        {
            return allRelations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Node && r.SourceCommunityEntityType == CommunityEntityType.Workspace && communityIds.Contains(r.SourceCommunityEntityId))
                 .Select(r => r.TargetCommunityEntityId)
                 .ToList();
        }

        protected List<string> GetNodeIdsForMemberships(IEnumerable<Relation> allRelations, IEnumerable<Membership> currentUserMemberships)
        {
            var nodeIds = currentUserMemberships.Where(m => m.CommunityEntityType == CommunityEntityType.Node).Select(m => m.CommunityEntityId);
            var workspaceNodeIds = allRelations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Node && r.SourceCommunityEntityType == CommunityEntityType.Workspace && currentUserMemberships.Any(m => m.CommunityEntityId == r.SourceCommunityEntityId))
                 .Select(r => r.TargetCommunityEntityId);

            var subWorkspaceNodeIds = allRelations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Workspace && r.SourceCommunityEntityType == CommunityEntityType.Workspace && allRelations.Any(r2 => r2.SourceCommunityEntityId == r.TargetCommunityEntityId && r2.TargetCommunityEntityType == CommunityEntityType.Node) && currentUserMemberships.Any(m => m.CommunityEntityId == r.SourceCommunityEntityId))
                 .Select(r => r.TargetCommunityEntityId);


            return nodeIds.Concat(workspaceNodeIds).Concat(subWorkspaceNodeIds).ToList();
        }

        protected List<string> GetCommunityEntityIdsForNode(IEnumerable<Relation> allRelations, string nodeId)
        {
            return GetCommunityEntityIdsForNodes(allRelations, new List<string> { nodeId });
        }

        protected List<string> GetCommunityEntityIdsForNode(string nodeId)
        {
            var relations = _relationRepository.List(r => !r.Deleted);
            return GetCommunityEntityIdsForNode(relations, nodeId);
        }

        protected List<string> GetCommunityEntityIdsForNodes(IEnumerable<Relation> allRelations, List<string> nodeIds)
        {
            var directLinkCommunityIds = allRelations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Node && r.SourceCommunityEntityType == CommunityEntityType.Workspace && nodeIds.Contains(r.TargetCommunityEntityId))
                .Select(r => r.SourceCommunityEntityId);

            var indirectLinkIds = allRelations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Workspace && directLinkCommunityIds.Contains(r.TargetCommunityEntityId))
                .Select(r => r.SourceCommunityEntityId);

            return directLinkCommunityIds.Concat(indirectLinkIds).Concat(nodeIds)
              .Distinct()
              .ToList();
        }

        protected List<string> GetFeedEntityIdsForNode(IEnumerable<Relation> allRelations, IEnumerable<Event> allEvents, string nodeId)
        {
            return GetFeedEntityIdsForNodes(allRelations, allEvents, new List<string> { nodeId });
        }

        protected List<string> GetFeedEntityIdsForNode(string nodeId)
        {
            var relations = _relationRepository.List(r => !r.Deleted);
            var communityEntityIds = GetCommunityEntityIdsForNode(relations, nodeId);
            var events = _eventRepository.List(e => communityEntityIds.Contains(e.CommunityEntityId) && !e.Deleted);
            var eventIds = events.Select(e => e.Id.ToString()).ToList();

            return communityEntityIds
              .Concat(eventIds)
              .Distinct()
              .ToList();
        }

        protected List<string> GetFeedEntityIdsForNodes(IEnumerable<Relation> allRelations, IEnumerable<Event> allEvents, List<string> nodeIds)
        {
            var communityEntityIds = GetCommunityEntityIdsForNodes(allRelations, nodeIds);
            var eventIds = allEvents.Where(e => communityEntityIds.Contains(e.CommunityEntityId)).Select(e => e.Id.ToString());

            return communityEntityIds
                .Concat(eventIds)
                .Distinct()
                .ToList();
        }

        protected List<string> GetCommunityEntityIdsForOrg(IEnumerable<Relation> allRelations, string orgId)
        {
            return GetCommunityEntityIdsForOrgs(allRelations, new List<string> { orgId });
        }

        protected List<string> GetCommunityEntityIdsForOrg(string orgId)
        {
            var relations = _relationRepository.List(r => r.TargetCommunityEntityType == CommunityEntityType.Organization && r.TargetCommunityEntityId == orgId && !r.Deleted);
            return GetCommunityEntityIdsForNode(relations, orgId);
        }

        protected List<string> GetCommunityEntityIdsForOrgs(IEnumerable<Relation> allRelations, List<string> orgIds)
        {
            return allRelations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Organization && orgIds.Contains(r.TargetCommunityEntityId))
                .Select(r => r.SourceCommunityEntityId)
                .Concat(orgIds)
                .Distinct()
                .ToList();
        }

        protected List<string> GetFeedEntityIdsForOrg(IEnumerable<Relation> allRelations, IEnumerable<Event> allEvents, string orgId)
        {
            return GetFeedEntityIdsForOrgs(allRelations, allEvents, new List<string> { orgId });
        }

        protected List<string> GetFeedEntityIdsForOrgs(IEnumerable<Relation> allRelations, IEnumerable<Event> allEvents, List<string> orgIds)
        {
            var communityEntityIds = GetCommunityEntityIdsForOrgs(allRelations, orgIds);
            var eventIds = allEvents.Where(e => communityEntityIds.Contains(e.CommunityEntityId)).Select(e => e.Id.ToString());

            return communityEntityIds
                .Concat(eventIds)
                .Distinct()
                .ToList();
        }

        //protected List<CommunityEntity> GetEcosystemForOrganizations(IProjectRepository projectRepository, IWorkspaceRepository workspaceRepository, INodeRepository nodeRepository, ICallForProposalRepository callForProposalsRepository, IOrganizationRepository organizationRepository, string organizationId, string currentUserId, IEnumerable<CommunityEntityType> types)
        //{
        //    if (!types.Any())
        //        types = Enum.GetValues<CommunityEntityType>();

        //    var res = new List<CommunityEntity>();

        //    if (types.Contains(CommunityEntityType.Project))
        //    {
        //        var projectIds = _relationRepository.List(r => r.TargetCommunityEntityId == organizationId && r.SourceCommunityEntityType == CommunityEntityType.Project && !r.Deleted)
        //          .Select(pn => pn.SourceCommunityEntityId)
        //          .ToList();

        //        var projects = projectRepository.Get(projectIds);
        //        var filteredProjects = GetFilteredProjects(projects, currentUserId);
        //        res.AddRange(filteredProjects);
        //    }

        //    if (types.Contains(CommunityEntityType.Workspace))
        //    {
        //        var communityIds = _relationRepository.List(r => r.TargetCommunityEntityId == organizationId && r.SourceCommunityEntityType == CommunityEntityType.Workspace && !r.Deleted)
        //          .Select(pn => pn.SourceCommunityEntityId)
        //          .ToList();

        //        var communities = workspaceRepository.Get(communityIds);
        //        var filteredCommunities = GetFilteredWorkspaces(communities, currentUserId);
        //        res.AddRange(filteredCommunities);
        //    }

        //    if (types.Contains(CommunityEntityType.Node))
        //    {
        //        var nodeIds = _relationRepository.List(r => r.TargetCommunityEntityId == organizationId && r.SourceCommunityEntityType == CommunityEntityType.Node && !r.Deleted)
        //          .Select(pn => pn.SourceCommunityEntityId)
        //          .ToList();

        //        var nodes = nodeRepository.Get(nodeIds);
        //        var filteredNodes = GetFilteredNodes(nodes, currentUserId);
        //        res.AddRange(filteredNodes);
        //    }

        //    if (types.Contains(CommunityEntityType.CallForProposal))
        //    {
        //        var communityIds = _relationRepository.List(r => r.TargetCommunityEntityId == organizationId && r.SourceCommunityEntityType == CommunityEntityType.Workspace && !r.Deleted)
        //          .Select(pn => pn.SourceCommunityEntityId)
        //          .ToList();

        //        var communities = workspaceRepository.Get(communityIds);
        //        var cfps = callForProposalsRepository.ListForCommunityIds(communityIds);
        //        var filteredCommunities = GetFilteredCallForProposals(cfps, communities, currentUserId);
        //        res.AddRange(filteredCommunities);
        //    }

        //    if (types.Contains(CommunityEntityType.Organization))
        //    {
        //        var organizationIds = new List<string> { organizationId };
        //        var organizations = organizationRepository.Get(organizationIds);
        //        var filteredOrganizations = GetFilteredOrganizations(organizations, currentUserId);

        //        res.AddRange(filteredOrganizations);
        //    }

        //    return res;
        //}

        protected List<string> GetCommunityEntityIdsForCommunity(IEnumerable<Relation> allRelations, string communityId)
        {
            return GetCommunityEntityIdsForCommunities(allRelations, new List<string> { communityId });
        }

        protected List<string> GetCommunityEntityIdsForCommunity(string communityId)
        {
            var relations = _relationRepository.List(r => r.TargetCommunityEntityType == CommunityEntityType.Workspace && r.TargetCommunityEntityId == communityId && !r.Deleted);
            return GetCommunityEntityIdsForCommunity(relations, communityId);
        }

        protected List<string> GetCommunityEntityIdsForCommunities(IEnumerable<Relation> allRelations, List<string> communityIds)
        {
            return allRelations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Workspace && communityIds.Contains(r.TargetCommunityEntityId))
                .Select(r => r.SourceCommunityEntityId)
                .Concat(communityIds)
                .Distinct()
                .ToList();
        }

        protected List<string> GetFeedEntityIdsForCommunity(IEnumerable<Relation> allRelations, IEnumerable<Event> allEvents, string communityId)
        {
            return GetFeedEntityIdsForCommunities(allRelations, allEvents, new List<string> { communityId });
        }


        protected List<string> GetFeedEntityIdsForCommunities(IEnumerable<Relation> allRelations, IEnumerable<Event> allEvents, List<string> communityIds)
        {
            var communityEntityIds = GetCommunityEntityIdsForCommunities(allRelations, communityIds);
            var eventIds = allEvents.Where(e => communityEntityIds.Contains(e.CommunityEntityId)).Select(e => e.Id.ToString());

            return communityEntityIds
                .Concat(eventIds)
                .Distinct()
                .ToList();
        }

        //protected List<CommunityEntity> GetEcosystemForCommunity(IProjectRepository projectRepository, IWorkspaceRepository workspaceRepository, string communityId, string currentUserId, IEnumerable<CommunityEntityType> types)
        //{
        //    if (!types.Any())
        //        types = Enum.GetValues<CommunityEntityType>();

        //    var res = new List<CommunityEntity>();

        //    if (types.Contains(CommunityEntityType.Project))
        //    {
        //        var projectIds = _relationRepository.List(r => r.TargetCommunityEntityId == communityId && r.SourceCommunityEntityType == CommunityEntityType.Project && !r.Deleted)
        //          .Select(pn => pn.SourceCommunityEntityId)
        //          .ToList();

        //        var projects = projectRepository.Get(projectIds);
        //        var filteredProjects = GetFilteredProjects(projects, currentUserId);
        //        res.AddRange(filteredProjects);
        //    }

        //    if (types.Contains(CommunityEntityType.Workspace))
        //    {
        //        var communityIds = new List<string> { communityId };
        //        var communities = workspaceRepository.Get(communityIds);
        //        var filteredCommunities = GetFilteredWorkspaces(communities, currentUserId);

        //        res.AddRange(filteredCommunities);
        //    }

        //    return res;
        //}

        //protected List<CommunityEntity> GetEcosystemForUser(IProjectRepository projectRepository, IWorkspaceRepository workspaceRepository, INodeRepository nodeRepository, string targetUserId, string currentUserId, IEnumerable<CommunityEntityType> types)
        //{
        //    if (!types.Any())
        //        types = Enum.GetValues<CommunityEntityType>();

        //    var res = new List<CommunityEntity>();

        //    if (types.Contains(CommunityEntityType.Project))
        //    {
        //        var projectIds = _membershipRepository.List(m => m.UserId == targetUserId && m.CommunityEntityType == CommunityEntityType.Project && !m.Deleted)
        //          .Select(pm => pm.CommunityEntityId)
        //          .ToList();

        //        var projects = projectRepository.Get(projectIds);
        //        var filteredProjects = GetFilteredProjects(projects, currentUserId);
        //        res.AddRange(filteredProjects);
        //    }

        //    if (types.Contains(CommunityEntityType.Workspace))
        //    {
        //        var communityIds = _membershipRepository.List(m => m.UserId == targetUserId && m.CommunityEntityType == CommunityEntityType.Workspace && !m.Deleted)
        //          .Select(cm => cm.CommunityEntityId)
        //          .ToList();

        //        var communities = workspaceRepository.Get(communityIds);
        //        var filteredCommunities = GetFilteredWorkspaces(communities, currentUserId);

        //        res.AddRange(filteredCommunities);
        //    }

        //    if (types.Contains(CommunityEntityType.Node))
        //    {
        //        var nodeIds = _membershipRepository.List(m => m.UserId == targetUserId && m.CommunityEntityType == CommunityEntityType.Node && !m.Deleted)
        //          .Select(nm => nm.CommunityEntityId)
        //          .ToList();

        //        var nodes = nodeRepository.Get(nodeIds);
        //        var filteredNodes = GetFilteredNodes(nodes, currentUserId);

        //        res.AddRange(filteredNodes);
        //    }

        //    return res;
        //}

        protected async Task CreateAutoContentEntityAsync(ContentEntity entity)
        {
            await _contentEntityRepository.CreateAsync(entity);

            //mark content entity write
            await _userContentEntityRecordRepository.SetContentEntityWrittenAsync(entity.CreatedByUserId, entity.FeedId.ToString(), entity.Id.ToString(), DateTime.UtcNow);
        }

        protected void RecordListing(string userId, FeedEntity feedEntity)
        {
            RecordListings(userId, new List<FeedEntity> { feedEntity });
        }

        protected void RecordListings(string userId, IEnumerable<FeedEntity> feedEntities)
        {
            if (string.IsNullOrEmpty(userId))
                return;

            Task.WhenAll(feedEntities.Select(entity => _userFeedRecordRepository.SetFeedListedAsync(userId, entity.Id.ToString(), DateTime.UtcNow))).Wait();
        }

        public bool IsVisible(CommunityEntity c, IEnumerable<Membership> currentUserMemberships, IEnumerable<Invitation> invitations, IEnumerable<Relation> allRelations)
        {
            if (c == null)
                return false;

            switch (c.ListingPrivacy)
            {
                case PrivacyLevel.Public:
                    c.ListingAccessOrigin = new AccessOrigin(AccessOriginType.Public);
                    return true;
                case PrivacyLevel.Ecosystem:
                    // is user a direct member
                    if (currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString()))
                    {
                        c.ListingAccessOrigin = new AccessOrigin(AccessOriginType.DirectMember);
                        return true;
                    }

                    //does user have a membership in one of the ecosystem community entities (going up)
                    var targetMemberships = currentUserMemberships.Where(m => allRelations.Any(r => r.SourceCommunityEntityId == c.Id.ToString() && r.TargetCommunityEntityId == m.CommunityEntityId));
                    if (targetMemberships.Any())
                    {
                        c.ListingAccessOrigin = new AccessOrigin(AccessOriginType.EcosystemMember, targetMemberships.ToList());
                        return true;
                    }

                    return false;
                case PrivacyLevel.Private:
                    // is user a direct member
                    if (currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString()))
                    {
                        c.ListingAccessOrigin = new AccessOrigin(AccessOriginType.DirectMember);
                        return true;
                    }

                    return false;
                default:
                    throw new Exception($"Cannot filter {c.Type} for listing privacy {c.ListingPrivacy}");
            }
        }

        public bool IsReadable(CommunityEntity c, IEnumerable<Membership> currentUserMemberships, IEnumerable<Relation> allRelations)
        {
            switch (c.ContentPrivacy)
            {
                case PrivacyLevel.Public:
                    c.ContentAccessOrigin = new AccessOrigin(AccessOriginType.Public);
                    return true;
                case PrivacyLevel.Ecosystem:
                    // is user a direct member
                    if (currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString()))
                    {
                        c.ContentAccessOrigin = new AccessOrigin(AccessOriginType.DirectMember);
                        return true;
                    }

                    //does user have a membership in one of the ecosystem community entities (going up)
                    var targetMemberships = currentUserMemberships.Where(m => allRelations.Any(r => r.SourceCommunityEntityId == c.Id.ToString() && r.TargetCommunityEntityId == m.CommunityEntityId));
                    if (targetMemberships.Any())
                    {
                        c.ListingAccessOrigin = new AccessOrigin(AccessOriginType.EcosystemMember, targetMemberships.ToList());
                        return true;
                    }

                    return false;
                case PrivacyLevel.Private:
                    // is user a direct member
                    if (currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString()))
                    {
                        c.ContentAccessOrigin = new AccessOrigin(AccessOriginType.DirectMember);
                        return true;
                    }

                    return false;

                case PrivacyLevel.Custom:
                    if (currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString()))
                    {
                        c.ContentAccessOrigin = new AccessOrigin(AccessOriginType.DirectMember);
                        return true;
                    }

                    if (c.ContentPrivacyCustomSettings == null)
                        return false;

                    //does user have a membership in one of the ecosystem community entities that are set to grant content read rights
                    var sourceMembershipsCustom = currentUserMemberships.Where(m => allRelations.Any(r => r.SourceCommunityEntityId == m.CommunityEntityId && c.ContentPrivacyCustomSettings.Any(cs => cs.CommunityEntityId == r.SourceCommunityEntityId && cs.Allowed)));
                    var targetMembershipsCustom = currentUserMemberships.Where(m => allRelations.Any(r => r.TargetCommunityEntityId == m.CommunityEntityId && c.ContentPrivacyCustomSettings.Any(cs => cs.CommunityEntityId == r.SourceCommunityEntityId && cs.Allowed)));
                    if (sourceMembershipsCustom.Any() || targetMembershipsCustom.Any())
                    {
                        c.ContentAccessOrigin = new AccessOrigin(AccessOriginType.EcosystemMember, sourceMembershipsCustom.Union(targetMembershipsCustom).ToList());
                        return true;
                    }

                    return false;

                default:
                    throw new Exception($"Cannot filter {c.Type} for content privacy {c.ContentPrivacy}");
            }
        }

        protected void EnrichUserData(IOrganizationRepository organizationRepository, IEnumerable<User> users, string currentUserId)
        {
            var memberships = _membershipRepository.ListForUsers(users.Select(u => u.Id.ToString()));
            var needs = _needRepository.ListForUsers(users.Select(u => u.Id.ToString()));
            var organizations = organizationRepository.Get(memberships.Where(m => m.CommunityEntityType == CommunityEntityType.Organization).Select(m => m.CommunityEntityId).ToList());
            var filteredOrgs = GetFilteredOrganizations(organizations, currentUserId);
            foreach (var user in users)
            {
                user.ProjectCount = memberships.Count(m => m.UserId == user.Id.ToString() && !m.Deleted && m.CommunityEntityType == CommunityEntityType.Project);
                user.CommunityCount = memberships.Count(m => m.UserId == user.Id.ToString() && !m.Deleted && m.CommunityEntityType == CommunityEntityType.Workspace);
                user.NodeCount = memberships.Count(m => m.UserId == user.Id.ToString() && !m.Deleted && m.CommunityEntityType == CommunityEntityType.Node);
                user.OrganizationCount = memberships.Count(m => m.UserId == user.Id.ToString() && !m.Deleted && m.CommunityEntityType == CommunityEntityType.Organization);
                user.NeedCount = needs.Count(n => n.CreatedByUserId == user.Id.ToString() && !n.Deleted && n.EndDate > DateTime.UtcNow);
                user.Organizations = filteredOrgs.Where(o => memberships.Any(m => m.UserId == user.Id.ToString() && m.CommunityEntityId == o.Id.ToString())).ToList();
            }

            var followed = _followingRepository.ListForFollowers(users.Select(u => u.Id.ToString()));
            var followers = _followingRepository.ListForFolloweds(users.Select(u => u.Id.ToString()));

            foreach (var user in users)
            {
                user.FollowedCount = followed.Count(m => m.UserIdFrom == user.Id.ToString() && !m.Deleted);
                user.FollowerCount = followers.Count(m => m.UserIdTo == user.Id.ToString() && !m.Deleted);

                if (string.IsNullOrEmpty(currentUserId))
                    continue;

                user.UserFollows = followers.Any(f => f.UserIdFrom == currentUserId && f.UserIdTo == user.Id.ToString());
            }
        }

        protected void EnrichChannelData(IEnumerable<Channel> channels, string userId)
        {
            var channelIds = channels.Select(c => c.Id.ToString()).ToList();
            var channelMemberships = _membershipRepository.List(m => channelIds.Contains(m.CommunityEntityId) && !m.Deleted);
            var currentUserMemberships = _membershipRepository.List(m => m.UserId == userId && !m.Deleted);

            EnrichChannelData(channels, channelMemberships, currentUserMemberships);
        }

        protected void EnrichChannelData(IEnumerable<Channel> channels, IEnumerable<Membership> channelMemberships, IEnumerable<Membership> currentUserMemberships)
        {
            var channelCommunityEntityIds = channels.Select(c => c.CommunityEntityId).ToList();
            var feedEntitySet = _feedEntityService.GetFeedEntitySet(channelCommunityEntityIds);

            foreach (var c in channels)
            {
                c.MemberCount = channelMemberships.Count(m => m.CommunityEntityId == c.Id.ToString());
                c.CommunityEntity = (CommunityEntity)_feedEntityService.GetEntityFromLists(c.CommunityEntityId, feedEntitySet);
            }

            EnrichChannelsWithMembershipData(channels, currentUserMemberships);
            EnrichEntitiesWithCreatorData(channels);
        }

        //protected void EnrichProjectData(IEnumerable<Project> projects, string userId)
        //{
        //    var projectIds = projects.Select(p => p.Id.ToString());
        //    var relations = _relationRepository.List(r => (projectIds.Contains(r.SourceCommunityEntityId) || projectIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);

        //    EnrichProjectData(projects, relations, userId);
        //}

        protected void EnrichChannelsWithMembershipData(IEnumerable<Channel> channels, IEnumerable<Membership> currentUserMemberships)
        {
            foreach (var c in channels)
            {
                var count = currentUserMemberships.Where(m => m.CommunityEntityId == c.Id.ToString()).ToList();
                if (count.Count() > 1)
                    return;
                var membership = currentUserMemberships.SingleOrDefault(m => m.CommunityEntityId == c.Id.ToString());
                var communityEntityMembership = currentUserMemberships.SingleOrDefault(m => m.CommunityEntityId == c.CommunityEntityId);

                c.Permissions = Enum.GetValues<Permission>().Where(p => Can(c, membership, communityEntityMembership, p)).ToList();
                c.CurrentUserAccessLevel = membership?.AccessLevel;
            }
        }

        protected void EnrichChannelsWithPermissions(IEnumerable<Channel> channels, string userId = null)
        {
            var currentUserMemberships = _membershipRepository.List(m => m.UserId == userId && !m.Deleted);

            foreach (var c in channels)
            {
                var membership = currentUserMemberships.SingleOrDefault(m => m.CommunityEntityId == c.Id.ToString());
                var communityEntityMembership = currentUserMemberships.SingleOrDefault(m => m.CommunityEntityId == c.CommunityEntityId);
                c.Permissions = Enum.GetValues<Permission>().Where(p => Can(c, membership, communityEntityMembership, p)).ToList();
            }
        }

        //protected void EnrichProjectData(IEnumerable<Project> projects, IEnumerable<Relation> relations, string userId)
        //{
        //    var needs = _needRepository.ListForEntityIds(projects.Select(p => p.Id.ToString()));
        //    foreach (var p in projects)
        //    {
        //        p.CommunityCount = relations.Count(r => r.SourceCommunityEntityId == p.Id.ToString());
        //        p.NodeCount = relations.Count(r => r.TargetCommunityEntityType == CommunityEntityType.Node && r.SourceCommunityEntityId == p.Id.ToString());
        //        p.OrganizationCount = relations.Count(r => r.TargetCommunityEntityType == CommunityEntityType.Organization && r.SourceCommunityEntityId == p.Id.ToString());
        //        p.NeedCount = needs.Count(n => n.EntityId == p.Id.ToString());
        //    }

        //    EnrichCommunityEntitiesWithMembershipData(projects, relations, userId);
        //    EnrichCommunityEntitiesWithContentEntityData(projects);
        //}

        //protected void EnrichProjectDataDetail(IEnumerable<Project> projects, string userId = null)
        //{
        //    var projectIds = projects.Select(p => p.Id.ToString());
        //    var relations = _relationRepository.List(r => (projectIds.Contains(r.SourceCommunityEntityId) || projectIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);
        //    var proposals = _proposalRepository.List(p => projectIds.Contains(p.SourceCommunityEntityId) && !p.Deleted);

        //    var papers = _paperRepository.List(p => p.FeedIds.Any(id => projectIds.Contains(id)) && !p.Deleted);
        //    var resources = _resourceRepository.List(r => projectIds.Contains(r.FeedId) && !r.Deleted);
        //    var documents = _documentRepository.List(d => projectIds.Contains(d.FeedId) && d.ContentEntityId == null && d.CommentId == null && !d.Deleted);

        //    foreach (var p in projects)
        //    {
        //        p.ProposalCount = proposals.Count(proposal => proposal.SourceCommunityEntityId == p.Id.ToString());
        //        p.PaperCount = papers.Count(ppr => ppr.FeedIds.Contains(p.Id.ToString()));
        //        p.ResourceCount = resources.Count(r => r.FeedId == p.Id.ToString());
        //        p.DocumentCount = documents.Count(d => d.FeedId == p.Id.ToString());
        //    }

        //    EnrichProjectData(projects, relations, userId);
        //    EnrichEntitiesWithCreatorData(projects);
        //}

        protected void EnrichCallForProposalData(IEnumerable<CallForProposal> callForProposals, string userId)
        {
            var cfpIds = callForProposals.Select(c => c.Id.ToString()).ToList();
            var proposals = _proposalRepository.List(p => cfpIds.Contains(p.CallForProposalId) && !p.Deleted);
            foreach (var cfp in callForProposals)
            {
                cfp.ProposalCount = proposals.Count(p => p.CallForProposalId == cfp.Id.ToString());
                cfp.SubmittedProposalCount = proposals.Count(p => p.CallForProposalId == cfp.Id.ToString() && p.Status != ProposalStatus.Draft);
            }

            //EnrichCommunityEntitiesWithMembershipData(callForProposals, userId);
            EnrichCFPWithMembershipData(callForProposals, userId);
            EnrichCommunityEntitiesWithContentEntityData(callForProposals);
        }

        protected void EnrichCallForProposalDataDetail(IEnumerable<CallForProposal> callForProposals, string userId)
        {
            //var cfpIds = callForProposals.Select(c => c.Id.ToString()).ToList();
            //var relations = _relationRepository.List(r => (cfpIds.Contains(r.SourceCommunityEntityId) || cfpIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);

            //var papers = _paperRepository.List(p => p.FeedIds.Any(id => cfpIds.Contains(id)) && !p.Deleted);
            //var resources = _resourceRepository.List(r => cfpIds.Contains(r.FeedId) && !r.Deleted);
            //var documents = _documentRepository.List(d => cfpIds.Contains(d.FeedId) && !d.Deleted);

            //foreach (var c in callForProposals)
            //{
            //    c.PaperCount = papers.Count(ppr => ppr.FeedIds.Contains(c.Id.ToString()));
            //    c.ResourceCount = resources.Count(r => r.FeedId == c.Id.ToString());
            //    c.DocumentCount = documents.Count(d => d.FeedId == c.Id.ToString());
            //}

            EnrichCallForProposalData(callForProposals, userId);
            EnrichEntitiesWithCreatorData(callForProposals);
        }

        protected void EnrichWorkspaceData(IEnumerable<Workspace> communities, string userId)
        {
            var communityIds = communities.Select(c => c.Id.ToString()).ToList();
            var relations = _relationRepository.List(r => (communityIds.Contains(r.SourceCommunityEntityId) || communityIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);
            EnrichWorkspaceData(communities, relations, userId);
        }

        protected void EnrichWorkspaceData(IEnumerable<Workspace> communities, IEnumerable<Relation> relations, string userId)
        {
            var communityIds = communities.Select(c => c.Id.ToString()).ToList();
            var cfps = _callForProposalsRepository.List(cfp => communityIds.Contains(cfp.ParentCommunityEntityId) && !cfp.Deleted);
            var needs = _needRepository.ListForEntityIds(communities.Select(c => c.Id.ToString()));
            foreach (var c in communities)
            {
                c.WorkspaceCount = relations.Count(r => r.SourceCommunityEntityType == CommunityEntityType.Workspace && r.TargetCommunityEntityId == c.Id.ToString());
                c.NodeCount = relations.Count(r => r.TargetCommunityEntityType == CommunityEntityType.Node && r.SourceCommunityEntityId == c.Id.ToString());
                c.OrganizationCount = relations.Count(r => r.TargetCommunityEntityType == CommunityEntityType.Organization && r.SourceCommunityEntityId == c.Id.ToString());
                c.CFPCount = cfps.Count(cfp => cfp.ParentCommunityEntityId == c.Id.ToString());
                c.NeedCount = needs.Count(n => n.EntityId == c.Id.ToString());
            }

            EnrichCommunityEntitiesWithMembershipData(communities, relations, userId);
            EnrichCommunityEntitiesWithContentEntityData(communities);
        }

        protected void EnrichCommunityEntityDataWithContribution(IEnumerable<CommunityEntity> communityEntities, List<Membership> memberships, string targetUserId)
        {
            foreach (var communityEntity in communityEntities)
            {
                var membership = memberships.FirstOrDefault(m => m.CommunityEntityId == communityEntity.Id.ToString() && m.UserId == targetUserId);
                communityEntity.Contribution = membership?.Contribution;
            }
        }

        protected void EnrichCommunityDataDetail(IEnumerable<Workspace> communities, string userId)
        {
            var communityIds = communities.Select(c => c.Id.ToString()).ToList();
            var relations = _relationRepository.List(r => (communityIds.Contains(r.SourceCommunityEntityId) || communityIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);
            var communityEntityIds = GetCommunityEntityIdsForCommunities(relations, communityIds);

            var needs = _needRepository.ListForEntityIds(communityEntityIds);
            var papers = _paperRepository.List(p => communityEntityIds.Contains(p.FeedId) && !p.Deleted);
            var resources = _resourceRepository.List(r => communityEntityIds.Contains(r.FeedId) && !r.Deleted);
            var documents = _documentRepository.List(d => communityEntityIds.Contains(d.FeedId) && d.ContentEntityId == null && d.CommentId == null && !d.Deleted);

            foreach (var c in communities)
            {
                c.NeedCount = needs.Count(need => c.Id.ToString() == need.EntityId);
                c.NeedCountAggregate = needs.Count(need => c.Id.ToString() == need.EntityId || relations.Any(r => r.SourceCommunityEntityId == need.EntityId && r.TargetCommunityEntityId == c.Id.ToString()));
                c.PaperCount = papers.Count(ppr => c.Id.ToString() == ppr.FeedId);
                c.PaperCountAggregate = papers.Count(ppr => c.Id.ToString() == ppr.FeedId || relations.Any(r => r.SourceCommunityEntityId == ppr.FeedId && r.TargetCommunityEntityId == c.Id.ToString()));
                c.ResourceCount = resources.Count(res => c.Id.ToString() == res.FeedId);
                c.ResourceCountAggregate = resources.Count(res => c.Id.ToString() == res.FeedId || relations.Any(r => r.SourceCommunityEntityId == res.FeedId && r.TargetCommunityEntityId == c.Id.ToString()));
                c.DocumentCount = documents.Count(d => c.Id.ToString() == d.FeedId);
                c.DocumentCountAggregate = documents.Count(d => c.Id.ToString() == d.FeedId || relations.Any(r => r.SourceCommunityEntityId == d.FeedId && r.TargetCommunityEntityId == c.Id.ToString()));
            }

            EnrichWorkspaceData(communities, relations, userId);
            EnrichEntitiesWithCreatorData(communities);
        }

        protected void EnrichNodeData(IEnumerable<Node> nodes, string userId)
        {
            var nodeIds = nodes.Select(c => c.Id.ToString()).ToList();
            var relations = _relationRepository.List(r => (nodeIds.Contains(r.SourceCommunityEntityId) || nodeIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);
            EnrichNodeData(nodes, relations, userId);
        }

        protected void EnrichNodeData(IEnumerable<Node> nodes, IEnumerable<Relation> relations, string userId)
        {
            foreach (var n in nodes)
            {
                n.WorkspaceCount = relations.Count(r => r.SourceCommunityEntityType == CommunityEntityType.Workspace && r.TargetCommunityEntityId == n.Id.ToString());
                n.OrganizationCount = relations.Count(r => r.TargetCommunityEntityType == CommunityEntityType.Organization && r.SourceCommunityEntityId == n.Id.ToString());
            }

            EnrichCommunityEntitiesWithMembershipData(nodes, relations, userId);
            EnrichCommunityEntitiesWithContentEntityData(nodes);
        }

        protected void EnrichNodeDataDetail(IEnumerable<Node> nodes, string userId)
        {
            var nodeIds = nodes.Select(c => c.Id.ToString()).ToList();
            var relations = _relationRepository.List(r => (nodeIds.Contains(r.SourceCommunityEntityId) || nodeIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);
            var communityEntityIds = GetCommunityEntityIdsForNodes(relations, nodeIds);

            var needs = _needRepository.ListForEntityIds(communityEntityIds);
            var papers = _paperRepository.List(p => communityEntityIds.Contains(p.FeedId) && !p.Deleted);
            var resources = _resourceRepository.List(r => communityEntityIds.Contains(r.FeedId) && !r.Deleted);
            var documents = _documentRepository.List(d => communityEntityIds.Contains(d.FeedId) && d.ContentEntityId == null && d.CommentId == null && !d.Deleted);
            var cfps = _callForProposalsRepository.List(cfp => communityEntityIds.Contains(cfp.ParentCommunityEntityId) && !cfp.Deleted);

            foreach (var n in nodes)
            {
                var nodeCommunityEntityIds = GetCommunityEntityIdsForNode(relations, n.Id.ToString());

                n.NeedCount = needs.Count(need => n.Id.ToString() == need.EntityId);
                n.NeedCountAggregate = needs.Count(need => nodeCommunityEntityIds.Contains(need.EntityId));
                n.PaperCount = papers.Count(ppr => n.Id.ToString() == ppr.FeedId);
                n.PaperCountAggregate = papers.Count(ppr => nodeCommunityEntityIds.Contains(ppr.FeedId));
                n.ResourceCount = resources.Count(res => n.Id.ToString() == res.FeedId);
                n.ResourceCountAggregate = resources.Count(res => nodeCommunityEntityIds.Contains(res.FeedId));
                n.DocumentCount = documents.Count(d => n.Id.ToString() == d.FeedId);
                n.DocumentCountAggregate = documents.Count(d => nodeCommunityEntityIds.Contains(d.FeedEntityId));

                n.CFPCount = cfps.Count(cfp => nodeCommunityEntityIds.Contains(cfp.ParentCommunityEntityId));
            }

            EnrichNodeData(nodes, relations, userId);
            EnrichEntitiesWithCreatorData(nodes);
        }

        protected void EnrichOrganizationData(IEnumerable<Organization> organizations, string userId)
        {
            var orgIds = organizations.Select(c => c.Id.ToString()).ToList();
            var relations = _relationRepository.List(r => (orgIds.Contains(r.SourceCommunityEntityId) || orgIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);
            EnrichOrganizationData(organizations, relations, userId);
        }

        protected void EnrichOrganizationData(IEnumerable<Organization> organizations, IEnumerable<Relation> relations, string userId = null)
        {
            foreach (var o in organizations)
            {
                o.WorkspaceCount = relations.Count(r => r.SourceCommunityEntityType == CommunityEntityType.Workspace && r.TargetCommunityEntityId == o.Id.ToString());
                o.NodeCount = relations.Count(r => r.SourceCommunityEntityType == CommunityEntityType.Node && r.TargetCommunityEntityId == o.Id.ToString());
            }

            EnrichCommunityEntitiesWithMembershipData(organizations, relations, userId);
        }

        protected void EnrichOrganizationDataDetail(IEnumerable<Organization> organizations, string userId)
        {
            var orgIds = organizations.Select(c => c.Id.ToString()).ToList();
            var relations = _relationRepository.List(r => (orgIds.Contains(r.SourceCommunityEntityId) || orgIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);

            var papers = _paperRepository.List(p => orgIds.Contains(p.FeedId) && !p.Deleted);
            var resources = _resourceRepository.List(r => orgIds.Contains(r.FeedId) && !r.Deleted);
            var documents = _documentRepository.List(d => orgIds.Contains(d.FeedId) && !d.Deleted);

            foreach (var o in organizations)
            {
                o.PaperCount = papers.Count(ppr => ppr.FeedId == o.Id.ToString());
                o.ResourceCount = resources.Count(r => r.FeedId == o.Id.ToString());
                o.DocumentCount = documents.Count(d => d.FeedId == o.Id.ToString());
            }

            EnrichOrganizationData(organizations, relations, userId);
            EnrichEntitiesWithCreatorData(organizations);
        }

        protected void EnrichCommunityEntitiesWithMembershipData(IEnumerable<CommunityEntity> communityEntities, string userId)
        {
            var communityEntityMemberships = _membershipRepository.ListForCommunityEntities(communityEntities.Select(c => c.Id.ToString()));
            var currentUserMemberships = _membershipRepository.List(m => !m.Deleted && m.UserId == userId);
            var relations = _relationRepository.ListForSourceOrTargetIds(communityEntities.Select(c => c.Id.ToString()));
            EnrichCommunityEntitiesWithMembershipData(communityEntities, communityEntityMemberships, currentUserMemberships, relations, userId);
        }

        protected void EnrichCommunityEntitiesWithMembershipData(IEnumerable<CommunityEntity> communityEntities, IEnumerable<Relation> relations, string userId)
        {
            var communityEntityMemberships = _membershipRepository.ListForCommunityEntities(communityEntities.Select(c => c.Id.ToString()));
            var currentUserMemberships = _membershipRepository.List(m => !m.Deleted && m.UserId == userId);
            EnrichCommunityEntitiesWithMembershipData(communityEntities, communityEntityMemberships, currentUserMemberships, relations, userId);
        }

        //protected void EnrichCommunityEntitiesWithMembershipData(IEnumerable<CommunityEntity> communityEntities, IEnumerable<Membership> currentUserMemberships, IEnumerable<Relation> relations, string userId)
        //{
        //    var communityEntityMemberships = _membershipRepository.ListForCommunityEntities(communityEntities.Select(c => c.Id.ToString()));
        //    EnrichCommunityEntitiesWithMembershipData(communityEntities, communityEntityMemberships, currentUserMemberships, relations, userId);
        //}

        protected void EnrichCommunityEntitiesWithMembershipData(IEnumerable<CommunityEntity> communityEntities, IEnumerable<Membership> communityEntityMemberships, IEnumerable<Membership> currentUserMemberships, IEnumerable<Relation> relations, string userId)
        {
            foreach (var ce in communityEntities)
            {
                ce.MemberCount = communityEntityMemberships.Count(cm => cm.CommunityEntityId == ce.Id.ToString());
                ce.Permissions = Enum.GetValues<Permission>().Where(p => Can(ce, currentUserMemberships, relations.Where(r => r.SourceCommunityEntityId == ce.Id.ToString() || r.TargetCommunityEntityId == ce.Id.ToString()), p)).ToList();

                if (!string.IsNullOrEmpty(userId))
                {
                    var membership = currentUserMemberships.SingleOrDefault(m => m.CommunityEntityId == ce.Id.ToString() && m.UserId == userId);
                    ce.OnboardedUTC = membership?.OnboardedUTC;
                    ce.AccessLevel = membership?.AccessLevel;
                }
            }
        }

        protected void EnrichCFPWithMembershipData(IEnumerable<CallForProposal> callsForProposals, string userId = null)
        {
            var communityEntityMemberships = _membershipRepository.ListForCommunityEntities(callsForProposals.Select(c => c.Id.ToString()));
            var currentUserMemberships = _membershipRepository.List(m => !m.Deleted && m.UserId == userId);
            var relations = _relationRepository.ListForSourceOrTargetIds(callsForProposals.Select(c => c.ParentCommunityEntityId));

            EnrichCommunityEntitiesWithMembershipData(callsForProposals, communityEntityMemberships, currentUserMemberships, relations, userId);

            var projectRelations = relations.Where(r => r.SourceCommunityEntityType == CommunityEntityType.Project);
            var hubRelations = relations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Node);

            foreach (var cfp in callsForProposals)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var cfpMembership = currentUserMemberships.SingleOrDefault(m => m.CommunityEntityId == cfp.Id.ToString());
                    var communityMembership = currentUserMemberships.SingleOrDefault(m => m.CommunityEntityId == cfp.ParentCommunityEntityId);
                    var projectMemberships = currentUserMemberships.Where(m => projectRelations.Any(r => r.SourceCommunityEntityId == m.CommunityEntityId)).ToList();
                    var hubMemberships = currentUserMemberships.Where(m => hubRelations.Any(r => r.TargetCommunityEntityId == m.CommunityEntityId)).ToList();

                    switch (cfp.ProposalParticipation)
                    {
                        case PrivacyLevel.Public:
                            cfp.Permissions.Add(Permission.CreateProposals);
                            break;
                        case PrivacyLevel.Ecosystem:
                            if (projectMemberships.Any() || hubMemberships.Any())
                                cfp.Permissions.Add(Permission.CreateProposals);

                            if (communityMembership != null)
                                cfp.Permissions.Add(Permission.CreateProposals);

                            break;
                        case PrivacyLevel.Private:
                            if (communityMembership != null)
                                cfp.Permissions.Add(Permission.CreateProposals);

                            break;
                        default:
                            throw new Exception($"Unknown proposal privacy: {cfp.ProposalParticipation}");
                    }

                    switch (cfp.DiscussionParticipation)
                    {
                        case DiscussionParticipation.Public:
                            cfp.Permissions.Add(Permission.PostContentEntity);
                            break;
                        case DiscussionParticipation.Ecosystem:
                            if (projectMemberships.Any() || hubMemberships.Any())
                                cfp.Permissions.Add(Permission.PostContentEntity);

                            if (communityMembership != null)
                                cfp.Permissions.Add(Permission.PostContentEntity);

                            break;
                        case DiscussionParticipation.Private:
                            if (communityMembership != null)
                                cfp.Permissions.Add(Permission.PostContentEntity);

                            break;
                        case DiscussionParticipation.Participants:
                            if (cfpMembership != null)
                                cfp.Permissions.Add(Permission.PostContentEntity);
                            break;
                        case DiscussionParticipation.AdminOnly:
                            if (cfpMembership != null && (cfpMembership.AccessLevel == AccessLevel.Admin || cfpMembership.AccessLevel == AccessLevel.Owner))
                                cfp.Permissions.Add(Permission.PostContentEntity);

                            break;
                        default:
                            throw new Exception($"Unknown discussion participation: {cfp.DiscussionParticipation}");
                    }

                    switch (cfp.ProposalPrivacy)
                    {
                        case ProposalPrivacyLevel.Public:
                            cfp.Permissions.Add(Permission.ListProposals);
                            break;
                        case ProposalPrivacyLevel.Ecosystem:
                            if (projectMemberships.Any() || hubMemberships.Any())
                                cfp.Permissions.Add(Permission.ListProposals);

                            if (communityMembership != null)
                                cfp.Permissions.Add(Permission.ListProposals);

                            break;
                        case ProposalPrivacyLevel.Private:
                            if (communityMembership != null)
                                cfp.Permissions.Add(Permission.ListProposals);

                            break;

                        case ProposalPrivacyLevel.AdminAndReviewers:
                            if (communityMembership != null && (communityMembership.AccessLevel == AccessLevel.Admin || communityMembership.AccessLevel == AccessLevel.Owner))
                                cfp.Permissions.Add(Permission.ListProposals);

                            if (communityMembership != null && communityMembership.Labels?.Contains(LABEL_REVIEWER) == true)
                                cfp.Permissions.Add(Permission.ListProposals);

                            break;

                        case ProposalPrivacyLevel.Admin:
                            if (communityMembership != null && (communityMembership.AccessLevel == AccessLevel.Admin || communityMembership.AccessLevel == AccessLevel.Owner))
                                cfp.Permissions.Add(Permission.ListProposals);

                            break;

                        default:
                            throw new Exception($"Unknown discussion participation: {cfp.DiscussionParticipation}");
                    }
                }
                else
                {
                    switch (cfp.ProposalPrivacy)
                    {
                        case ProposalPrivacyLevel.Public:
                            cfp.Permissions.Add(Permission.ListProposals);
                            break;

                    }
                }
            }
        }

        protected void EnrichPapersWithPermissions(IEnumerable<Paper> papers, IEnumerable<Membership> currentUserMemberships, string userId)
        {
            foreach (var paper in papers)
            {
                paper.Permissions = Enum.GetValues<Permission>().Where(p => Can(paper, currentUserMemberships, userId, p)).ToList();
            }
        }

        protected void EnrichPapersWithPermissions(IEnumerable<Paper> papers, string userId)
        {
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == userId && !p.Deleted);

            EnrichPapersWithPermissions(papers, currentUserMemberships, userId);
        }

        protected void EnrichDocumentsWithPermissions(IEnumerable<Document> documents, IEnumerable<Membership> currentUserMemberships, IEnumerable<EventAttendance> currentUserAttendances, IEnumerable<Relation> entityRelations, string userId)
        {
            foreach (var d in documents)
            {
                d.Permissions = Enum.GetValues<Permission>().Where(p => Can(d, d.FeedEntity, currentUserMemberships, currentUserAttendances, entityRelations, userId, p)).ToList();
            }
        }

        protected void EnrichDocumentsWithPermissions(IEnumerable<Document> documents, string userId)
        {
            var entityIds = documents.Select(d => d.FeedId).ToList();
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == userId && !p.Deleted);
            var currentUserAttendances = _eventAttendanceRepository.List(a => a.UserId == userId && !a.Deleted);
            var entityRelations = _relationRepository.List(r => (entityIds.Contains(r.SourceCommunityEntityId) || entityIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);

            EnrichDocumentsWithPermissions(documents, currentUserMemberships, currentUserAttendances, entityRelations, userId);
        }

        protected void EnrichNeedsWithPermissions(IEnumerable<Need> needs, IEnumerable<Membership> currentUserMemberships, string userId)
        {
            foreach (var n in needs)
            {
                n.Permissions = Enum.GetValues<Permission>().Where(p => Can(n, currentUserMemberships, userId, p)).ToList();
            }
        }

        protected void EnrichNeedsWithPermissions(IEnumerable<Need> needs, string userId = null)
        {
            var currentUserMemberships = _membershipRepository.List(m => m.UserId == userId && !m.Deleted);
            EnrichNeedsWithPermissions(needs, currentUserMemberships, userId);
        }

        protected void EnrichEventsWithPermissions(IEnumerable<Event> events, IEnumerable<EventAttendance> eventAttendances, IEnumerable<Membership> currentUserMemberships, string userId = null)
        {
            foreach (var e in events)
            {
                e.Permissions = Enum.GetValues<Permission>().Where(p => Can(e, eventAttendances.Where(ea => ea.EventId == e.Id.ToString() && ea.UserId == userId && !string.IsNullOrEmpty(userId)), currentUserMemberships, userId, p)).ToList();
            }
        }

        protected void EnrichEventsWithPermissions(IEnumerable<Event> events, IEnumerable<EventAttendance> eventAttendances, string userId = null)
        {
            var currentUserMemberships = _membershipRepository.List(m => m.UserId == userId && !m.Deleted);
            EnrichEventsWithPermissions(events, eventAttendances, currentUserMemberships, userId);
        }

        protected void EnrichEventsWithPermissions(IEnumerable<Event> events, string userId = null)
        {
            var currentUserMemberships = _membershipRepository.List(m => m.UserId == userId && !m.Deleted);
            var eventAttendances = _eventAttendanceRepository.List(m => m.UserId == userId && !m.Deleted);

            EnrichEventsWithPermissions(events, eventAttendances, currentUserMemberships, userId);
        }

        protected void EnrichUsersWithPermissions(IEnumerable<User> users, string userId = null)
        {
            foreach (var u in users)
            {
                if (u.Id.ToString() == userId)
                    u.Permissions = new List<Permission> { Permission.Read, Permission.Manage, Permission.ManageLibrary, Permission.ManageDocuments };
                else
                    u.Permissions = new List<Permission> { Permission.Read };
            }
        }

        protected void EnrichCommunityEntitiesWithContentEntityData(IEnumerable<CommunityEntity> communityEntities)
        {
            var contentEntities = _contentEntityRepository.List(communityEntities.Select(c => c.Id.ToString()));
            foreach (var ce in communityEntities)
            {
                ce.ContentEntityCount = contentEntities.Count(coe => coe.FeedId == ce.Id.ToString() && !(coe.Type == ContentEntityType.Announcement));
                ce.PostCount = contentEntities.Count(coe => coe.FeedId == ce.Id.ToString() && (coe.Type == ContentEntityType.Announcement));
            }
        }

        protected void EnrichEntityWithCreatorData(Entity entity)
        {
            EnrichEntitiesWithCreatorData(new List<Entity> { entity });
        }

        protected void EnrichEntitiesWithCreatorData(IEnumerable<Entity> entities)
        {
            var users = _userRepository.Get(entities.Select(e => e.CreatedByUserId).ToList());
            EnrichEntitiesWithCreatorData(entities, users);
        }

        protected void EnrichFeedEntitiesWithVisibilityData(IEnumerable<FeedEntity> feedEntities)
        {
            var visibilityUserIds = feedEntities.SelectMany(d => (d.UserVisibility ?? new List<FeedEntityUserVisibility>()).Select(uv => uv.UserId)).ToList();
            var visibilityEntityIds = feedEntities.SelectMany(d => (d.CommunityEntityVisibility ?? new List<FeedEntityCommunityEntityVisibility>()).Select(cev => cev.CommunityEntityId)).ToList();
            var visibilityUsers = _userRepository.Get(visibilityUserIds);
            var visibilityCes = _feedEntityService.GetFeedEntitySetForCommunities(visibilityEntityIds);

            foreach (var fe in feedEntities)
            {
                foreach (var uv in fe.UserVisibility ?? new List<FeedEntityUserVisibility>())
                {
                    uv.User = visibilityUsers.SingleOrDefault(u => u.Id.ToString() == uv.UserId);
                }

                foreach (var cev in fe.CommunityEntityVisibility ?? new List<FeedEntityCommunityEntityVisibility>())
                {
                    cev.CommunityEntity = visibilityCes.CommunityEntities.SingleOrDefault(ce => ce.Id.ToString() == cev.CommunityEntityId);
                }
            }
        }

        protected void EnrichEntitiesWithCreatorData(IEnumerable<Entity> entities, IEnumerable<User> users)
        {
            foreach (var entity in entities)
            {
                entity.CreatedBy = users.SingleOrDefault(u => u.Id.ToString() == entity.CreatedByUserId);
            }
        }

        protected void EnrichFeedEntitiesWithFeedStats(IEnumerable<FeedEntity> feedEntities)
        {
            var entityIds = feedEntities.Select(e => e.Id.ToString()).ToList();
            var counts = _contentEntityRepository.Counts(ce => ce.FeedId); //TODO optimize - only return for specific entities
            foreach (var entity in feedEntities)
            {
                entity.PostCount = counts.ContainsKey(entity.Id.ToString()) ? (int)counts[entity.Id.ToString()] : 0;
            }
        }

        //protected void EnrichContentEntitiesWithMembershipData(IEnumerable<ContentEntity> contentEntities, string userId)
        //{
        //    var currentUserMemberships = _membershipRepository.List(p => p.UserId == userId && !p.Deleted);
        //    var sourceEntityRelations = _relationRepository.ListForSourceIds(contentEntities.Select(p => p.FeedId).ToList());
        //    var targetEntityRelations = _relationRepository.ListForTargetIds(contentEntities.Select(p => p.FeedId).ToList());

        //    foreach (var ce in contentEntities)
        //    {
        //        ce.Permissions = new List<Permission>();
        //        if (CanSeeContentEntity(ce, currentUserMemberships, sourceEntityRelations, targetEntityRelations, userId))
        //            ce.Permissions.Add(Permission.PostContentEntity);
        //    }
        //}

        protected List<Channel> GetFilteredChannels(IEnumerable<Channel> channels, IEnumerable<Membership> currentUserMemberships, int page, int pageSize)
        {
            return GetFilteredChannels(channels, currentUserMemberships)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();
        }

        protected List<Channel> GetFilteredChannels(IEnumerable<Channel> channels, IEnumerable<Membership> currentUserMemberships)
        {
            return channels.Where(c => currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString()) || (c.Visibility == ChannelVisibility.Open))
                           .ToList();
        }

        //protected List<Project> GetFilteredProjects(IEnumerable<Project> projects, string currentUserId, List<Permission> permissions = null)
        //{
        //    var targetRelations = _relationRepository.ListForSourceIds(projects.Select(c => c.Id.ToString()));

        //    return GetFilteredProjects(projects, targetRelations, currentUserId, permissions);
        //}

        //protected List<Project> GetFilteredProjects(IEnumerable<Project> projects, string currentUserId, List<Permission> permissions, int page, int pageSize)
        //{
        //    var targetRelations = _relationRepository.ListForSourceIds(projects.Select(c => c.Id.ToString()));

        //    return GetFilteredProjects(projects, targetRelations, currentUserId, permissions, page, pageSize);
        //}

        //protected List<Project> GetFilteredProjects(IEnumerable<Project> projects, IEnumerable<Relation> allRelations, string currentUserId, List<Permission> permissions, int page, int pageSize)
        //{
        //    return GetFilteredProjects(projects, allRelations, currentUserId, permissions)
        //                             .Skip((page - 1) * pageSize)
        //                             .Take(pageSize)
        //                             .ToList();
        //}

        //protected List<Project> GetFilteredProjects(IEnumerable<Project> projects, IEnumerable<Relation> allRelations, string currentUserId, List<Permission> permissions)
        //{
        //    var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
        //    var currentUserInvitations = _invitationRepository.List(i => !i.Deleted && i.Status == InvitationStatus.Pending && i.InviteeUserId == currentUserId);

        //    return GetFilteredProjects(projects, allRelations, currentUserMemberships, currentUserInvitations, permissions);
        //}

        //protected List<Project> GetFilteredProjects(IEnumerable<Project> projects, IEnumerable<Relation> allRelations, IEnumerable<Membership> currentUserMemberships, IEnumerable<Invitation> currentUserInvitations, List<Permission> permissions)
        //{
        //    var filteredProjects = projects.Where(p => p.Status != "draft" || currentUserMemberships.Any(m => m.CommunityEntityId == p.Id.ToString())) // is user a member?
        //                                   .Where(p => !p.Deleted)
        //                                   .Where(c => IsVisible(c, currentUserMemberships, currentUserInvitations, allRelations.Where(r => r.SourceCommunityEntityId == c.Id.ToString() || r.TargetCommunityEntityId == c.Id.ToString())))
        //                                   .Where(p => permissions == null || permissions.All(pm => Can(p, currentUserMemberships, allRelations, pm)))
        //                                   .ToList();

        //    return filteredProjects;
        //}

        protected List<CallForProposal> GetFilteredCallForProposals(IEnumerable<CallForProposal> callForProposals, IEnumerable<Workspace> communities, string currentUserId, List<Permission> permissions = null)
        {
            var sourceRelations = _relationRepository.ListForTargetIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));
            var targetRelations = _relationRepository.ListForSourceIds(callForProposals.Select(c => c.ParentCommunityEntityId.ToString()));

            return GetFilteredCallForProposals(callForProposals, communities, sourceRelations.Union(targetRelations), currentUserId, permissions);
        }

        protected List<CallForProposal> GetFilteredCallForProposals(IEnumerable<CallForProposal> callForProposals, IEnumerable<Workspace> communities, IEnumerable<Relation> allRelations, string currentUserId, List<Permission> permissions, int page, int pageSize)
        {
            return GetFilteredCallForProposals(callForProposals, communities, allRelations, currentUserId, permissions)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();
        }

        protected List<CallForProposal> GetFilteredCallForProposals(IEnumerable<CallForProposal> callForProposals, IEnumerable<Workspace> communities, IEnumerable<Relation> allRelations, string currentUserId, List<Permission> permissions)
        {
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            var currentUserInvitations = _invitationRepository.List(i => !i.Deleted && i.Status == InvitationStatus.Pending && i.InviteeUserId == currentUserId);

            return GetFilteredCallForProposals(callForProposals, communities, allRelations, currentUserMemberships, currentUserInvitations, permissions);
        }

        protected List<CallForProposal> GetFilteredCallForProposals(IEnumerable<CallForProposal> callForProposals, IEnumerable<Workspace> communities, IEnumerable<Relation> allRelations, IEnumerable<Membership> currentUserMemberships, IEnumerable<Invitation> currentUserInvitations, List<Permission> permissions)
        {
            var filteredCallForProposals = callForProposals
                                           .Where(p => p.Status != "draft" || currentUserMemberships.Any(m => m.CommunityEntityId == p.Id.ToString())) // is user a member?
                                           .Where(p => !p.Deleted)
                                           .Where(p => IsVisible(communities.SingleOrDefault(c => c.Id.ToString() == p.ParentCommunityEntityId), currentUserMemberships, currentUserInvitations, allRelations.Where(r => r.SourceCommunityEntityId == p.ParentCommunityEntityId || r.TargetCommunityEntityId == p.ParentCommunityEntityId)))
                                           .Where(p => permissions == null || permissions.All(pm => Can(p, currentUserMemberships, allRelations, pm)))
                                           .ToList();

            return filteredCallForProposals;
        }

        protected List<Workspace> GetFilteredWorkspaces(IEnumerable<Workspace> communities, string currentUserId, List<Permission> permissions = null)
        {
            var sourceRelations = _relationRepository.ListForTargetIds(communities.Select(c => c.Id.ToString()));
            var targetRelations = _relationRepository.ListForSourceIds(communities.Select(c => c.Id.ToString()));

            return GetFilteredWorkspaces(communities, sourceRelations.Union(targetRelations), currentUserId, permissions);
        }

        protected List<Workspace> GetFilteredWorkspaces(IEnumerable<Workspace> communities, string currentUserId, List<Permission> permissions, int page, int pageSize)
        {
            var sourceRelations = _relationRepository.ListForTargetIds(communities.Select(c => c.Id.ToString()));
            var targetRelations = _relationRepository.ListForSourceIds(communities.Select(c => c.Id.ToString()));

            return GetFilteredWorkspaces(communities, sourceRelations.Union(targetRelations), currentUserId, permissions, page, pageSize);
        }

        protected List<Workspace> GetFilteredWorkspaces(IEnumerable<Workspace> communities, IEnumerable<Relation> allRelations, string currentUserId, List<Permission> permissions, int page, int pageSize)
        {
            return GetFilteredWorkspaces(communities, allRelations, currentUserId, permissions)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();
        }

        protected List<Workspace> GetFilteredWorkspaces(IEnumerable<Workspace> communities, IEnumerable<Relation> allRelations, string currentUserId, List<Permission> permissions)
        {
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            var currentUserInvitations = _invitationRepository.List(i => !i.Deleted && i.Status == InvitationStatus.Pending && i.InviteeUserId == currentUserId);

            return GetFilteredWorkspaces(communities, allRelations, currentUserMemberships, currentUserInvitations, permissions);
        }

        protected List<Workspace> GetFilteredWorkspaces(IEnumerable<Workspace> communities, IEnumerable<Relation> allRelations, IEnumerable<Membership> currentUserMemberships, IEnumerable<Invitation> currentUserInvitations, List<Permission> permissions)
        {
            return communities.Where(c => !c.Deleted)
                              .Where(c => c.Status != "draft" || currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString())) // is user a member?
                              .Where(c => IsVisible(c, currentUserMemberships, currentUserInvitations, allRelations.Where(r => r.SourceCommunityEntityId == c.Id.ToString() || r.TargetCommunityEntityId == c.Id.ToString())))
                              .Where(c => permissions == null || permissions.All(pm => Can(c, currentUserMemberships, allRelations, pm)))
                              .ToList();

        }

        protected List<Node> GetFilteredNodes(IEnumerable<Node> nodes, string currentUserId, List<Permission> permissions = null)
        {
            var sourceRelations = _relationRepository.ListForTargetIds(nodes.Select(c => c.Id.ToString()));

            return GetFilteredNodes(nodes, sourceRelations, currentUserId, permissions);
        }

        protected List<Node> GetFilteredNodes(IEnumerable<Node> nodes, string currentUserId, List<Permission> permissions, int page, int pageSize)
        {
            var sourceRelations = _relationRepository.ListForTargetIds(nodes.Select(c => c.Id.ToString()));

            return GetFilteredNodes(nodes, sourceRelations, currentUserId, permissions, page, pageSize);
        }

        protected List<Node> GetFilteredNodes(IEnumerable<Node> nodes, IEnumerable<Relation> allRelations, string currentUserId, List<Permission> permissions, int page, int pageSize)
        {
            return GetFilteredNodes(nodes, allRelations, currentUserId, permissions)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();
        }

        protected List<Node> GetFilteredNodes(IEnumerable<Node> nodes, IEnumerable<Relation> allRelations, string currentUserId, List<Permission> permissions)
        {
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            var currentUserInvitations = _invitationRepository.List(i => !i.Deleted && i.Status == InvitationStatus.Pending && i.InviteeUserId == currentUserId);

            return GetFilteredNodes(nodes, allRelations, currentUserMemberships, currentUserInvitations, permissions);
        }

        protected List<Node> GetFilteredNodes(IEnumerable<Node> nodes, IEnumerable<Relation> allRelations, IEnumerable<Membership> currentUserMemberships, IEnumerable<Invitation> currentUserInvitations, List<Permission> permissions)
        {
            return nodes.Where(c => !c.Deleted)
                        .Where(c => c.Status != "draft" || currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString())) // is user a member?
                        .Where(c => IsVisible(c, currentUserMemberships, currentUserInvitations, allRelations.Where(r => r.SourceCommunityEntityId == c.Id.ToString() || r.TargetCommunityEntityId == c.Id.ToString())))
                        .Where(n => permissions == null || permissions.All(pm => Can(n, currentUserMemberships, allRelations, pm)))
                        .ToList();
        }

        protected List<Organization> GetFilteredOrganizations(IEnumerable<Organization> organizations, string currentUserId)
        {
            return GetFilteredOrganizations(organizations, currentUserId, new List<Permission> { });
        }

        protected List<Organization> GetFilteredOrganizations(IEnumerable<Organization> organizations, string currentUserId, List<Permission> permissions, int page, int pageSize)
        {
            return GetFilteredOrganizations(organizations, currentUserId, permissions)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();
        }

        protected List<Organization> GetFilteredOrganizations(IEnumerable<Organization> organizations, string currentUserId, List<Permission> permissions)
        {
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            var currentUserInvitations = _invitationRepository.List(i => !i.Deleted && i.Status == InvitationStatus.Pending && i.InviteeUserId == currentUserId);

            return GetFilteredOrganizations(organizations, currentUserMemberships, currentUserInvitations, permissions);
        }

        protected List<Organization> GetFilteredOrganizations(IEnumerable<Organization> organizations, IEnumerable<Membership> currentUserMemberships, IEnumerable<Invitation> currentUserInvitations, List<Permission> permissions)
        {
            return organizations.Where(c => !c.Deleted)
                                .Where(c => c.Status != "draft" || currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString())) // is user a member?
                                .Where(c => IsVisible(c, currentUserMemberships, currentUserInvitations, new List<Relation>()))
                                .Where(n => permissions == null || permissions.All(pm => Can(n, currentUserMemberships, new List<Relation>(), pm)))
                                .ToList();
        }

        protected List<T> GetFilteredFeedEntities<T>(IEnumerable<T> entities, string currentUserId, FeedEntityFilter? filter, int page, int pageSize) where T : FeedEntity
        {
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);

            var filteredEntities = GetFilteredFeedEntities(entities, currentUserMemberships, currentUserId, filter);
            return GetPage(filteredEntities, page, pageSize);
        }

        protected List<T> GetFilteredFeedEntities<T>(IEnumerable<T> entities, string currentUserId, FeedEntityFilter? filter = null) where T : FeedEntity
        {
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);

            return GetFilteredFeedEntities(entities, currentUserMemberships, currentUserId, filter);
        }

        protected List<T> GetFilteredFeedEntities<T>(IEnumerable<T> entities, IEnumerable<Membership> currentUserMemberships, string currentUserId, FeedEntityFilter? filter = null) where T : FeedEntity
        {
            return entities.Where(e => CanSeeFeedEntity(e, currentUserMemberships, currentUserId))
                           .Where(e => IsFeedEntityInFilter(e, filter, currentUserId))
                           .ToList();
        }

        protected List<Document> GetFilteredDocuments(IEnumerable<Document> documents, FeedEntitySet feedEntitySet, string currentUserId, DocumentFilter? type, int page, int pageSize)
        {
            return GetPage(GetFilteredDocuments(documents, feedEntitySet, currentUserId, type), page, pageSize);
        }

        protected List<Document> GetFilteredJoglDocs(IEnumerable<Document> documents, string currentUserId)
        {
            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            return documents.Where(d => CanSeeFeedEntity(d, currentUserMemberships, currentUserId)).ToList();
        }

        protected List<Document> GetFilteredDocuments(IEnumerable<Document> documents, FeedEntitySet feedEntitySet, string currentUserId, DocumentFilter? type = null, FeedEntityFilter? filter = null)
        {
            var entityIds = documents.Select(d => d.FeedId).ToList();

            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            var currentUserAttendances = _eventAttendanceRepository.List(a => a.UserId == currentUserId && !a.Deleted);
            var allRelations = _relationRepository.List(r => (entityIds.Contains(r.SourceCommunityEntityId) || entityIds.Contains(r.TargetCommunityEntityId)) && !r.Deleted);

            return GetFilteredDocuments(documents, feedEntitySet, allRelations, currentUserMemberships, currentUserAttendances, currentUserId, type, filter);
        }

        protected List<Document> GetFilteredDocuments(IEnumerable<Document> documents, string currentUserId, DocumentFilter? type = null)
        {
            var entityIds = documents.Select(d => d.FeedId).ToList();
            var feedEntitySet = _feedEntityService.GetFeedEntitySet(entityIds);

            return GetFilteredDocuments(documents, feedEntitySet, currentUserId, type);
        }

        protected List<Document> GetFilteredDocuments(IEnumerable<Document> documents, FeedEntitySet feedEntitySet, IEnumerable<Relation> allRelations, IEnumerable<Membership> currentUserMemberships, IEnumerable<EventAttendance> currentUserAttendances, string currentUserId, DocumentFilter? type = null, FeedEntityFilter? filter = null)
        {
            var entityIds = documents.Select(d => d.FeedId).ToList();
            var entityRelations = allRelations.Where(r => (entityIds.Contains(r.TargetCommunityEntityId) || entityIds.Contains(r.SourceCommunityEntityId)) && !r.Deleted);
            return documents.Where(d => IsFeedEntityInFilter(d, filter, currentUserId))
                            .Where(d => !d.Deleted)
                            .Where(d => d.Status != ContentEntityStatus.Draft)
                            .Where(d =>
                            {
                                if (type == null)
                                    return true;

                                switch (type)
                                {
                                    case DocumentFilter.Document:
                                        return d.Type == DocumentType.Document;
                                    case DocumentFilter.File:
                                        return d.Type == DocumentType.Document && !IsMedia(d);
                                    case DocumentFilter.Media:
                                        return d.Type == DocumentType.Document && IsMedia(d);
                                    case DocumentFilter.Link:
                                        return d.Type == DocumentType.Link;
                                    case DocumentFilter.JoglDoc:
                                        return d.Type == DocumentType.JoglDoc;
                                    default:
                                        return false;
                                }
                            })
                            .Where(d => CanSeeDocument(d, _feedEntityService.GetEntityFromLists(d.FeedEntityId, feedEntitySet), currentUserMemberships, currentUserAttendances, entityRelations, currentUserId))
                            .ToList();
        }

        private bool IsFeedEntityInFilter(FeedEntity e, FeedEntityFilter? filter, string currentUserId)
        {
            if (filter == null)
                return true;

            switch (filter)
            {
                case FeedEntityFilter.CreatedByUser:
                    return e.CreatedByUserId == currentUserId;
                case FeedEntityFilter.SharedWithUser:
                    return e.UserVisibility?.Any(uv => uv.UserId == currentUserId) == true;
                default:
                    return true;
            }
        }

        protected bool IsMedia(Document d)
        {
            if (string.IsNullOrEmpty(d.Filename))
                return false;

            var extensions = ".jpg,.jpeg,.png,.gif,.webp,.bmp,.tiff,.mp3,.wav,.aac,.ogg,.flac,.mp4,.webm,.avi,.mov,.heic,.heif".Split(",");
            try
            {
                var ext = Path.GetExtension(d.Filename).ToLower();
                return extensions.Contains(ext);
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected bool CanSeeDocument(Document document, FeedEntity feedEntity, IEnumerable<Membership> currentUserMemberships, IEnumerable<EventAttendance> currentUserAttendances, IEnumerable<Relation> entityRelations, string userId)
        {
            if (feedEntity == null)
                return false;

            switch (document.Type)
            {
                case DocumentType.JoglDoc:
                    return CanSeeFeedEntity(document, currentUserMemberships, userId);
                default:
                    switch (feedEntity.FeedType)
                    {
                        case FeedType.Document:
                            var parentDocVisibility = GetFeedEntityVisibility(feedEntity as Document, currentUserMemberships, userId);
                            return parentDocVisibility != null;
                        case FeedType.Event:
                            return CanSeeEvent(feedEntity as Event, currentUserMemberships, currentUserAttendances, userId);
                        case FeedType.Channel:
                            var membership = currentUserMemberships.FirstOrDefault(m => m.CommunityEntityId == feedEntity.Id.ToString());
                            return Can(feedEntity as Channel, membership, null, Permission.Read);
                        case FeedType.Need:
                            return true;//TODO fix
                                        //return Can(feedEntity as Need, currentUserMemberships, null, Permission.Read);
                        default:
                            return Can(feedEntity as CommunityEntity, currentUserMemberships, entityRelations, Permission.Read);
                    }
            }
        }

        protected bool CanSeeFeedEntity(FeedEntity entity, IEnumerable<Membership> currentUserMemberships, string userId)
        {
            if (entity == null)
                return false;

            var docVisibility = GetFeedEntityVisibility(entity, currentUserMemberships, userId);
            return docVisibility != null;
        }

        protected bool CanSeeEvent(Event ev, IEnumerable<Membership> currentUserMemberships, IEnumerable<EventAttendance> eventAttendances, string userId)
        {
            switch (ev.Visibility)
            {
                case EventVisibility.Public:
                    return true;
                case EventVisibility.Container:
                    return currentUserMemberships.Any(m => m.CommunityEntityId == ev.CommunityEntityId || eventAttendances.Any(a => a.Status == AttendanceStatus.Yes && a.CommunityEntityId == m.CommunityEntityId && !string.IsNullOrEmpty(a.CommunityEntityId)))
                        || eventAttendances.Any(a => a.UserId == userId && !string.IsNullOrEmpty(a.UserId));
                case EventVisibility.Private:
                    return eventAttendances.Any(a => a.UserId == userId && !string.IsNullOrEmpty(a.UserId));
                default:
                    throw new Exception($"Cannot filter event for visibility {ev.Visibility}");
            }
        }

        protected List<Event> GetFilteredEvents(IEnumerable<Event> events, IEnumerable<EventAttendance> eventAttendances, IEnumerable<Membership> currentUserMemberships, string currentUserId, List<EventTag> tags, int page, int pageSize)
        {
            return GetPage(GetFilteredEvents(events, eventAttendances, currentUserMemberships, currentUserId, tags), page, pageSize);
        }

        protected List<Event> GetFilteredEvents(IEnumerable<Event> events, IEnumerable<EventAttendance> eventAttendances, IEnumerable<Membership> currentUserMemberships, string currentUserId, List<EventTag> tags = null)
        {
            var filteredEvents = events
                                              .Where(e =>
                                              {
                                                  if (tags == null)
                                                      return true;

                                                  if (tags.Contains(EventTag.Attending))
                                                      if (!eventAttendances.Any(a => a.EventId == e.Id.ToString() && a.UserId == currentUserId && a.Status == AttendanceStatus.Yes))
                                                          return false;

                                                  if (tags.Contains(EventTag.Invited))
                                                      if (!eventAttendances.Any(a => a.EventId == e.Id.ToString() && a.UserId == currentUserId && a.Status == AttendanceStatus.Pending))
                                                          return false;

                                                  if (tags.Contains(EventTag.Rejected))
                                                      if (!eventAttendances.Any(a => a.EventId == e.Id.ToString() && a.UserId == currentUserId && a.Status == AttendanceStatus.No))
                                                          return false;

                                                  if (tags.Contains(EventTag.Attendee))
                                                      if (!eventAttendances.Any(a => a.EventId == e.Id.ToString() && a.UserId == currentUserId && a.Status == AttendanceStatus.Yes && a.AccessLevel == AttendanceAccessLevel.Member))
                                                          return false;

                                                  if (tags.Contains(EventTag.Organizer))
                                                      if (!eventAttendances.Any(a => a.EventId == e.Id.ToString() && a.UserId == currentUserId && a.Status == AttendanceStatus.Yes && a.AccessLevel == AttendanceAccessLevel.Admin))
                                                          return false;

                                                  if (tags.Contains(EventTag.Speaker))
                                                      if (!eventAttendances.Any(a => a.EventId == e.Id.ToString() && a.UserId == currentUserId && a.Status == AttendanceStatus.Yes && a.Labels?.Contains(LABEL_SPEAKER) == true))
                                                          return false;

                                                  if (tags.Contains(EventTag.Online))
                                                      if (string.IsNullOrEmpty(e.MeetingURL) && string.IsNullOrEmpty(e.GeneratedMeetingURL))
                                                          return false;

                                                  if (tags.Contains(EventTag.Physical))
                                                      if (e.Location == null)
                                                          return false;

                                                  return true;
                                              })
                                              .Where(e => CanSeeEvent(e, currentUserMemberships, eventAttendances.Where(ea => ea.EventId == e.Id.ToString()), currentUserId))
                                              .ToList();

            return filteredEvents;
        }

        protected List<EventAttendance> GetFilteredAttendances(IEnumerable<EventAttendance> eventAttendances, string search, int page, int pageSize)
        {
            var filteredAttendances = eventAttendances.Where(e => string.IsNullOrEmpty(search) || e.User == null || e.User.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                                                      .Where(e => string.IsNullOrEmpty(search) || e.CommunityEntity == null || e.CommunityEntity.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                                                      .Where(e => string.IsNullOrEmpty(search) || e.UserEmail == null || e.UserEmail.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                                                      .ToList();

            return GetPage(filteredAttendances, page, pageSize);
        }


        //protected List<ContentEntity> GetFilteredContentEntities(IEnumerable<ContentEntity> contentEntities, string currentUserId, string search = null, ContentEntityType? type = null)
        //{
        //    var sourceEntityRelations = _relationRepository.ListForSourceIds(contentEntities.Select(p => p.FeedId).ToList());
        //    var targetEntityRelations = _relationRepository.ListForTargetIds(contentEntities.Select(p => p.FeedId).ToList());

        //    return GetFilteredContentEntities(contentEntities, sourceEntityRelations, targetEntityRelations, currentUserId, search, type);
        //}

        //protected List<ContentEntity> GetFilteredContentEntities(IEnumerable<ContentEntity> contentEntities, string currentUserId, string search, ContentEntityType? type, int page, int pageSize)
        //{
        //    return GetFilteredContentEntities(contentEntities, currentUserId, search, type)
        //                             .Skip((page - 1) * pageSize)
        //                             .Take(pageSize)
        //                             .ToList();
        //}

        protected List<ContentEntity> GetFilteredContentEntities(IEnumerable<ContentEntity> contentEntities, /*IEnumerable<Relation> sourceEntityRelations, IEnumerable<Relation> targetEntityRelations,*/ string currentUserId, string search, ContentEntityType? type)
        {
            //var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            var filteredDocuments = contentEntities.Where(p => string.IsNullOrEmpty(search) || p.Text?.Contains(search, StringComparison.CurrentCultureIgnoreCase) == true)
                                              .Where(p => !p.Deleted)
                                              .Where(p => type == null || p.Type == type)
                                              .Where(p => p.Status == ContentEntityStatus.Active)
                                              //.Where(p => CanSeeContentEntity(p, currentUserMemberships, sourceEntityRelations, targetEntityRelations, currentUserId))
                                              .ToList();

            return filteredDocuments;
        }

        protected List<T> GetPage<T>(IEnumerable<T> entities, int page, int pageSize)
        {
            return GetPage<T>(entities, 0, page, pageSize);
        }

        protected List<T> GetPage<T>(IEnumerable<T> entities, int offset, int page, int pageSize)
        {
            return entities.Skip(((page - 1) * pageSize) + offset)
                           .Take(pageSize)
                           .ToList();
        }

        protected bool IsNeedForUser(Need n, string userId)
        {
            return n.CreatedByUserId == userId;
        }

        protected bool IsEventForUser(Event e, IEnumerable<EventAttendance> eventAttendances, string userId)
        {
            return e.CreatedByUserId == userId || eventAttendances.Any(ea => ea.UserId == userId && ea.Status == AttendanceStatus.Yes && !ea.Deleted);
        }

        protected DiscussionStats GetDiscussionStats(string currentUserId, string feedId)
        {
            var contentEntities = _contentEntityRepository.List(ce => ce.FeedId == feedId && !ce.Deleted);

            var currentUserFeedRecord = _userFeedRecordRepository.Get(ufr => ufr.UserId == currentUserId && ufr.FeedId == feedId && !ufr.Deleted);
            var currentUserMentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.OriginFeedId == feedId && !m.Deleted);
            var currentUserContentEntityRecords = _userContentEntityRecordRepository.List(r => r.UserId == currentUserId && r.FeedId == feedId && !r.Deleted);
            var currentUserContentEntityRecordsEntityIds = currentUserContentEntityRecords.Select(ucer => ucer.ContentEntityId);
            var currentUserContentEntityRecordsComments = _commentRepository.List(c => currentUserContentEntityRecordsEntityIds.Contains(c.ContentEntityId) && !c.Deleted);

            var unreadPosts = contentEntities.Count(ce => ce.CreatedUTC > (currentUserFeedRecord?.LastReadUTC ?? DateTime.MaxValue));
            var unreadMentions = currentUserMentions.Count(m => m.Unread);
            var unreadThreads = contentEntities.Count(ce => currentUserContentEntityRecordsComments.Any(c => c.ContentEntityId == ce.Id.ToString() && c.CreatedUTC > (currentUserContentEntityRecords.SingleOrDefault(r => r.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue)));

            return new DiscussionStats
            {
                UnreadPosts = unreadPosts,
                UnreadMentions = unreadMentions,
                UnreadThreads = unreadThreads
            };
        }

        protected async Task DeleteFeedAsync(string id)
        {
            //delete event feeds
            foreach (var ev in _eventRepository.List(c => c.CommunityEntityId == id))
            {
                await DeleteFeedAsync(ev.Id.ToString());
            }

            //delete document feeds
            foreach (var d in _documentRepository.List(d => d.EntityId == id))
            {
                await DeleteFeedAsync(d.Id.ToString());
            }

            //delete need feeds
            foreach (var n in _needRepository.List(n => n.EntityId == id))
            {
                await DeleteFeedAsync(n.Id.ToString());
            }

            await _userFeedRecordRepository.DeleteAsync(ufr => ufr.FeedId == id && !ufr.Deleted);
            await _userContentEntityRecordRepository.DeleteAsync(ucer => ucer.FeedId == id && !ucer.Deleted);
            await _needRepository.DeleteAsync(n => n.EntityId == id && !n.Deleted);
            await _documentRepository.DeleteAsync(d => d.FeedId == id && !d.Deleted);
            await _eventRepository.DeleteAsync(e => e.CommunityEntityId == id && !e.Deleted);

            await _contentEntityRepository.DeleteAsync(ce => ce.FeedId == id && !ce.Deleted);
            await _commentRepository.DeleteAsync(c => c.FeedId == id && !c.Deleted);
            await _mentionRepository.DeleteAsync(m => m.OriginFeedId == id && !m.Deleted);
            await _reactionRepository.DeleteAsync(r => r.FeedId == id && !r.Deleted);
            //TODO await _feedIntegrationRepository.DeleteAsync(i => i.FeedId == id && !i.Deleted);
            await _feedRepository.DeleteAsync(id);
        }

        protected async Task DeleteChannel(string id)
        {
            await DeleteFeedAsync(id);
            await _channelRepository.DeleteAsync(id);
            await _membershipRepository.DeleteAsync(m => m.CommunityEntityId == id && !m.Deleted);
        }

        protected async Task DeleteCommunityEntityAsync(string id)
        {
            //delete channels
            foreach (var channel in _channelRepository.List(c => c.CommunityEntityId == id))
            {
                await DeleteChannelAsync(channel.Id.ToString());
            }

            //delete feed
            await DeleteFeedAsync(id);

            await _relationRepository.DeleteAsync(m => m.SourceCommunityEntityId == id && !m.Deleted);
            await _relationRepository.DeleteAsync(m => m.TargetCommunityEntityId == id && !m.Deleted);
            await _membershipRepository.DeleteAsync(m => m.CommunityEntityId == id && !m.Deleted);
            await _invitationRepository.DeleteAsync(i => i.CommunityEntityId == id && !i.Deleted);
        }

        protected async Task DeleteChannelAsync(string channelId) { }
    }
}