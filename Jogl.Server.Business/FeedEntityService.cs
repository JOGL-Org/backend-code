using Jogl.Server.Data.Util;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.OpenAlex.DTO;

namespace Jogl.Server.Business
{
    public class FeedEntityService : BaseService, IFeedEntityService
    {
        protected readonly IWorkspaceRepository _workspaceRepository;
        protected readonly INodeRepository _nodeRepository;
        protected readonly IOrganizationRepository _organizationRepository;

        public FeedEntityService(IWorkspaceRepository workspaceRepository, INodeRepository nodeRepository, IOrganizationRepository organizationRepository, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, null)
        {
            _workspaceRepository= workspaceRepository;
            _nodeRepository =   nodeRepository;
            _organizationRepository = organizationRepository;
        }

        public string GetPrintName(FeedType feedType)
        {
            switch (feedType)
            {
                case FeedType.Node:
                    return "Hub";
                case FeedType.CallForProposal:
                    return "Call for proposals";
                default:
                    return feedType.ToString();
            }
        }

        public FeedEntitySet GetFeedEntitySet(IEnumerable<string> feedIds)
        {
            var count = feedIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().Count();

            var ids = feedIds.Distinct().ToList();
            var communities = count > 0 ? _workspaceRepository.Get(ids) : new List<Workspace>();
            count -= communities.Count();
            var nodes = count > 0 ? _nodeRepository.Get(ids) : new List<Node>();
            count -= nodes.Count();
            var organizations = count > 0 ? _organizationRepository.Get(ids) : new List<Organization>();
            count -= organizations.Count();
            var callsForProposals = count > 0 ? _callForProposalsRepository.Get(ids) : new List<CallForProposal>();
            count -= callsForProposals.Count();
            var needs = count > 0 ? _needRepository.Get(ids) : new List<Need>();
            count -= needs.Count();
            var documents = count > 0 ? _documentRepository.Get(ids) : new List<Document>();
            count -= documents.Count();
            var papers = count > 0 ? _paperRepository.Get(ids) : new List<Paper>();
            count -= papers.Count();
            var events = count > 0 ? _eventRepository.Get(ids) : new List<Event>();
            count -= events.Count();
            var users = count > 0 ? _userRepository.Get(ids) : new List<User>();
            count -= users.Count();
            var channels = count > 0 ? _channelRepository.Get(ids) : new List<Channel>();
            count -= channels.Count();

            return new FeedEntitySet
            {
                Communities = communities,
                Nodes = nodes,
                Organizations = organizations,
                CallsForProposals = callsForProposals,
                Needs = needs,
                Documents = documents,
                Papers = papers,
                Events = events,
                Users = users,
                Channels = channels,
            };
        }

        public FeedEntitySet GetFeedEntitySetExtended(IEnumerable<string> feedIds)
        {
            var channels = _channelRepository.Get(feedIds.ToList());
            var feedIdsPlus = feedIds.Concat(channels.Select(c=>c.CommunityEntityId)).Distinct().ToList();
            var set= GetFeedEntitySet(feedIdsPlus);
            foreach (var c in set.Channels)
            {
                c.CommunityEntity = GetCommunityEntityFromLists(c.CommunityEntityId, set);
            }

            return set;
        }

        public FeedEntitySet GetFeedEntitySetForCommunities(IEnumerable<string> feedIds)
        {
            var count = feedIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().Count();

            var ids = feedIds.Distinct().ToList();
            var communities = count > 0 ? _workspaceRepository.Get(ids) : new List<Workspace>();
            count -= communities.Count();
            var nodes = count > 0 ? _nodeRepository.Get(ids) : new List<Node>();
            count -= nodes.Count();
            var organizations = count > 0 ? _organizationRepository.Get(ids) : new List<Organization>();
            count -= organizations.Count();
            var callsForProposals = count > 0 ? _callForProposalsRepository.Get(ids) : new List<CallForProposal>();
            count -= callsForProposals.Count();

            return new FeedEntitySet
            {
                Communities = communities,
                Nodes = nodes,
                Organizations = organizations,
                CallsForProposals = callsForProposals,
                Needs = new List<Need>(),
                Documents = new List<Document>(),
                Papers = new List<Paper>(),
                Events = new List<Event>(),
                Users = new List<User>(),
                Channels = new List<Channel>()
            };
        }

        public FeedEntitySet GetFeedEntitySet(string feedId)
        {
            var ids = new List<string> { feedId };
            return GetFeedEntitySet(ids);
        }

        public FeedEntity GetFeedEntity(string feedId)
        {
            var set = GetFeedEntitySet(new List<string> { feedId });
            return GetEntityFromLists(feedId, set);
        }

