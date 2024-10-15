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
using System.Linq.Expressions;
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

        public async Task<string> CreateAsync(string entityId, Paper paper)
        {
            var existingPaper = _paperRepository.Get(p => !string.IsNullOrEmpty(paper.ExternalId) && p.ExternalId == paper.ExternalId && !p.Deleted);
            if (existingPaper == null)
            {
                return await CreateNewAsync(entityId, paper);
            }
            else
            {
                await AssociateAsync(entityId, existingPaper.Id.ToString(), paper.UserIds);
                return existingPaper.Id.ToString();
            }
        }

        private async Task<string> CreateNewAsync(string entityId, Paper paper)
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

            switch (paper.ExternalSystem)
            {
                case ExternalSystem.None:
                    paper.FeedIds = new List<string> { };
                    break;
                default:
                    paper.FeedIds = new List<string> { entityId };

                    //automatically set paper to active 
                    paper.Status = ContentEntityStatus.Active;

                    //load tags
                    await LoadTagsAsync(paper);

                    //Attempt to load paper abstract from OA
                    if (string.IsNullOrEmpty(paper.Summary))
                        paper.Summary = FormatOAWorkAbstract(await _openAlexFacade.GetAbstractFromDOIAsync(paper.ExternalId));

                    break;
            }


            switch (paper.Status)
            {
                case ContentEntityStatus.Draft:
                    break;
                default:
                    //await CreateAutoContentEntityAsync(new ContentEntity
                    //{
                    //    Id = ObjectId.Parse(id),
                    //    Title = paper.Title,
                    //    Summary = paper.Summary,
                    //    FeedId = entityId,
                    //    Visibility = ContentEntityVisibility.Public,
                    //    Type = ContentEntityType.Paper,
                    //    CreatedByUserId = paper.CreatedByUserId,
                    //    CreatedUTC = paper.CreatedUTC,
                    //});

                    break;
            }

            //create paper
            paper.Id = ObjectId.Parse(id);
            await _notificationFacade.NotifyAddedAsync(paper);
            await _paperRepository.CreateAsync(paper);

            return id;
        }

        public Paper GetDraft(string entityId, string userId)
        {
            var draft = _paperRepository.Get(p => !p.Deleted && p.Status == ContentEntityStatus.Draft && p.FeedIds.Contains(entityId) && p.CreatedByUserId == userId);
            EnrichPaperData(new List<Paper> { draft }, userId);

            return draft;
        }

        public async Task AssociateAsync(string entityId, string paperId, List<string> userIds = null)
        {
            var paper = _paperRepository.Get(paperId);
            if (paper.FeedIds == null)
                paper.FeedIds = new List<string>();
            if (paper.FeedIds.Contains(entityId))
                return;

            if (userIds != null && userIds.Any())
                paper.UserIds = paper.UserIds.Union(userIds).ToList();

            //process notifications
            await _notificationService.NotifyPaperAssociatedAsync(paper, entityId);
            paper.FeedIds.Add(entityId);
            await _notificationFacade.NotifyAddedAsync(paper);

            await _paperRepository.UpdateAsync(paper);
        }

        public async Task DisassociateAsync(string entityId, string paperId)
        {
            var paper = _paperRepository.Get(paperId);
            if (paper.FeedIds == null)
                paper.FeedIds = new List<string>();
            if (!paper.FeedIds.Contains(entityId))
                return;

            paper.FeedIds.Remove(entityId);
            await _paperRepository.UpdateAsync(paper);
        }

        public Paper Get(string paperId, string currentUserId)
        {
            var paper = _paperRepository.Get(paperId);
            EnrichPaperData(new List<Paper> { paper }, currentUserId);
            RecordListing(currentUserId, paper);

            return paper;
        }

        public List<Paper> ListForEntity(string currentUserId, string entityId, PaperType? type, List<PaperTag>? tags, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var feed = _feedRepository.Get(entityId);
            var papers = _paperRepository.SearchListSort(p => p.FeedIds.Contains(entityId) && !p.Deleted, sortKey, ascending, search);

            var filteredPapers = papers;
            filteredPapers = GetFilteredPapers(papers);
            TagPapers(filteredPapers, entityId, CollectCommunityEntityTypeDictionary(feed), currentUserId);
            filteredPapers = GetFilteredPapers(filteredPapers, tags);
            filteredPapers = GetPage(filteredPapers, page, pageSize);
            EnrichPaperData(filteredPapers, currentUserId);
            RecordListings(currentUserId, filteredPapers);

            return filteredPapers;
        }

        public List<Paper> ListForAuthor(string currentUserId, string userId, PaperType? type, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var papers = _paperRepository.SearchListSort(p => p.UserIds.Contains(userId) && !p.Deleted, sortKey, ascending, search);

            var filteredPapers = GetFilteredPapers(papers);
            filteredPapers = GetPage(filteredPapers, page, pageSize);
            EnrichPaperData(filteredPapers, currentUserId);
            RecordListings(currentUserId, filteredPapers);

            return filteredPapers;
        }

        public ListPage<Paper> ListForCommunity(string currentUserId, string communityId, List<CommunityEntityType> types, List<string> communityEntityIds, PaperType? type, List<PaperTag>? tags, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var entityIds = GetCommunityEntityIdsForCommunity(communityId);
            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var entities = _communityEntityService.List(entityIds);
            var papers = _paperRepository.SearchListSort(p => p.FeedIds.Any(id => entityIds.Contains(id)) && !p.Deleted, sortKey, ascending, search);

            var filteredPapers = papers;
            filteredPapers = GetFilteredPapers(papers);
            TagPapers(filteredPapers, communityId, entities.ToDictionary(e => e.Id.ToString(), e => e.Type), currentUserId);
            filteredPapers = GetFilteredPapers(filteredPapers, tags);
            var total = filteredPapers.Count;

            var filteredPaperPage = GetPage(filteredPapers, page, pageSize);
            EnrichPaperData(filteredPaperPage, currentUserId);
            RecordListings(currentUserId, filteredPaperPage);

            return new ListPage<Paper>(filteredPaperPage, total);
        }
        public ListPage<Paper> ListForNode(string currentUserId, string nodeId, List<CommunityEntityType> types, List<string> communityEntityIds, PaperType? type, List<PaperTag>? tags, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var entities = _communityEntityService.List(entityIds);
            var papers = _paperRepository.SearchListSort(p => p.FeedIds.Any(id => entityIds.Contains(id)) && !p.Deleted, sortKey, ascending, search);

            var filteredPapers = GetFilteredPapers(papers);
            TagPapers(filteredPapers, nodeId, entities.ToDictionary(e => e.Id.ToString(), e => e.Type), currentUserId);
            filteredPapers = GetFilteredPapers(filteredPapers, tags);
            var total = filteredPapers.Count;

            var filteredPaperPage = GetPage(filteredPapers, page, pageSize);
            EnrichPaperData(filteredPaperPage, currentUserId);
            RecordListings(currentUserId, filteredPaperPage);

            return new ListPage<Paper>(filteredPaperPage, total);
        }

        public long CountForNode(string userId, string nodeId, string search)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            var entities = _communityEntityService.List(entityIds);
            var papers = _paperRepository.SearchList(p => p.FeedIds.Any(id => entityIds.Contains(id)) && !p.Deleted, search);

            var filteredPapers = GetFilteredPapers(papers);
            return filteredPapers.Count;
        }

        public List<Paper> ListForExternalIds(IEnumerable<string> externalIds)
        {
            return _paperRepository.ListForExternalIds(externalIds);
        }

        public async Task UpdateAsync(Paper paper)
        {
            var existingPaper = _paperRepository.Get(paper.Id.ToString());

            //once papers are published, they are not to be edited
            if (existingPaper.Status != ContentEntityStatus.Draft)
                throw new Exception($"Can not update active paper");

            //draft papers only belong to one feed
            if (paper?.FeedIds.Count() != 1)
                throw new Exception();

            await _paperRepository.UpdateAsync(paper);
        }

        public async Task DeleteForExternalSystemAndUserAsync(string userId, ExternalSystem externalSystem)
        {
            var user = _userRepository.Get(userId);
            var papers = _paperRepository.List(p => p.FeedIds.Contains(userId) && p.ExternalSystem == externalSystem);
            foreach (var paper in papers)
            {
                paper.FeedIds.Remove(userId);
                await _paperRepository.UpdateAsync(paper);
                await _documentRepository.DeleteAsync(paper.Id.ToString());
            }
        }

        protected void EnrichPaperData(IEnumerable<Paper> papers, string currentUserId)
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

                paper.Users = users.Where(u => paper.UserIds?.Contains(u.Id.ToString()) == true).ToList();
                paper.PostCount = contentEntities.Count(ce => ce.FeedId == paper.Id.ToString());
                paper.NewPostCount = contentEntities.Count(ce => ce.FeedId == paper.Id.ToString() && ce.CreatedUTC > (feedRecord?.LastReadUTC ?? DateTime.MinValue));
                paper.NewMentionCount = mentions.Count(m => m.OriginFeedId == paper.Id.ToString());
                paper.NewThreadActivityCount = contentEntities.Count(ce => ce.FeedId == paper.Id.ToString() && ce.LastActivityUTC > (userContentEntityRecords.SingleOrDefault(ucer => ucer.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue));
                paper.IsNew = feedRecord == null;

                if (!string.IsNullOrEmpty(currentUserId))
                    paper.UserInLibrary = paper.FeedIds.Contains(currentUserId);
            }

            EnrichPapersWithPermissions(papers, currentUserId);
            EnrichEntitiesWithCreatorData(papers, users);
        }

        protected IEnumerable<CommunityEntityType> GetTypesFromTags(IEnumerable<PaperTag> paperTags)
        {
            foreach (var tag in paperTags ?? new List<PaperTag>())
            {
                CommunityEntityType entityType;
                if (Enum.TryParse(tag.ToString(), out entityType))
                    yield return entityType;
            }
        }

        protected void TagPapers(IEnumerable<Paper> papers, string feedId, Dictionary<string, CommunityEntityType> feedTypes, string currentUserId)
        {
            foreach (var paper in papers)
            {
                paper.OriginTags = new List<PaperTag>();

                if (paper.UserIds?.Contains(feedId) == true)
                    paper.OriginTags.Add(PaperTag.AuthorOf);

                if (paper.UserIds?.Contains(currentUserId) == true)
                    paper.OriginTags.Add(PaperTag.AuthoredByMe);

                if (!paper.FeedIds.Contains(feedId))
                    paper.OriginTags.Add(PaperTag.Aggregated);

                //else
                //    switch (type)
                //    {
                //        case FeedType.Project:
                //            paper.OriginTags.Add(PaperTag.Reference);
                //            continue;
                //        case FeedType.Workspace:
                //            paper.OriginTags.Add(PaperTag.Library);
                //            continue;
                //    }

                //var applicableTypes = new[] { CommunityEntityType.Project, CommunityEntityType.Workspace, CommunityEntityType.Node }.Where(type => paper.FeedIds.Any(feedId => feedTypes.ContainsKey(feedId) && feedTypes[feedId] == type)).ToList();
                //var applicableTags = new[] { PaperTag.Project, PaperTag.Community, PaperTag.Node }.Where(paperTag => applicableTypes.Any(type => Enum.Parse<PaperTag>(type.ToString()) == paperTag));
                //paper.OriginTags.AddRange(applicableTags);
            }
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

        public List<CommunityEntity> ListCommunityEntitiesForNodePapers(string currentUserId, string nodeId, List<CommunityEntityType> types, PaperType? type, List<PaperTag> tags, string search, int page, int pageSize)
        {
            var entityIds = GetCommunityEntityIdsForNode(nodeId);
            var entities = _communityEntityService.List(entityIds);
            var papers = _paperRepository.SearchList(p => p.FeedIds.Any(id => entityIds.Contains(id)) && !p.Deleted, search);

            var filteredPapers = GetFilteredPapers(papers);
            TagPapers(filteredPapers, nodeId, entities.ToDictionary(e => e.Id.ToString(), e => e.Type), currentUserId);
            filteredPapers = GetFilteredPapers(filteredPapers, tags);

            return entities.Where(e => papers.Any(p => p.FeedIds.Contains(e.Id.ToString()))).ToList();
        }

        public List<CommunityEntity> ListCommunityEntitiesForCommunityPapers(string currentUserId, string communityId, List<CommunityEntityType> types, PaperType? type, List<PaperTag> tags, string search, int page, int pageSize)
        {
            var entityIds = GetCommunityEntityIdsForCommunity(communityId);
            var entities = _communityEntityService.List(entityIds);
            var papers = _paperRepository.SearchList(p => p.FeedIds.Any(id => entityIds.Contains(id)) && !p.Deleted, search);

            var filteredPapers = GetFilteredPapers(papers);
            TagPapers(filteredPapers, communityId, entities.ToDictionary(e => e.Id.ToString(), e => e.Type), currentUserId);
            filteredPapers = GetFilteredPapers(filteredPapers, tags);

            return entities.Where(e => papers.Any(p => p.FeedIds.Contains(e.Id.ToString()))).ToList();
        }
    }
}