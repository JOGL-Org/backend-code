using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using Jogl.Server.GitHub;
using Jogl.Server.HuggingFace;
using MongoDB.Bson;

namespace Jogl.Server.Business
{
    public class ResourceService : BaseService, IResourceService
    {
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IGitHubFacade _githubFacade;
        private readonly IHuggingFaceFacade _huggingFaceFacade;

        public ResourceService(ICommunityEntityService communityEntityService, IGitHubFacade githubFacade, IHuggingFaceFacade huggingFaceFacade, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _communityEntityService = communityEntityService;
            _githubFacade = githubFacade;
            _huggingFaceFacade = huggingFaceFacade;
        }

        public async Task<string> CreateAsync(Resource resource)
        {
            //create feed
            var feed = new Feed()
            {
                CreatedUTC = resource.CreatedUTC,
                CreatedByUserId = resource.CreatedByUserId,
                Type = FeedType.Resource,
            };

            var id = await _feedRepository.CreateAsync(feed);

            //mark feed write
            await _userFeedRecordRepository.SetFeedWrittenAsync(resource.CreatedByUserId, id, DateTime.UtcNow);

            //create resource
            resource.Id = ObjectId.Parse(id);
            resource.UpdatedUTC = resource.CreatedUTC; //the purpose of this is to always have a value in the UpdatedUTC field, so that sorting by last update works
            await _resourceRepository.CreateAsync(resource);

            //process notifications
            //await _notificationFacade.NotifyCreatedAsync(resource);

            //return
            return id;
        }

        public Resource Get(string resourceId, string userId)
        {
            var resource = _resourceRepository.Get(resourceId);
            if (resource == null)
                return null;

            EnrichResourceData(new List<Resource> { resource }, userId);
            resource.Path = _feedEntityService.GetPath(resource, userId);

            return resource;
        }

        public ListPage<Resource> List(string currentUserId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var resources = _resourceRepository
                .Query(search)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .WithFeedRecordData()
                .Sort(sortKey, ascending)
                .ToList();


            var total = resources.Count;

            var resourcePage = GetPage(resources, page, pageSize);
            EnrichResourceData(resourcePage, currentUserId);
            RecordListings(currentUserId, resourcePage);

            return new ListPage<Resource>(resourcePage, total);
        }

        public long Count(string currentUserId, string search)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _resourceRepository
                .Query(search)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Count();
        }