        public FeedEntity GetEntityFromLists(string entityId, FeedEntitySet entitySet)
        {
            return (FeedEntity)entitySet.Communities.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (FeedEntity)entitySet.Nodes.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (FeedEntity)entitySet.Organizations.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (FeedEntity)entitySet.CallsForProposals.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (FeedEntity)entitySet.Needs.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (FeedEntity)entitySet.Documents.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (FeedEntity)entitySet.Papers.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (FeedEntity)entitySet.Events.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (FeedEntity)entitySet.Users.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (FeedEntity)entitySet.Channels.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? null;
        }

        public CommunityEntity GetCommunityEntityFromLists(string entityId, FeedEntitySet entitySet)
        {
            return (CommunityEntity)entitySet.Communities.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (CommunityEntity)entitySet.Nodes.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (CommunityEntity)entitySet.Organizations.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? (CommunityEntity)entitySet.CallsForProposals.SingleOrDefault(p => p.Id.ToString() == entityId)
                ?? null;
        }

        public void PopulateFeedEntities(IEnumerable<ICommunityEntityOwned> entities)
        {
            var feedEntitySet = GetFeedEntitySet(entities.Select(e => e.CommunityEntityId).ToList());
            foreach (var entity in entities)
            {
                entity.CommunityEntity = GetCommunityEntityFromLists(entity.CommunityEntityId, feedEntitySet);
            }
        }

        public async Task UpdateActivityAsync(string entityId, DateTime updatedUTC, string updatedByUserId)
        {
            var feed = _feedRepository.Get(entityId);
            switch (feed.Type)
            {
                case FeedType.Workspace:
                    await _workspaceRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                case FeedType.Node:
                    await _nodeRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                case FeedType.Organization:
                    await _organizationRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                case FeedType.CallForProposal:
                    await _callForProposalsRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                case FeedType.Need:
                    await _needRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                case FeedType.User:
                    await _userRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                case FeedType.Document:
                    await _documentRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                case FeedType.Event:
                    await _eventRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                case FeedType.Paper:
                    await _paperRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                case FeedType.Channel:
                    await _channelRepository.UpdateLastActivityAsync(entityId, updatedUTC, updatedByUserId);
                    break;
                default:
                    throw new Exception($"Cannot update activity on feed type {feed.Type}");
            }
        }

        public List<FeedEntity> GetPath(FeedEntity feedEntity, string currentUserId)
        {
            switch (feedEntity.FeedType)
            {
                case FeedType.Workspace:
                    var parentRelations = _relationRepository.List(r => r.SourceCommunityEntityId == feedEntity.Id.ToString() && r.TargetCommunityEntityType != CommunityEntityType.Organization && !r.Deleted);
                    var parent = GetPathCommunityEntity(parentRelations, currentUserId);

                    if (parent == null)
                        return new List<FeedEntity> { feedEntity };

                    var grandParentRelations = _relationRepository.List(r => r.SourceCommunityEntityId == parent.Id.ToString() && !r.Deleted);
                    var grandparent = GetPathCommunityEntity(grandParentRelations, currentUserId);

                    if (grandparent == null)
                        return new List<FeedEntity>() { parent, feedEntity };

                    return new List<FeedEntity> { grandparent, parent, feedEntity };
                case FeedType.Node:
                case FeedType.CallForProposal:
                case FeedType.User:
                case FeedType.Organization:
                    return new List<FeedEntity> { feedEntity };
                case FeedType.Need:
                    var need = feedEntity as Need;
                    if (need.FeedEntity == null)
                        need.FeedEntity = GetFeedEntity(need.FeedEntityId);

                    return GetPath(need.FeedEntity, currentUserId).Concat(new List<FeedEntity> { need }).ToList();
                case FeedType.Document:
                    var doc = feedEntity as Document;
                    if (doc.FeedEntity == null)
                        doc.FeedEntity = GetFeedEntity(doc.FeedId);

                    if (doc.FeedId == doc.Id.ToString())
                        throw new Exception($"Recursive path detected for document {doc.Id.ToString()}");

                    return GetPath(doc.FeedEntity, currentUserId).Concat(new List<FeedEntity> { doc }).ToList();
                case FeedType.Event:
                    var ev = feedEntity as Event;
                    if (ev.FeedEntity == null)
                        ev.FeedEntity = GetFeedEntity(ev.FeedEntityId);

                    return GetPath(ev.FeedEntity, currentUserId).Concat(new List<FeedEntity> { ev }).ToList();
                case FeedType.Channel:
                    var channel = feedEntity as Channel;
                    if (channel.CommunityEntity == null)
                    {
                        var parentEntitySet = GetFeedEntitySetForCommunities(new List<string> { channel.CommunityEntityId });
                        channel.CommunityEntity = parentEntitySet.CommunityEntities.SingleOrDefault();
                    }
                    return GetPath(channel.CommunityEntity, currentUserId).Concat(new List<FeedEntity> { channel }).ToList();
                case FeedType.Paper:
                default:
                    return new List<FeedEntity>();
            }
        }

