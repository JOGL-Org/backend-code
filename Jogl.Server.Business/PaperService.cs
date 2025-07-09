using Jogl.Server.Data;
using Jogl.Server.DB;
using MongoDB.Bson;
using Jogl.Server.Orcid;
using Jogl.Server.SemanticScholar;
using Jogl.Server.PubMed;
using Jogl.Server.OpenAlex;
using Microsoft.Extensions.Logging;
using System.Text;
using Jogl.Server.Notifications;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public class PaperService : BaseService, IPaperService
    {
        private readonly INotificationService _notificationService;
        private readonly IOrcidFacade _orcidFacade;
        private readonly ISemanticScholarFacade _s2Facade;
        private readonly IPubMedFacade _pubmedFacade;
        private readonly IOpenAlexFacade _openAlexFacade;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly INotificationFacade _notificationFacade;
        private readonly ILogger<PaperService> _logger;

        public PaperService(ICommunityEntityService communityEntityService, INotificationFacade notificationFacade, INotificationService notificationService, IOrcidFacade orcidFacade, ISemanticScholarFacade s2Facade, IPubMedFacade pubMedFacade, IOpenAlexFacade openAlexFacade, ILogger<PaperService> logger, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _notificationService = notificationService;
            _orcidFacade = orcidFacade;
            _s2Facade = s2Facade;
            _pubmedFacade = pubMedFacade;
            _openAlexFacade = openAlexFacade;
            _communityEntityService = communityEntityService;
            _notificationFacade = notificationFacade;
            _logger = logger;
        }

        public async Task<string> CreateAsync(Paper paper)
        {
            //create feed
            var feed = new Feed()
            {
                CreatedUTC = paper.CreatedUTC,
                CreatedByUserId = paper.CreatedByUserId,
                Type = FeedType.Paper,
            };

            var id = await _feedRepository.CreateAsync(feed);

            //mark feed write
            await _userFeedRecordRepository.SetFeedWrittenAsync(paper.CreatedByUserId, id, DateTime.UtcNow);

            //automatically set paper to active 
            paper.Status = ContentEntityStatus.Active;

            //load tags
            await LoadTagsAsync(paper);

            //Attempt to load paper abstract from OA
            if (string.IsNullOrEmpty(paper.Summary) && !string.IsNullOrEmpty(paper.ExternalId))
                paper.Summary = FormatOAWorkAbstract(await _openAlexFacade.GetAbstractFromDOIAsync(paper.ExternalId));

            //create paper
            paper.Id = ObjectId.Parse(id);
            paper.UpdatedUTC = paper.CreatedUTC; //the purpose of this is to always have a value in the UpdatedUTC field, so that sorting by last update works
            await _paperRepository.CreateAsync(paper);

            //process notifications
            await _notificationFacade.NotifyCreatedAsync(paper);

            return id;
        }

        public Paper Get(string paperId, string currentUserId)
        {
            var paper = _paperRepository.Get(paperId);
            if (paper == null)
                return null;

            var entitySet = _feedEntityService.GetFeedEntitySet(paper.FeedId);
            EnrichPaperData(new List<Paper> { paper }, entitySet, currentUserId);
            paper.Path = _feedEntityService.GetPath(paper, currentUserId);
            RecordListing(currentUserId, paper);

            return paper;
        }

        public List<Paper> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool ascending, bool recordListings = true, bool enrich = true)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var papers = _paperRepository
                .Query(search)
                .Filter(p => p.FeedId == entityId)
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Sort(sortKey, ascending)
                .Page(page, pageSize)
                .ToList();

            var entitySet = _feedEntityService.GetFeedEntitySet(entityId);

            if (enrich)
                EnrichPaperData(papers, entitySet, currentUserId);
            if (recordListings)
                RecordListings(currentUserId, papers);

            return papers;
        }

        public bool ListForEntityHasNew(string currentUserId, string entityId)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _paperRepository
                   .Query(p => p.FeedId == entityId)
                   .WithFeedRecordData()
                   .FilterFeedEntities(currentUserId, currentUserMemberships)
                   .Filter(p => p.LastOpenedUTC == null)
                   .Any();
        }

        public List<Paper> ListForAuthor(string currentUserId, string userId, PaperType? type, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var papers = _paperRepository
                .Query(search)
                .Filter(p => p.UserIds.Contains(userId))
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Sort(sortKey, ascending)
                .Page(page, pageSize)
                .ToList();

            var entityIds = papers.Select(p => p.FeedId).ToList();
            var entitySet = _feedEntityService.GetFeedEntitySet(entityIds);

            EnrichPaperData(papers, entitySet, currentUserId);
            RecordListings(currentUserId, papers);

            return papers;
        }

        public ListPage<Paper> ListForNode(string currentUserId, string nodeId, List<string> communityEntityIds, FeedEntityFilter? filter, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var entityIds = GetFeedEntityIdsForNode(nodeId);

            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var papers = _paperRepository
                .Query(search)
                .Filter(p => entityIds.Contains(p.FeedId))
                .WithFeedRecordData()
                .FilterFeedEntities(currentUserId, currentUserMemberships, filter)
                .Sort(sortKey, ascending)
                .ToList();

            var entitySet = _feedEntityService.GetFeedEntitySet(entityIds);

            var filteredPaperPage = GetPage(papers, page, pageSize);
            EnrichPaperData(filteredPaperPage, entitySet, currentUserId);
            RecordListings(currentUserId, filteredPaperPage);

            return new ListPage<Paper>(filteredPaperPage, papers.Count);
        }

        public bool ListForNodeHasNew(string currentUserId, string nodeId, FeedEntityFilter? filter)
        {
            var entityIds = GetFeedEntityIdsForNode(nodeId);

            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _paperRepository
                   .Query(p => entityIds.Contains(p.FeedId))
                   .WithFeedRecordData()
                   .FilterFeedEntities(currentUserId, currentUserMemberships, filter)
                   .Filter(p => p.LastOpenedUTC == null)
                   .Any();
        }

        public long CountForNode(string currentUserId, string nodeId, string search)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            var entities = _communityEntityService.List(entityIds);
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            return _paperRepository
                .Query(search)
                .Filter(p => entityIds.Contains(p.FeedId))
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .Count();
        }

        public async Task UpdateAsync(Paper paper)
        {
            await _paperRepository.UpdateAsync(paper);
            await _notificationFacade.NotifyUpdatedAsync(paper);
        }

        public async Task DeleteAsync(Paper paper)
        {
            await DeleteFeedAsync(paper.Id.ToString());
            await _paperRepository.DeleteAsync(paper);
        }

        public async Task DeleteForExternalSystemAndUserAsync(string userId, ExternalSystem externalSystem)
        {
            var papers = _paperRepository
                .Query(p => p.FeedId == userId && p.ExternalSystem == externalSystem)
                .ToList();

            await _paperRepository.DeleteAsync(papers);
        }

        protected void EnrichPaperData(IEnumerable<Paper> papers, FeedEntitySet feedEntitySet, string currentUserId)
        {
            var users = _userRepository.Get(papers.Select(d => d.CreatedByUserId).Union(papers.SelectMany(d => d.UserIds ?? new List<string>())).ToList());
            var contentEntities = _contentEntityRepository.List(ce => papers.Any(p => p.Id.ToString() == ce.FeedId) && !ce.Deleted);
            //var comments = _commentRepository.List(co => papers.Any(p => p.Id.ToString() == co.FeedId)! && co.Deleted);
            var paperIds = papers.Select(e => e.Id.ToString());
            var userFeedRecords = _userFeedRecordRepository.List(ufr => ufr.UserId == currentUserId && paperIds.Contains(ufr.FeedId));
            var userContentEntityRecords = _userContentEntityRecordRepository.List(ucer => ucer.UserId == currentUserId && paperIds.Contains(ucer.FeedId) && !ucer.Deleted);
            var mentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.Unread && paperIds.Contains(m.OriginFeedId) && !m.Deleted);

            foreach (var paper in papers)
            {
                var feedRecord = userFeedRecords.SingleOrDefault(ufr => ufr.FeedId == paper.Id.ToString());

                paper.FeedEntity = _feedEntityService.GetEntityFromLists(paper.FeedId, feedEntitySet);
                paper.Users = users.Where(u => paper.UserIds?.Contains(u.Id.ToString()) == true).ToList();
                paper.PostCount = contentEntities.Count(ce => ce.FeedId == paper.Id.ToString());
                paper.NewPostCount = contentEntities.Count(ce => ce.FeedId == paper.Id.ToString() && ce.CreatedUTC > (feedRecord?.LastReadUTC ?? DateTime.MinValue));
                paper.NewMentionCount = mentions.Count(m => m.OriginFeedId == paper.Id.ToString());
                paper.NewThreadActivityCount = contentEntities.Count(ce => ce.FeedId == paper.Id.ToString() && ce.LastActivityUTC > (userContentEntityRecords.SingleOrDefault(ucer => ucer.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue));
            }

            EnrichFeedEntitiesWithVisibilityData(papers);
            EnrichPapersWithPermissions(papers, currentUserId);
            EnrichEntitiesWithCreatorData(papers, users);
        }

        protected string FormatOAWorkAbstract(Dictionary<string, List<int>> invertedIndex)
        {
            StringBuilder stringBuilder = new();
            int maxPosition = invertedIndex?.Values?.SelectMany(positions => positions)?.Max() ?? -1;

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

        private async Task LoadTagsAsync(Paper paper)
        {
            paper.TagData = new TagData();
            if (string.IsNullOrEmpty(paper.ExternalId))
                return;

            try
            {
                var taskDoi = Task.Run(async () =>
                {
                    try
                    {
                        var work = await _orcidFacade.GetWorkFromDOI(paper.ExternalId);
                        paper.TagData.DOITags = work.Tags;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("An error occurred reading doi tags: " + ex.ToString());
                    }
                });

                Task taskS2 = Task.Run(async () =>
                {
                    try
                    {
                        var s2Tags = await _s2Facade.ListTagsByDOIAsync(paper.ExternalId);
                        paper.TagData.S2Tags = s2Tags.Tags.Select(s2Tag => new SemanticTag { Category = s2Tag.Category, Source = s2Tag.Source }).ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("An error occurred reading Semantic Scholar tags: " + ex.ToString());
                    }
                });

                Task taskPubmed = Task.Run(async () =>
                {
                    try
                    {
                        var pubmedTags = await _pubmedFacade.GetTagsFromDOI(paper.ExternalId);
                        paper.TagData.PubMedTags = pubmedTags.Select(pmTag => new PubMedTag { DescriptorName = pmTag.DescriptorName, QualifierNames = pmTag.QualifierNames }).ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("An error occurred reading Pubmed tags: " + ex.ToString());
                    }
                });

                Task taskOpenAlex = Task.Run(async () =>
               {
                   try
                   {


                       var openAlexTags = await _openAlexFacade.ListTagsByDOIAsync(paper.ExternalId);
                       paper.TagData.OpenAlexTags = openAlexTags.Select(oaTag => new OpenAlexTag
                       {
                           Id = oaTag.Id,
                           Wikidata = oaTag.Wikidata,
                           DisplayName = oaTag.DisplayName,
                           Level = oaTag.Level,
                           Score = oaTag.Score
                       }).ToList();
                   }
                   catch (Exception ex)
                   {
                       _logger.LogWarning("An error occurred reading OpenAlex tags: " + ex.ToString());
                   }
               });

                await Task.WhenAll(taskDoi, taskS2, taskPubmed, taskOpenAlex);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
            }
        }

        public List<CommunityEntity> ListCommunityEntitiesForNodePapers(string currentUserId, string nodeId, string search, int page, int pageSize)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            var entities = _communityEntityService.List(entityIds);
            var currentUserMemberships = _membershipRepository.Query(m => m.UserId == currentUserId).ToList();
            var papers = _paperRepository
                .Query(search)
                .Filter(p => entityIds.Contains(p.FeedId))
                .FilterFeedEntities(currentUserId, currentUserMemberships)
                .ToList();

            var entitySet = _feedEntityService.GetFeedEntitySet(entityIds);
            EnrichPaperData(papers, entitySet, currentUserId);

            return entities.Where(e => papers.Any(p => p.FeedId == e.Id.ToString())).ToList();
        }
    }
}