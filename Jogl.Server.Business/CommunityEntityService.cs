using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
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

        public List<CommunityEntity> List(string id, string currentUserId, Permission? permission, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var feed = _feedRepository.Get(id);
            var allRelations = _relationRepository.Query(r => true).ToList();
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var currentUserInvitations = _invitationRepository.Query(i => i.InviteeUserId == currentUserId)
                .Filter(i => i.Status == InvitationStatus.Pending)
                .ToList();

            switch (feed?.Type)
            {
                case FeedType.Workspace:
                    {
                        var entityIds = GetCommunityEntityIdsForCommunity(allRelations, id);

                        var communities = _workspaceRepository
                            .QueryAutocomplete(search)
                            .Filter(n => entityIds.Contains(n.Id.ToString()))
                            .Sort(sortKey, sortAscending)
                            .ToList();

                        var filteredCommunities = GetFilteredWorkspaces(communities, allRelations, currentUserMemberships, permission.HasValue ? new List<Permission> { permission.Value } : null);
                        var res = new List<CommunityEntity>();
                        res.AddRange(filteredCommunities);
                        return GetPage(res, page, pageSize);
                    }
                case FeedType.Node:
                default:
                    {
                        var entityIds = GetCommunityEntityIdsForNode(allRelations, id);

                        var nodes = _nodeRepository
                            .QueryAutocomplete(search)
                            .Filter(n => !entityIds.Any() || entityIds.Contains(n.Id.ToString()))
                            .Sort(sortKey, sortAscending)
                            .ToList();
                        var communities = _workspaceRepository
                            .QueryAutocomplete(search)
                            .Filter(n => !entityIds.Any() || entityIds.Contains(n.Id.ToString()))
                            .Sort(sortKey, sortAscending)
                            .ToList();

                        var filteredNodes = GetFilteredNodes(nodes, allRelations, currentUserMemberships, permission.HasValue ? new List<Permission> { permission.Value } : null);
                        var filteredCommunities = GetFilteredWorkspaces(communities, allRelations, currentUserMemberships, permission.HasValue ? new List<Permission> { permission.Value } : null);

                        var res = new List<CommunityEntity>();
                        res.AddRange(filteredNodes);
                        res.AddRange(filteredCommunities);
                        EnrichCommunityEntitiesWithMembershipData(res, currentUserId);
                        EnrichCommunityEntitiesWithInvitationData(res, currentUserInvitations, currentUserId);

                        return GetPage(res, page, pageSize);
                    }
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
                    EnrichNeedsWithPermissions(new Need[] { need }, userId);
                    return need.Permissions;

                case FeedType.Document:
                    var doc = _documentRepository.Get(id);
                    doc.FeedEntity = _feedEntityService.GetEntity(doc.FeedEntityId);
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
                    if (id == userId)
                        return new List<Permission> { Permission.Read, Permission.ManageDocuments, Permission.ManageLibrary };
                    else
                        return new List<Permission> { Permission.Read };

                case FeedType.Channel:
                    var channel = _channelRepository.Get(id);
                    EnrichChannelData(new Channel[] { channel }, userId);
                    return channel.Permissions;

                default:
                    throw new NotImplementedException($"Cannot load permissions for feed type {type}");
            }
        }
    }
}