        public List<FeedEntity> GetPath(string entityId, string currentUserId)
        {
            var entity = GetFeedEntity(entityId);
            return GetPath(entity, currentUserId);
        }

        private CommunityEntity GetPathCommunityEntity(List<Relation> relations, string currentUserId)
        {
            switch (relations.Count)
            {
                case 0:
                    return null;
                case 1:
                    var rel = relations.Single();
                    return Get(rel.TargetCommunityEntityId, rel.TargetCommunityEntityType);
                default:
                    var targetIds = relations.Select(cr => cr.TargetCommunityEntityId);
                    var targetMemberships = _membershipRepository.List(m => targetIds.Contains(m.CommunityEntityId) && m.UserId == currentUserId && !m.Deleted);

                    var membership = targetMemberships.FirstOrDefault();
                    var membershipRel = membership != null ? relations.SingleOrDefault(r => r.TargetCommunityEntityId == membership.CommunityEntityId) : relations.First();
                    return Get(membershipRel.TargetCommunityEntityId, membershipRel.TargetCommunityEntityType);
            }
        }

        private CommunityEntity Get(string id, CommunityEntityType type)
        {
            switch (type)
            {
                case CommunityEntityType.Workspace: return _workspaceRepository.Get(id);
                case CommunityEntityType.Node: return _nodeRepository.Get(id);
                default:
                    return null;
            }
        }

        public FeedEntity GetEntity(string id)
        {
            var feed = _feedRepository.Get(id);
            if (feed == null)
                return null;

            return GetEntity(id, feed.Type);
        }

        private FeedEntity GetEntity(string id, FeedType type)
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

        public FeedEntity GetEntity(string id, string userId)
        {
            var feed = _feedRepository.Get(id);
            if (feed == null)
                return null;

            return GetEntity(id, feed.Type, userId);
        }

        private FeedEntity GetEntity(string id, FeedType type, string userId)
        {
            switch (type)
            {
                case FeedType.Workspace:
                    var workspace = _workspaceRepository.Get(id);
                    if (workspace == null)
                        return null;

                    EnrichWorkspaceData(new[] { workspace }, userId);
                    return workspace;
                case FeedType.Node:
                    var node = _nodeRepository.Get(id);
                    if (node == null)
                        return null;

                    EnrichNodeData(new[] { node }, userId);
                    return node;

                case FeedType.Organization:
                    var organization= _organizationRepository.Get(id);
                    if (organization == null)
                        return null;

                    EnrichOrganizationData(new[] { organization }, userId);
                    return organization;
                case FeedType.CallForProposal:
                    var cfp= _callForProposalsRepository.Get(id);
                    if (cfp == null)
                        return null;

                    EnrichCallForProposalData(new[] { cfp }, userId);
                    return cfp;
                case FeedType.Need:
                    var need= _needRepository.Get(id);
                    if (need == null)
                        return null;

                    EnrichNeedsWithPermissions(new[] { need }, userId);
                    return need;

                case FeedType.Document:
                    var doc =  _documentRepository.Get(id);
                    if (doc == null)
                        return null;

                    EnrichDocumentsWithPermissions(new[] { doc }, userId);
                    return doc;
                case FeedType.Event:
                    var ev= _eventRepository.Get(id);
                    if (ev == null)
                        return null;

                    EnrichEventsWithPermissions(new[] { ev }, userId);
                    return ev;
                case FeedType.Paper:
                    var paper = _paperRepository.Get(id);
                    if (paper == null)
                        return null;

                    EnrichPapersWithPermissions(new[] { paper }, userId);
                    return paper;
                case FeedType.User:
                    var user = _userRepository.Get(id);
                    if (user == null)
                        return null;

                    EnrichUsersWithPermissions(new[] { user }, userId);
                    return user;
                case FeedType.Channel:
                     var channel= _channelRepository.Get(id);
                    if (channel == null)
                        return null;

                    EnrichChannelsWithPermissions(new  [] { channel }, userId);
                    return channel;
                default:
                    throw new NotImplementedException($"Cannot load entity for type {type}");
            }
        }

        public FeedType GetType(CommunityEntityType type)
        {
            return Enum.Parse<FeedType>(type.ToString());
        }

        public CommunityEntityType GetType(FeedType type)
        {
            return Enum.Parse<CommunityEntityType>(type.ToString());
        }
        
        public Feed GetFeed(string id)
        {
            return _feedRepository.Get(id);
        }
    }
}
