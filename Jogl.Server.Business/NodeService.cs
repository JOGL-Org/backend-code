using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using MongoDB.Bson;

namespace Jogl.Server.Business
{
    public class NodeService : BaseService, INodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IChannelService _channelService;
        private readonly INotificationService _notificationService;

        public NodeService(INodeRepository nodeRepository, IChannelService channelService, INotificationService notificationService, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _nodeRepository = nodeRepository;
            _channelService = channelService;
            _notificationService = notificationService;
        }

        public async Task<string> CreateAsync(Node node)
        {
            var feed = new Feed()
            {
                CreatedUTC = node.CreatedUTC,
                CreatedByUserId = node.CreatedByUserId,
                Type = FeedType.Node
            };

            var id = await _feedRepository.CreateAsync(feed);
            node.Id = ObjectId.Parse(id);
            node.FeedId = id;

            if (node.Onboarding == null)
                node.Onboarding = new OnboardingConfiguration
                {
                    Presentation = new OnboardingPresentation { Items = new List<OnboardingPresentationItem> { } },
                    Questionnaire = new OnboardingQuestionnaire { Items = new List<OnboardingQuestionnaireItem> { } },
                    Rules = new OnboardingRules { Text = string.Empty }
                };

            if (node.Settings == null)
                node.Settings = new List<string>();

            if (node.Tabs == null)
                node.Tabs = new List<string>();

            var nodeId = await _nodeRepository.CreateAsync(node);

            //create node membership record
            var membership = new Membership
            {
                UserId = node.CreatedByUserId,
                CreatedByUserId = node.CreatedByUserId,
                CreatedUTC = node.CreatedUTC,
                AccessLevel = AccessLevel.Owner,
                CommunityEntityId = nodeId,
                CommunityEntityType = CommunityEntityType.Node,
            };

            await _membershipRepository.CreateAsync(membership);

            //create user feed record
            await _userFeedRecordRepository.SetFeedReadAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);

            //create channel
            node.HomeChannelId = await _channelService.CreateAsync(new Channel
            {
                AutoJoin = true,
                Visibility = ChannelVisibility.Open,
                Title = "General",
                IconKey = "table",
                Settings = new List<string> { CONTENT_MEMBER_POST, COMMENT_MEMBER_POST },
                CreatedByUserId = node.CreatedByUserId,
                CreatedUTC = node.CreatedUTC,
                CommunityEntityId = nodeId
            });

            //update entity with home channel id
            await _nodeRepository.UpdateAsync(node);

            return nodeId;
        }

        public Node Get(string nodeId, string userId)
        {
            var node = _nodeRepository.Get(nodeId);
            EnrichNodeData(new Node[] { node }, userId);
            if (node == null)
                return null;

            return node;
        }

        public Node GetDetail(string nodeId, string userId)
        {
            var node = _nodeRepository.Get(nodeId);
            if (node == null)
                return null;

            EnrichNodeDataDetail(new Node[] { node }, userId);
            node.Path = _feedEntityService.GetPath(node, userId);

            return node;
        }

        public List<Node> Autocomplete(string userId, string search, int page, int pageSize)
        {
            var nodes = _nodeRepository.Search(search);
            return GetFilteredNodes(nodes, userId, null, page, pageSize);
        }

        public ListPage<Node> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var nodes = _nodeRepository.SearchSort(search, sortKey, ascending);
            var filteredNodes = GetFilteredNodes(nodes, userId);
            var total = filteredNodes.Count;

            var filteredNeedPage = GetPage(filteredNodes, page, pageSize);
            EnrichNodeData(filteredNeedPage, userId);

            return new ListPage<Node>(filteredNeedPage, total);
        }

        public long Count(string userId, string search)
        {
            var nodes = _nodeRepository.Search(search);
            var filteredNodes = GetFilteredNodes(nodes, userId);

            return filteredNodes.Count;
        }

        public List<Node> ListForUser(string userId, string targetUserId, Permission? permission, string search, int page, int pageSize)
        {
            var ecosystemNodeMemberships = _membershipRepository.List(p => p.UserId == targetUserId && p.CommunityEntityType == CommunityEntityType.Node && !p.Deleted);
            var ecosystemNodeIds = ecosystemNodeMemberships.Select(m => m.CommunityEntityId).ToList();

            var nodes = _nodeRepository.SearchGet(ecosystemNodeIds, search);
            var filteredNodes = GetFilteredNodes(nodes, userId, permission != null ? new List<Permission> { permission.Value } : null, page, pageSize);
            EnrichNodeData(filteredNodes, userId);

            return filteredNodes;
        }

        public List<Node> ListForProject(string userId, string projectId, string search, int page, int pageSize)
        {
            var nodeRelations = _relationRepository.List(r => r.SourceCommunityEntityId == projectId && r.TargetCommunityEntityType == CommunityEntityType.Node && !r.Deleted);
            var nodeIds = nodeRelations.Select(m => m.TargetCommunityEntityId).ToList();

            var nodes = _nodeRepository.SearchGet(nodeIds, search);
            var filteredNodes = GetFilteredNodes(nodes, userId, null, page, pageSize);
            EnrichNodeData(filteredNodes, userId);

            return filteredNodes;
        }

        public List<Node> ListForCommunity(string userId, string communityId, string search, int page, int pageSize)
        {
            var nodeRelations = _relationRepository.List(r => r.SourceCommunityEntityId == communityId && r.TargetCommunityEntityType == CommunityEntityType.Node && !r.Deleted);
            var nodeIds = nodeRelations.Select(m => m.TargetCommunityEntityId).ToList();

            var nodes = _nodeRepository.SearchGet(nodeIds, search);
            var filteredNodes = GetFilteredNodes(nodes, userId, null, page, pageSize);
            EnrichNodeData(filteredNodes, userId);

            return filteredNodes;
        }

        public List<Node> ListForOrganization(string userId, string organizationId, string search, int page, int pageSize)
        {
            var nodeIds = _relationRepository.List(r => r.TargetCommunityEntityId == organizationId && r.SourceCommunityEntityType == CommunityEntityType.Node && !r.Deleted)
              .Select(pn => pn.SourceCommunityEntityId)
              .ToList();

            var nodes = _nodeRepository.SearchGet(nodeIds, search);
            var filteredNodes = GetFilteredNodes(nodes, userId, null, page, pageSize);
            EnrichNodeData(filteredNodes, userId);

            return filteredNodes;
        }

        public async Task UpdateAsync(Node node)
        {
            await _nodeRepository.UpdateAsync(node);
        }

        public async Task DeleteAsync(string id)
        {
            await DeleteCommunityEntityAsync(id);
            await _nodeRepository.DeleteAsync(id);
        }
    }
}