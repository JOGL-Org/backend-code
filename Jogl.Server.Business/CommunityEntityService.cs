using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.DB;

namespace Jogl.Server.Business
{
    public class CommunityEntityService : BaseService, ICommunityEntityService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly INodeRepository _nodeRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly ICommunityEntityFollowingRepository _communityEntityfollowingRepository;

        public CommunityEntityService(IOrganizationRepository organizationRepository, INodeRepository nodeRepository, IWorkspaceRepository workspaceRepository, ICommunityEntityFollowingRepository communityEntityFollowingRepository, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _organizationRepository = organizationRepository;
            _nodeRepository = nodeRepository;
            _workspaceRepository = workspaceRepository;
            _communityEntityfollowingRepository = communityEntityFollowingRepository;
        }

        public List<CommunityEntity> List(IEnumerable<string> ids)
        {
            var idList = ids.ToList();
            var communities = _workspaceRepository.Get(idList);
            var nodes = _nodeRepository.Get(idList);
            var orgs = _organizationRepository.Get(idList);
            var cfps = _callForProposalsRepository.Get(idList);

            var res = new List<CommunityEntity>();
            res.AddRange(communities);
            res.AddRange(nodes);
            res.AddRange(orgs);
            res.AddRange(cfps);

            return res;
        }

        public List<CommunityEntity> List(string id, string currentUserId, Permission? permission, string search, int page, int pageSize)
        {
            var feed = _feedRepository.Get(id);
            var allRelations = _relationRepository.List(r => !r.Deleted);
            var currentUserMemberships = _membershipRepository.List(m => m.UserId == currentUserId && !m.Deleted);
            var currentUserInvitations = _invitationRepository.List(i => !i.Deleted && i.Status == InvitationStatus.Pending && i.InviteeUserId == currentUserId);

            switch (feed?.Type)
            {
                case FeedType.Node:
                    {
                        var communityEntityIds = GetCommunityEntityIdsForNode(allRelations, id);
                        var nodes = _nodeRepository.AutocompleteGet(communityEntityIds, search);
                        var communities = _workspaceRepository.AutocompleteGet(communityEntityIds, search);

                        var filteredNodes = GetFilteredNodes(nodes, allRelations, currentUserMemberships, currentUserInvitations, permission.HasValue ? new List<Permission> { permission.Value } : null);
                        var filteredCommunities = GetFilteredWorkspaces(communities, allRelations, currentUserMemberships, currentUserInvitations, permission.HasValue ? new List<Permission> { permission.Value } : null);

                        var res = new List<CommunityEntity>();
                        res.AddRange(filteredNodes);
                        res.AddRange(filteredCommunities);

                        return GetPage(res, page, pageSize);
                    }
                case FeedType.Workspace:
                    {
                        var nodeIds = GetNodeIdsForCommunity(allRelations, id);
                        var communityEntityIds = GetCommunityEntityIdsForNodes(allRelations, nodeIds);
                        var nodes = _nodeRepository.AutocompleteGet(nodeIds, search);
                        var communities = _workspaceRepository.AutocompleteGet(communityEntityIds, search);

                        var filteredNodes = GetFilteredNodes(nodes, allRelations, currentUserMemberships, currentUserInvitations, permission.HasValue ? new List<Permission> { permission.Value } : null);
                        var filteredCommunities = GetFilteredWorkspaces(communities, allRelations, currentUserMemberships, currentUserInvitations, permission.HasValue ? new List<Permission> { permission.Value } : null);

                        var res = new List<CommunityEntity>();
                        res.AddRange(filteredNodes);
                        res.AddRange(filteredCommunities);

                        return GetPage(res, page, pageSize);
                    }
                default:
                    {
                        var nodes = _nodeRepository.AutocompleteList(n => !n.Deleted, search);
                        var communities = _workspaceRepository.AutocompleteList(c => !c.Deleted, search);

                        var filteredNodes = GetFilteredNodes(nodes, allRelations, currentUserMemberships, currentUserInvitations, permission.HasValue ? new List<Permission> { permission.Value } : null);
                        var filteredCommunities = GetFilteredWorkspaces(communities, allRelations, currentUserMemberships, currentUserInvitations, permission.HasValue ? new List<Permission> { permission.Value } : null);

                        var res = new List<CommunityEntity>();
                        res.AddRange(filteredNodes);
                        res.AddRange(filteredCommunities);

                        return GetPage(res, page, pageSize);
                    }
            }
        }