        public List<Resource> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool ascending, bool recordListings = true, bool enrich = true)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var resources = _resourceRepository
                .Query(search)
                .Filter(n => n.EntityId == entityId)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Sort(sortKey, ascending)
                .Page(page, pageSize)
                .ToList();

            if (enrich)
                EnrichResourceData(resources, currentUserId);
            if (recordListings)
                RecordListings(currentUserId, resources);

            return resources;
        }

        public bool ListForEntityHasNew(string currentUserId, string entityId)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _resourceRepository
                   .Query(n => n.EntityId == entityId)
                   .WithFeedRecordData()
                   .FilterFeedEntities(currentUserId, currentUserMemberships)
                   .Filter(p => p.LastOpenedUTC == null)
                   .Any();
        }

        public ListPage<Resource> ListForNode(string currentUserId, string nodeId, List<string> communityEntityIds, FeedEntityFilter? filter, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var communityEntities = _communityEntityService.List(entityIds);
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var resources = _resourceRepository
                .Query(search)
                .Filter(n => entityIds.Contains(n.EntityId))
                .WithFeedRecordData()
                .FilterFeedEntities(currentUserId, currentUserMemberships, filter)
                .Sort(sortKey, ascending)
                .ToList();

            var resourcePage = GetPage(resources, page, pageSize);
            EnrichResourceData(resourcePage, communityEntities, currentUserId);
            RecordListings(currentUserId, resourcePage);

            return new ListPage<Resource>(resourcePage, resources.Count);
        }

        public bool ListForNodeHasNew(string currentUserId, string nodeId, FeedEntityFilter? filter)
        {
            var entityIds = GetFeedEntityIdsForNode(nodeId);

            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _resourceRepository
                   .Query(n => entityIds.Contains(n.EntityId))
                   .WithFeedRecordData()
                   .FilterFeedEntities(currentUserId, currentUserMemberships, filter)
                   .Filter(n => n.LastOpenedUTC == null)
                   .Any();
        }

        public long CountForNode(string currentUserId, string nodeId, string search)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);

            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _resourceRepository
                .Query(search)
                .Filter(n => entityIds.Contains(n.EntityId))
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Count();
        }

        private void EnrichResourceData(IEnumerable<Resource> resources, string currentUserId)
        {
            var communityEntities = _communityEntityService.List(resources.Select(e => e.EntityId).Distinct());
            EnrichResourceData(resources, communityEntities, currentUserId);
        }

        private void EnrichResourceData(IEnumerable<Resource> resources, IEnumerable<CommunityEntity> communityEntities, string currentUserId)
        {
            var memberships = _membershipRepository.List(m => !m.Deleted && m.UserId == currentUserId);
            var contentEntities = _contentEntityRepository.List(ce => resources.Any(n => n.Id.ToString() == ce.FeedId) && !ce.Deleted);
            var resourceIds = resources.Select(e => e.Id.ToString());
            var userFeedRecords = _userFeedRecordRepository.List(ufr => ufr.UserId == currentUserId && resourceIds.Contains(ufr.FeedId));
            var userContentEntityRecords = _userContentEntityRecordRepository.List(ucer => ucer.UserId == currentUserId && resourceIds.Contains(ucer.FeedId) && !ucer.Deleted);
            var mentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.Unread && resourceIds.Contains(m.OriginFeedId) && !m.Deleted);

            //foreach (var resource in resources)
            //{
            //    var feedRecord = userFeedRecords.SingleOrDefault(ufr => ufr.FeedId == resource.Id.ToString());

            //    resource.CommunityEntity = communityEntities.SingleOrDefault(ce => ce.Id.ToString() == resource.EntityId);
            //    resource.PostCount = contentEntities.Count(ce => ce.FeedId == resource.Id.ToString());
            //    resource.NewPostCount = contentEntities.Count(ce => ce.FeedId == resource.Id.ToString() && ce.CreatedUTC > (feedRecord?.LastReadUTC ?? DateTime.MaxValue));
            //    resource.NewMentionCount = mentions.Count(m => m.OriginFeedId == resource.Id.ToString());
            //    resource.NewThreadActivityCount = contentEntities.Count(ce => ce.FeedId == resource.Id.ToString() && ce.LastActivityUTC > (userContentEntityRecords.SingleOrDefault(ucer => ucer.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue));

            //    if (resource.CommunityEntity != null)
            //    {
            //        var membership = memberships.SingleOrDefault(m => resource.EntityId == m.CommunityEntityId);
            //        resource.CommunityEntity.AccessLevel = membership?.AccessLevel;
            //    }
            //}

            EnrichFeedEntitiesWithVisibilityData(resources);
            EnrichResourcesWithPermissions(resources, currentUserId);
            EnrichEntitiesWithCreatorData(resources);
        }

        public List<Resource> ListForUser(string currentUserId, string targetUserId, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var resources = _resourceRepository.Query(search)
                .Filter(n => n.CreatedByUserId == targetUserId)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Sort(sortKey, ascending)
                .Page(page, pageSize)
                .ToList();

            EnrichResourceData(resources, currentUserId);
            RecordListings(currentUserId, resources);

            return resources;
        }

        public async Task UpdateAsync(Resource resource)
        {
            await _resourceRepository.UpdateAsync(resource);
            //      await _notificationFacade.NotifyUpdatedAsync(resource);
        }

        public async Task DeleteAsync(Resource resource)
        {
            await DeleteFeedAsync(resource.Id.ToString());
            await _resourceRepository.DeleteAsync(resource);
        }

        public async Task<Resource> BuildResourceForRepoAsync(string repoUrl, string? accessToken = default)
        {
            if (repoUrl.Contains("github"))
            {
                var githubRepo = await _githubFacade.GetRepoAsync(repoUrl.Replace("https://github.com/", string.Empty), accessToken);
                if (githubRepo == null)
                    return null;

                var readme = await _githubFacade.GetReadmeAsync(repoUrl.Replace("https://github.com/", string.Empty), accessToken);

                return new Resource
                {
                    Title = githubRepo.FullName,
                    Description = githubRepo.Description,
                    Data = new BsonDocument {
                        { "License", githubRepo.License?.Name ?? "" },
                        { "Source", "Github" },
                        { "Url", githubRepo.HtmlUrl ?? "" },
                        { "Readme", readme ?? "" }
                    },
                };

            }

            if (repoUrl.Contains("huggingface"))
            {
                var huggingfaceRepo = await _huggingFaceFacade.GetRepoAsync(repoUrl.Replace("https://huggingface.co/", string.Empty));
                if (huggingfaceRepo == null)
                    return null;

                var readme = await _huggingFaceFacade.GetReadmeAsync(repoUrl.Replace("https://huggingface.co/", string.Empty), accessToken);

                return new Resource
                {
                    Title = huggingfaceRepo.Title,
                    Description = huggingfaceRepo.Description,
                    Data = new BsonDocument {
                        { "Source", "Huggingface" },
                        { "Url", huggingfaceRepo.Url ?? "" },
                        { "Readme", readme ?? "" }
                    },
                };
            }

            return null;
        }
    }
}