        public async Task UpdateAsync(CommunityEntity entity)
        {
            switch (entity.Type)
            {
                case CommunityEntityType.Workspace:
                    await _workspaceRepository.UpdateAsync((Workspace)entity);
                    break;

                case CommunityEntityType.Node:
                    await _nodeRepository.UpdateAsync((Node)entity);
                    break;

                case CommunityEntityType.Organization:
                    await _organizationRepository.UpdateAsync((Organization)entity);
                    break;

                case CommunityEntityType.CallForProposal:
                    await _callForProposalsRepository.UpdateAsync((CallForProposal)entity);
                    break;

                default:
                    throw new NotImplementedException($"Cannot update entity for type {entity.Type}");
            }
        }

        private CommunityEntityType? GetType(FeedType type)
        {
            CommunityEntityType t;
            if (!Enum.TryParse(type.ToString(), out t))
                return null;

            return t;
        }

        private CommunityEntity Get(string id, string userId)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var feed = _feedRepository.Get(id);
            var type = GetType(feed.Type);
            if (!type.HasValue)
                return null;

            return Get(id, type.Value, userId);
        }

        private CommunityEntity Get(string id, FeedType feedType, string userId)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var type = GetType(feedType);
            if (!type.HasValue)
                return null;

            return Get(id, type.Value, userId);
        }

        private CommunityEntity Get(string feedId, CommunityEntityType type, string userId)
        {
            if (string.IsNullOrEmpty(feedId))
                return null;

            switch (type)
            {
                case CommunityEntityType.Workspace:
                    var community = _workspaceRepository.Get(feedId);
                    if (!string.IsNullOrEmpty(userId))
                        EnrichWorkspaceData(new List<Workspace> { community }, userId);

                    return community;

                case CommunityEntityType.Node:
                    var node = _nodeRepository.Get(feedId);
                    if (!string.IsNullOrEmpty(userId))
                        EnrichNodeData(new Node[] { node }, userId);

                    return node;

                case CommunityEntityType.CallForProposal:
                    var cfp = _callForProposalsRepository.Get(feedId);
                    if (!string.IsNullOrEmpty(userId))
                        EnrichCallForProposalData(new CallForProposal[] { cfp }, userId);

                    return cfp;

                case CommunityEntityType.Organization:
                    var org = _organizationRepository.Get(feedId);
                    if (!string.IsNullOrEmpty(userId))
                        EnrichOrganizationData(new Organization[] { org }, userId);

                    return org;

                default:
                    throw new NotImplementedException($"Cannot return entity for type {type}");
            }
        }

        public CommunityEntity Get(string id)
        {
            return Get(id, null);
        }

        public CommunityEntity GetEnriched(string id, string userId)
        {
            return Get(id, userId);
        }

        public CommunityEntity Get(string id, CommunityEntityType type)
        {
            return Get(id, type, null);
        }

        public CommunityEntity GetEnriched(string id, CommunityEntityType type, string userId)
        {
            return Get(id, type, userId);
        }

        public CommunityEntity Get(string id, FeedType type)
        {
            return Get(id, type, null);
        }

        public CommunityEntity GetEnriched(string id, FeedType type, string userId)
        {
            return Get(id, type, userId);
        }

        public FeedEntity GetEntity(string id, FeedType type)
        {
            switch (type)
            {
                case FeedType.Workspace:
                    return _workspaceRepository.Get(id);

                case FeedType.Node:
                    return _nodeRepository.Get(id);

                case FeedType.Organization:
                    return _organizationRepository.Get(id);

                case FeedType.CallForProposal:
                    return _callForProposalsRepository.Get(id);

                case FeedType.Need:
                    return _needRepository.Get(id);

                case FeedType.Document:
                    return _documentRepository.Get(id);

                case FeedType.Event:
                    return _eventRepository.Get(id);

                case FeedType.Paper:
                    return _paperRepository.Get(id);

                case FeedType.User:
                    return _userRepository.Get(id);

                case FeedType.Channel:
                    return _channelRepository.Get(id);

                default:
                    throw new NotImplementedException($"Cannot load entity for type {type}");
            }
        }

        public FeedEntity GetFeedEntity(string feedId)
        {
            var feed = _feedRepository.Get(feedId);
            return GetEntity(feedId, feed.Type);
        }

        public List<Permission> ListPermissions(string id, string userId)
        {
            var feed = _feedRepository.Get(id);
            if (feed == null)
                return new List<Permission>();

            return ListPermissions(id, feed.Type, userId);
        }

        public bool HasPermission(string id, Permission p, string userId)
        {
            var permissions = ListPermissions(id, userId);
            if (permissions == null)
                return false;

            return permissions.Contains(p);
        }

        private List<Permission> ListPermissions(string id, FeedType type, string userId)
        {
            switch (type)
            {
                case FeedType.Workspace:
                    var community = _workspaceRepository.Get(id);
                    EnrichCommunityEntitiesWithMembershipData(new Workspace[] { community }, userId);
                    return community.Permissions;

                case FeedType.Node:
                    var node = _nodeRepository.Get(id);
                    EnrichCommunityEntitiesWithMembershipData(new Node[] { node }, userId);
                    return node.Permissions;

                case FeedType.Organization:
                    var organization = _organizationRepository.Get(id);
                    EnrichCommunityEntitiesWithMembershipData(new Organization[] { organization }, userId);
                    return organization.Permissions;

                case FeedType.CallForProposal:
                    var cfp = _callForProposalsRepository.Get(id);
                    EnrichCFPWithMembershipData(new CallForProposal[] { cfp }, userId);
                    return cfp.Permissions;

                case FeedType.Need:
                    var need = _needRepository.Get(id);
                    need.CommunityEntity = Get(need.EntityId, userId);
                    EnrichNeedsWithPermissions(new Need[] { need }, userId);
                    return need.Permissions;

                case FeedType.Document:
                    var doc = _documentRepository.Get(id);
                    doc.FeedEntity = GetFeedEntity(doc.FeedEntityId);
                    EnrichDocumentsWithPermissions(new Document[] { doc }, userId);
                    return doc.Permissions;

                case FeedType.Event:
                    var ev = _eventRepository.Get(id);
                    var attendances = _eventAttendanceRepository.List(a => a.EventId == id && a.UserId == userId && !a.Deleted);
                    EnrichEventsWithPermissions(new Event[] { ev }, attendances, userId);
                    return ev.Permissions;

                case FeedType.Paper:
                    var paper = _paperRepository.Get(id);
                    EnrichPapersWithPermissions(new Paper[] { paper }, userId);
                    return paper.Permissions;

                case FeedType.User:
                    return new List<Permission> { };

                case FeedType.Channel:
                    var channel = _channelRepository.Get(id);
                    EnrichChannelData(new Channel[] { channel }, userId);
                    return channel.Permissions;

                default:
                    throw new NotImplementedException($"Cannot load permissions for feed type {type}");
            }
        }

        public CommunityEntityFollowing GetFollowing(string userIdFrom, string userIdTo)
        {
            return _communityEntityfollowingRepository.GetFollowing(userIdFrom, userIdTo);
        }

        public async Task<string> CreateFollowingAsync(CommunityEntityFollowing following)
        {
            return await _communityEntityfollowingRepository.CreateAsync(following);
        }

        public async Task DeleteFollowingAsync(string followingId)
        {
            await _communityEntityfollowingRepository.DeleteAsync(followingId);
        }
        public string GetPrintName(CommunityEntityType communityEntityType)
        {
            switch (communityEntityType)
            {
                case CommunityEntityType.Node:
                    return "Hub";
                case CommunityEntityType.CallForProposal:
                    return "Call for proposals";
                default:
                    return communityEntityType.ToString();
            }
        }
    }
}