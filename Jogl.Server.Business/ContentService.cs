using Jogl.Server.Arxiv;
using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.GitHub;
using Jogl.Server.HuggingFace;
using Jogl.Server.Notifications;
using Jogl.Server.PubMed;
using Jogl.Server.Storage;
using Jogl.Server.URL;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jogl.Server.Business
{
    public class ContentService : BaseService, IContentService
    {
        private readonly IGitHubFacade _githubFacade;
        private readonly IHuggingFaceFacade _huggingfaceFacade;
        private readonly IArxivFacade _arxivFacade;
        private readonly IPubMedFacade _pubmedFacade;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ICallForProposalRepository _callForProposalRepository;
        private readonly INodeRepository _nodeRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IDraftRepository _draftRepository;
        private readonly IFeedIntegrationRepository _feedIntegrationRepository;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly INotificationService _notificationService;
        private readonly IStorageService _storageService;
        private readonly IEmailService _emailService;
        private readonly IUrlService _urlService;
        private readonly INotificationFacade _notificationFacade;
        private readonly IConfiguration _configuration;

        private const string EVERYONE = "Everyone";
        private const string MENTION_REGEX = @"<span class=""mention"" data-index=""[0-9]"" data-denotation-char=""[\S]"" data-value=""(?:[^""]+)"" data-link=""\/?([^""]+)\/([^""]+)""";
        private const string MENTION_REGEX_2 = @"data-type=\""mention\"" data-id=\""([^""]+)\"" data-label=\""([^""]+)\""";

        public ContentService(IGitHubFacade githubFacade, IHuggingFaceFacade huggingfaceFacade, IArxivFacade arxivFacade, IPubMedFacade pubMedFacade, IOrganizationRepository organizationRepository, ICallForProposalRepository callForProposalRepository, INodeRepository nodeRepository, IWorkspaceRepository workspaceRepository, IDraftRepository draftRepository, IFeedIntegrationRepository feedIntegrationRepository, ICommunityEntityService communityEntityService, INotificationService notificationService, IStorageService storageService, IEmailService emailService, IUrlService urlService, INotificationFacade notificationFacade, IConfiguration configuration, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _githubFacade = githubFacade;
            _huggingfaceFacade = huggingfaceFacade;
            _arxivFacade = arxivFacade;
            _pubmedFacade = pubMedFacade;
            _organizationRepository = organizationRepository;
            _callForProposalRepository = callForProposalRepository;
            _nodeRepository = nodeRepository;
            _workspaceRepository = workspaceRepository;
            _draftRepository = draftRepository;
            _draftRepository = draftRepository;
            _feedIntegrationRepository = feedIntegrationRepository;
            _communityEntityService = communityEntityService;
            _notificationService = notificationService;
            _storageService = storageService;
            _emailService = emailService;
            _urlService = urlService;
            _notificationFacade = notificationFacade;
            _configuration = configuration;
        }

        public async Task<string> CreateAsync(ContentEntity entity)
        {
            //mark feed entity as updated
            await _feedEntityService.UpdateActivityAsync(entity.FeedId, entity.CreatedUTC, entity.CreatedByUserId);

            //create content entity
            var id = await _contentEntityRepository.CreateAsync(entity);

            //mark feed write
            await _userFeedRecordRepository.SetFeedWrittenAsync(entity.CreatedByUserId, entity.FeedId, DateTime.UtcNow);

            //mark content entity write
            await _userContentEntityRecordRepository.SetContentEntityWrittenAsync(entity.CreatedByUserId, entity.FeedId, id, DateTime.UtcNow);

            //process mentions
            entity.Mentions = GetMentions(entity.CreatedByUserId, entity.FeedId, entity.Text);
            foreach (var mention in entity.Mentions)
            {
                mention.CreatedByUserId = entity.CreatedByUserId;
                mention.CreatedUTC = entity.CreatedUTC;
                mention.OriginId = id;
                mention.OriginFeedId = entity.FeedId;
                mention.OriginType = MentionOrigin.ContentEntity;

                if (mention.EntityType == FeedType.User)
                {
                    mention.Unread = true;
                    await _userFeedRecordRepository.SetFeedMentionAsync(mention.EntityId, entity.FeedId, DateTime.UtcNow);
                    await _userContentEntityRecordRepository.SetContentEntityMentionAsync(mention.EntityId, entity.FeedId, id, DateTime.UtcNow);
                }

                await _mentionRepository.CreateAsync(mention);
            }

            //process attachments
            if (entity.DocumentsToAdd != null)
            {
                foreach (var document in entity.DocumentsToAdd)
                {
                    document.FeedId = entity.FeedId;
                    document.ContentEntityId = id;
                    document.CreatedByUserId = entity.CreatedByUserId;
                    document.CreatedUTC = entity.CreatedUTC;
                    switch (document.Type)
                    {
                        case DocumentType.Document:
                            document.FileSize = document.Data.Length;
                            var documentId = await _documentRepository.CreateAsync(document);
                            await _storageService.CreateOrReplaceAsync(IStorageService.DOCUMENT_CONTAINER, documentId, document.Data);
                            break;
                        default:
                            throw new Exception($"Cannot create document of type {document.Type}");

                    }
                }
            }

            //process notifications
            await _notificationFacade.NotifyCreatedAsync(entity);

            //return
            return id;
        }

        public ContentEntity Get(string entityId)
        {
            return _contentEntityRepository.Get(entityId);
        }

        public ContentEntity GetDetail(string entityId, string currentUserId)
        {
            var contentEntity = _contentEntityRepository.Get(entityId);
            if (contentEntity == null)
                return null;

            var currentUserFeedRecord = _userFeedRecordRepository.Get(ufr => ufr.UserId == currentUserId && ufr.FeedId == contentEntity.FeedId && !ufr.Deleted);

            var currentUserMemberships = _membershipRepository.List(p => p.UserId == currentUserId && !p.Deleted);
            var sourceEntityRelations = _relationRepository.ListForSourceIds(new List<string> { contentEntity.FeedId });
            var targetEntityRelations = _relationRepository.ListForTargetIds(new List<string> { contentEntity.FeedId });

            var currentUserMentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.OriginFeedId == contentEntity.FeedId && !m.Deleted);
            EnrichContentEntityData(new[] { contentEntity }, currentUserMentions, currentUserFeedRecord != null ? new List<UserFeedRecord> { currentUserFeedRecord } : new List<UserFeedRecord>(), currentUserId);

            return contentEntity;
        }

        public Discussion GetDiscussion(string currentUserId, string feedId, ContentEntityType? type, ContentEntityFilter filter, string search, int page, int pageSize)
        {
            var contentEntities = _contentEntityRepository.List(p => !p.Deleted
            && (type == null || p.Type == type)
            && p.FeedId == feedId)
                .OrderByDescending(c => c.CreatedUTC)
                .ToList();

            var currentUserFeedRecord = _userFeedRecordRepository.Get(ufr => ufr.UserId == currentUserId && ufr.FeedId == feedId && !ufr.Deleted);
            var currentUserMentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.OriginFeedId == feedId && !m.Deleted);
            var currentUserContentEntityRecords = _userContentEntityRecordRepository.List(r => r.UserId == currentUserId && r.FeedId == feedId && !r.Deleted);
            var currentUserContentEntityRecordsEntityIds = currentUserContentEntityRecords.Select(ucer => ucer.ContentEntityId);
            var currentUserContentEntityRecordsComments = _commentRepository.List(c => currentUserContentEntityRecordsEntityIds.Contains(c.ContentEntityId) && !c.Deleted);

            var unreadPosts = contentEntities.Count(ce => ce.CreatedUTC > (currentUserFeedRecord?.LastReadUTC ?? DateTime.MaxValue));
            var unreadMentions = currentUserMentions.Count(m => m.Unread);
            var unreadThreads = contentEntities.Count(ce => currentUserContentEntityRecordsComments.Any(c => c.ContentEntityId == ce.Id.ToString() && c.CreatedUTC > (currentUserContentEntityRecords.SingleOrDefault(r => r.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue)));

            switch (filter)
            {
                case ContentEntityFilter.Mentions:
                    var currentUserMentionContentEntityIds = currentUserMentions.Where(m => m.OriginType == MentionOrigin.ContentEntity).Select(m => m.OriginId);
                    var currentUserMentionCommentIds = currentUserMentions.Where(m => m.OriginType == MentionOrigin.Comment).Select(m => m.OriginId).Distinct();

                    var currentUserMentionComments = _commentRepository.List(c => currentUserMentionCommentIds.Contains(c.Id.ToString()) && !c.Deleted);
                    var currentUserMentionCommentContentEntityIds = currentUserMentionComments.Select(c => c.ContentEntityId).Distinct();

                    var mentionContentEntitiesFromContentEntities = contentEntities.Where(c => currentUserMentionContentEntityIds.Contains(c.Id.ToString())).ToList();
                    foreach (var contentEntity in mentionContentEntitiesFromContentEntities)
                    {
                        contentEntity.MentionDate = contentEntity.CreatedUTC;
                    }

                    var mentionContentEntitiesFromComments = contentEntities.Where(c => currentUserMentionCommentContentEntityIds.Contains(c.Id.ToString())).ToList();
                    foreach (var contentEntity in mentionContentEntitiesFromComments)
                    {
                        var mentionComment = currentUserMentionComments.Where(c => c.ContentEntityId == contentEntity.Id.ToString()).OrderByDescending(c => c.CreatedUTC).FirstOrDefault();
                        if (mentionComment != null)
                            contentEntity.MentionDate = mentionComment.CreatedUTC;
                    }

                    contentEntities = new List<ContentEntity>();
                    contentEntities.AddRange(mentionContentEntitiesFromContentEntities);
                    contentEntities.AddRange(mentionContentEntitiesFromComments);
                    contentEntities = contentEntities.DistinctBy(ce => ce.Id).OrderByDescending(ce => ce.MentionDate).ToList();

                    break;

                case ContentEntityFilter.Threads:
                    //contentEntities = contentEntities.Where(ce => currentUserContentEntityRecords.Any(r => r.ContentEntityId == ce.Id.ToString())).ToList();
                    var commentCounts = _commentRepository.Counts(c => currentUserContentEntityRecordsEntityIds.Contains(c.ContentEntityId), c => c.ContentEntityId);
                    contentEntities = contentEntities.Where(ce => commentCounts.ContainsKey(ce.Id.ToString())).OrderByDescending(ce => ce.LastActivityUTC).ToList();

                    break;
            }

            var filteredContentEntities = GetFilteredContentEntities(contentEntities, currentUserId, search, type);
            var total = filteredContentEntities.Count;

            var filteredContentEntityPage = GetPage(filteredContentEntities, page, pageSize);
            EnrichContentEntityData(filteredContentEntityPage, currentUserMentions, currentUserFeedRecord != null ? new List<UserFeedRecord> { currentUserFeedRecord } : new List<UserFeedRecord>(), currentUserId);

            return new Discussion(filteredContentEntityPage, total)
            {
                DiscussionStats = new DiscussionStats
                {
                    UnreadPosts = unreadPosts,
                    UnreadMentions = unreadMentions,
                    UnreadThreads = unreadThreads
                }
            };
        }

        public ListPage<ContentEntity> ListPostContentEntities(string currentUserId, string feedId, ContentEntityType? type, string search, int page, int pageSize)
        {
            var contentEntities = _contentEntityRepository.List(p => !p.Deleted
                && (type == null || p.Type == type)
                && p.FeedId == feedId)
               .OrderByDescending(c => c.CreatedUTC)
               .ToList();

            var currentUserFeedRecord = _userFeedRecordRepository.Get(ufr => ufr.UserId == currentUserId && ufr.FeedId == feedId && !ufr.Deleted);
            var currentUserMentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.OriginFeedId == feedId && !m.Deleted);

            var filteredContentEntities = GetFilteredContentEntities(contentEntities, currentUserId, search, type);
            var total = filteredContentEntities.Count;

            var filteredContentEntityPage = GetPage(filteredContentEntities, page, pageSize);
            EnrichContentEntityData(filteredContentEntityPage, currentUserMentions, currentUserFeedRecord != null ? new List<UserFeedRecord> { currentUserFeedRecord } : new List<UserFeedRecord>(), currentUserId);

            return new ListPage<ContentEntity>(filteredContentEntityPage, total);
        }

        public ListPage<ContentEntity> ListMentionContentEntities(string currentUserId, string feedId, ContentEntityType? type, string search, int page, int pageSize)
        {
            var mentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.OriginFeedId == feedId && !m.Deleted);
            var mentionOriginIds = mentions.Select(m => m.OriginId).ToList();
            var mentionComments = _commentRepository.List(c => mentionOriginIds.Contains(c.Id.ToString()) && !c.Deleted);
            var mentionContentEntityIds = mentions.Where(m => m.OriginType == MentionOrigin.ContentEntity).Select(m => m.OriginId).Concat(mentionComments.Select(c => c.ContentEntityId)).Distinct().ToList();
            //var contentEntities =_contentEntityRepository.SearchGet(mentionContentEntityIds,search,)
            var contentEntities = _contentEntityRepository.Get(mentionContentEntityIds);

            var currentUserFeedRecord = _userFeedRecordRepository.Get(ufr => ufr.UserId == currentUserId && ufr.FeedId == feedId && !ufr.Deleted);
            var currentUserContentEntityRecords = _userContentEntityRecordRepository.List(r => r.UserId == currentUserId && r.FeedId == feedId && !r.Deleted);

            foreach (var contentEntity in contentEntities)
            {
                var contentEntityMentionComments = mentionComments.Where(c => c.ContentEntityId == contentEntity.Id.ToString());
                var contentEntityMentions = mentions.Where(m => m.OriginId == contentEntity.Id.ToString() || contentEntityMentionComments.Any(cemc => cemc.Id.ToString() == m.OriginId));

                if (!contentEntityMentions.Any())
                    continue;

                contentEntity.MentionUnread = contentEntityMentions.Any(m => m.Unread);
                contentEntity.MentionDate = contentEntityMentions.Max(m => m.CreatedUTC);
            }

            var filteredContentEntities = GetFilteredContentEntities(contentEntities, currentUserId, search, type).OrderByDescending(ce => ce.MentionUnread).ThenByDescending(ce => ce.MentionDate).ToList();
            var total = filteredContentEntities.Count;

            var filteredContentEntityPage = GetPage(filteredContentEntities, page, pageSize);
            EnrichContentEntityData(filteredContentEntityPage, mentions, currentUserFeedRecord != null ? new List<UserFeedRecord> { currentUserFeedRecord } : new List<UserFeedRecord>(), currentUserId);

            return new ListPage<ContentEntity>(filteredContentEntityPage, total);
        }

        public ListPage<ContentEntity> ListThreadContentEntities(string currentUserId, string feedId, ContentEntityType? type, string search, int page, int pageSize)
        {
            var mentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.OriginFeedId == feedId && !m.Deleted);
            var comments = _commentRepository.List(c => c.FeedId == feedId && !c.Deleted);
            var contentEntityIds = comments.Select(c => c.ContentEntityId).Distinct().ToList();
            //var contentEntities =_contentEntityRepository.SearchGet(mentionContentEntityIds,search,)
            var contentEntities = _contentEntityRepository.Get(contentEntityIds);

            var currentUserFeedRecord = _userFeedRecordRepository.Get(ufr => ufr.UserId == currentUserId && ufr.FeedId == feedId && !ufr.Deleted);
            var currentUserContentEntityRecords = _userContentEntityRecordRepository.List(r => r.UserId == currentUserId && r.FeedId == feedId && !r.Deleted);

            foreach (var contentEntity in contentEntities)
            {
                var userContentEntityRecord = currentUserContentEntityRecords.SingleOrDefault(ucer => ucer.ContentEntityId == contentEntity.Id.ToString());
                if (userContentEntityRecord == null)
                    continue;

                contentEntity.LastReplyDate = comments.Where(c => c.ContentEntityId == contentEntity.Id.ToString()).OrderByDescending(c => c.CreatedUTC).FirstOrDefault()?.CreatedUTC;
                contentEntity.LastReplyUnread = contentEntity.LastReplyDate > (userContentEntityRecord.LastReadUTC ?? DateTime.MinValue);
            }

            var filteredContentEntities = GetFilteredContentEntities(contentEntities, currentUserId, search, type).OrderByDescending(ce => ce.LastReplyUnread).ThenByDescending(ce => ce.LastReplyDate).ToList();
            var total = filteredContentEntities.Count;

            var filteredContentEntityPage = GetPage(filteredContentEntities, page, pageSize);
            EnrichContentEntityData(filteredContentEntityPage, mentions, currentUserFeedRecord != null ? new List<UserFeedRecord> { currentUserFeedRecord } : new List<UserFeedRecord>(), comments, currentUserId);

            return new ListPage<ContentEntity>(filteredContentEntityPage, total);
        }

        public List<ContentEntity> ListContentEntitiesForNode(string currentUserId, string nodeId, int page, int pageSize)
        {
            var feedEntityIds = GetFeedEntityIdsForNode(nodeId);

            var userFeedIds = _userFeedRecordRepository
                .Query(ufr => ufr.UserId == currentUserId && ufr.FollowedUTC.HasValue && feedEntityIds.Contains(ufr.FeedId))
                .ToList() //TODO only select FeedId
                .Select(ucer => ucer.FeedId)
                .ToList();

            var ucerContentEntityIds = _userContentEntityRecordRepository
                .Query(ucer => ucer.UserId == currentUserId && ucer.FollowedUTC.HasValue && feedEntityIds.Contains(ucer.FeedId))
                .ToList() //TODO only select ContentEntityId
                .Select(ucer => ucer.ContentEntityId)
                .ToList();

            //TODO filter for visibility
            var contentEntities = _contentEntityRepository
                .QueryForActivity(currentUserId, ce => userFeedIds.Contains(ce.FeedId) || ucerContentEntityIds.Contains(ce.Id.ToString()))
                .Sort(SortKey.LastActivity, false)
                .Page(page, pageSize)
                .ToList();

            var contentEntityIds = contentEntities
                .Select(ce => ce.Id.ToString())
                .ToList();

            var comments = _commentRepository.Query(c => contentEntityIds.Contains(c.ContentEntityId) && c.CreatedByUserId != currentUserId).ToList(); //TODO only select one newest comment for every content entity
            EnrichContentEntityDataForInbox(contentEntities, comments);

            return contentEntities;
        }

        public List<ContentEntity> ListThreadsForNode(string currentUserId, string nodeId, int page, int pageSize)
        {
            var feedEntityIds = GetFeedEntityIdsForNode(nodeId);
            var ucerContentEntityIds = _userContentEntityRecordRepository
                .Query(ucer => ucer.UserId == currentUserId && ucer.FollowedUTC.HasValue && feedEntityIds.Contains(ucer.FeedId))
                .ToList() //TODO only select FeedId
                .Select(ucer => ucer.ContentEntityId)
                .ToList();

            //TODO filter for visibility
            var contentEntities = _contentEntityRepository
                .QueryForActivity(currentUserId, ce => ucerContentEntityIds.Contains(ce.Id.ToString()))
                .Sort(SortKey.LastActivity, false)
                .Page(page, pageSize)
                .ToList();

            var contentEntityIds = contentEntities
                .Select(ce => ce.Id.ToString())
                .ToList();

            var comments = _commentRepository.Query(c => contentEntityIds.Contains(c.ContentEntityId) && c.CreatedByUserId != currentUserId).ToList(); //TODO only select one newest comment for every content entity
            EnrichContentEntityDataForInbox(contentEntities, comments);

            return contentEntities;
        }

        public List<ContentEntity> ListMentionsForNode(string currentUserId, string nodeId, int page, int pageSize)
        {
            var feedEntityIds = GetFeedEntityIdsForNode(nodeId);
            var mentionOriginIds = _mentionRepository
                .Query(m => m.EntityId == currentUserId)
                .Sort(SortKey.CreatedDate)
                .ToList()
                .Select(m => m.OriginId)
                .ToList();

            var mentionComments = _commentRepository.Query(c => mentionOriginIds.Contains(c.Id.ToString())).ToList();
            var mentionCommentContentEntityIds = mentionComments.Select(c => c.ContentEntityId).ToList();

            //TODO filter for visibility
            var contentEntities = _contentEntityRepository
                .Query(ce => mentionOriginIds.Contains(ce.Id.ToString()) || mentionCommentContentEntityIds.Contains(ce.Id.ToString()))
                .Sort(SortKey.LastActivity, false)
                .Page(page, pageSize)
                .ToList();

            EnrichContentEntityDataForInbox(contentEntities, mentionComments);

            return contentEntities;
        }

        //private List<string> GetFeedIds(string currentUserId, FeedType type, string nodeId)
        //{
        //    var entities = new List<CommunityEntity>();
        //    if (string.IsNullOrEmpty(nodeId))
        //        entities = GetGlobalEcosystem(_projectRepository, _workspaceRepository, _nodeRepository, currentUserId, new CommunityEntityType[] { });
        //    else
        //        entities = GetEcosystemForNodes(_projectRepository, _workspaceRepository, _nodeRepository, currentUserId, new CommunityEntityType[] { }, nodeId);

        //    var entityIds = entities.Select(e => e.Id.ToString()).ToList();

        //    switch (type)
        //    {
        //        case FeedType.Event:
        //            var eventAttendances = _eventAttendanceRepository.List(ea => ea.UserId == currentUserId && !ea.Deleted);
        //            var eventIds = eventAttendances.Select(ea => ea.EventId).Distinct().ToList();
        //            var eventsForNode = _eventRepository.List(e => eventIds.Contains(e.Id.ToString()) && entityIds.Contains(e.CommunityEntityId) && !e.Deleted).ToList();
        //            var eventsForNodeIds = eventsForNode.Select(e => e.Id.ToString()).Distinct().ToList();
        //            return eventsForNodeIds;
        //        case FeedType.Need:
        //            var needs = _needRepository.ListForEntityIds(entityIds);

        //            return needs.Select(n => n.Id.ToString()).ToList();
        //        case FeedType.Paper:
        //            var papers = _paperRepository.ListForFeeds(entityIds);
        //            var filteredPapers = GetFilteredPapers(papers, currentUserId);

        //            return filteredPapers.Select(p => p.Id.ToString()).ToList();
        //        case FeedType.Document:
        //            var documents = _documentRepository.ListForFeeds(entityIds);
        //            var filteredDocuments = GetFilteredDocuments(documents, currentUserId);

        //            return filteredDocuments.Select(p => p.Id.ToString()).ToList();
        //        default:
        //            return new List<string>();
        //    }
        //}

        //public DiscussionStats GetAggregateData(string currentUserId, FeedType type, string nodeId, bool mention, string search, int page, int pageSize)
        //{
        //    var feedIds = GetFeedIds(currentUserId, type, nodeId);
        //    var contentEntities = _contentEntityRepository.List(p => (p.Type != ContentEntityType.Article && p.Type != ContentEntityType.Preprint)
        //    && feedIds.Contains(p.FeedId))
        //        .OrderByDescending(c => c.CreatedUTC)
        //        .ToList();

        //    var currentUserMentions = _mentionRepository.List(m => m.EntityId == currentUserId && feedIds.Contains(m.OriginFeedId) && !m.Deleted);
        //    if (mention)
        //    {
        //        var currentUserMentionContentEntityIds = currentUserMentions.Where(m => m.OriginType == MentionOrigin.ContentEntity).Select(m => m.OriginId);
        //        var currentUserMentionCommentIds = currentUserMentions.Where(m => m.OriginType == MentionOrigin.Comment).Select(m => m.OriginId);
        //        var currentUserMentionCommentContentEntityIds = _commentRepository.List(c => currentUserMentionCommentIds.Contains(c.Id.ToString()) && !c.Deleted).Select(c => c.ContentEntityId);

        //        var contentEntityIds = currentUserMentionContentEntityIds.Union(currentUserMentionCommentContentEntityIds);
        //        contentEntities = contentEntities.Where(c => contentEntityIds.Contains(c.Id.ToString())).ToList();
        //    }

        //    var filteredContentEntities = GetFilteredContentEntities(contentEntities, currentUserId, search, null);
        //    var total = filteredContentEntities.Count;

        //    var filteredContentEntityPage = GetPage(filteredContentEntities, page, pageSize);
        //    EnrichContentEntityData(filteredContentEntityPage, currentUserMentions, new List<UserFeedRecord> { }, currentUserId);

        //    return new DiscussionData(filteredContentEntityPage, total)
        //    {
        //        UnreadMentions = currentUserMentions.Count(m => m.Unread)
        //    };
        //}

        private void EnrichContentEntityData(IEnumerable<ContentEntity> contentEntities, IEnumerable<Mention> currentUserMentions, IEnumerable<UserFeedRecord> currentUserFeedRecords, string currentUserId)
        {
            var comments = _commentRepository.ListForContentEntities(contentEntities.Select(e => e.Id.ToString()));
            EnrichContentEntityData(contentEntities, currentUserMentions, currentUserFeedRecords, comments, currentUserId);
        }

        private void EnrichContentEntityData(IEnumerable<ContentEntity> contentEntities, IEnumerable<Mention> currentUserMentions, IEnumerable<UserFeedRecord> currentUserFeedRecords, IEnumerable<Comment> comments, string currentUserId)
        {
            var feedIds = contentEntities.Select(ce => ce.FeedId).Distinct().ToList();
            var feedEntities = _feedEntityService.GetFeedEntitySet(feedIds);

            var contentEntityIds = contentEntities.Select(e => e.Id.ToString());
            var contentEntityUserIds = contentEntities.Select(e => e.CreatedByUserId);
            var commentUserIds = comments.Select(c => c.CreatedByUserId);
            var userIds = contentEntityUserIds.Concat(commentUserIds);

            var users = _userRepository.Get(userIds.ToList());
            var documents = _documentRepository.List(d => contentEntityIds.Contains(d.ContentEntityId) && d.ContentEntityId != null & d.CommentId == null && !d.Deleted);
            var reactions = _reactionRepository.List(r => contentEntityIds.Contains(r.OriginId) && !r.Deleted);

            var currentUserContentEntityRecords = _userContentEntityRecordRepository.List(r => r.UserId == currentUserId && feedIds.Contains(r.FeedId) && !r.Deleted);

            foreach (var document in documents)
            {
                document.IsMedia = IsMedia(document);
            }

            foreach (var contentEntity in contentEntities)
            {
                contentEntity.Documents = documents.Where(d => d.ContentEntityId == contentEntity.Id.ToString()).ToList();
                contentEntity.Reactions = reactions.Where(r => r.OriginId == contentEntity.Id.ToString()).OrderByDescending(r => r.CreatedByUserId == currentUserId).ToList();
                contentEntity.UserMentions = currentUserMentions.Count(m => m.OriginId == contentEntity.Id.ToString() && m.Unread);
                contentEntity.UserMentionsInComments = currentUserMentions.Count(m => comments.Where(c => c.ContentEntityId == contentEntity.Id.ToString()).Select(c => c.Id.ToString()).Contains(m.OriginId) && m.Unread);

                var contentEntityComments = comments.Where(c => c.ContentEntityId == contentEntity.Id.ToString());
                var contentEntityCommentUsers = users.Where(u => contentEntityComments.Any(c => c.CreatedByUserId == u.Id.ToString()));

                contentEntity.CommentCount = contentEntityComments.Count();
                contentEntity.Users = contentEntityCommentUsers.ToList();

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var lastReadPost = currentUserContentEntityRecords.SingleOrDefault(r => r.ContentEntityId == contentEntity.Id.ToString())?.LastReadUTC ?? DateTime.MinValue;
                    contentEntity.NewCommentCount = comments.Count(c => c.ContentEntityId == contentEntity.Id.ToString() && c.CreatedUTC > lastReadPost);

                    var lastReadFeed = currentUserFeedRecords.SingleOrDefault(r => r.FeedId == contentEntity.FeedId)?.LastReadUTC ?? DateTime.MinValue;
                    contentEntity.IsNew = contentEntity.CreatedUTC > lastReadFeed;

                    contentEntity.UserReaction = contentEntity.Reactions.SingleOrDefault(r => r.UserId == currentUserId);
                }

                contentEntity.FeedEntity = _feedEntityService.GetEntityFromLists(contentEntity.FeedId, feedEntities);
            }

            EnrichEntitiesWithCreatorData(contentEntities, users);
        }

        private void EnrichContentEntityDataForInbox(IEnumerable<ContentEntity> contentEntities, IEnumerable<Comment> comments)
        {
            var feedIds = contentEntities.Select(ce => ce.FeedId).Distinct().ToList();
            var feedEntities = _feedEntityService.GetFeedEntitySetExtended(feedIds);

            foreach (var contentEntity in contentEntities)
            {
                contentEntity.FeedEntity = _feedEntityService.GetEntityFromLists(contentEntity.FeedId, feedEntities);
                contentEntity.LastComment = comments.Where(c => c.ContentEntityId == contentEntity.Id.ToString())
                    .OrderByDescending(c => c.CreatedUTC)
                    .FirstOrDefault();
            }

            EnrichEntitiesWithCreatorData(contentEntities);
            EnrichEntitiesWithCreatorData(comments);
        }

        //public List<ContentEntity> ListNotesForUser(string currentUserId, string targetUserId, string search, int page, int pageSize)
        //{
        //    var user = _userRepository.Get(targetUserId);
        //    var contentEntities = _contentEntityRepository.List((p) =>
        //        (string.IsNullOrEmpty(search) || p.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) || p.Summary.Contains(search, StringComparison.CurrentCultureIgnoreCase) || p.Text.Contains(search, StringComparison.CurrentCultureIgnoreCase))
        //        && (p.FeedId == user.FeedId)
        //        && (p.Status == ContentEntityStatus.Active || currentUserId == targetUserId)
        //        && (p.Type == ContentEntityType.JoglDoc)
        //        && (!p.Deleted),
        //        page, pageSize)
        //        .OrderByDescending(c => c.CreatedUTC)
        //        .ToList();

        //    var documents = _documentRepository.ListForContentEntities(contentEntities.Select(e => e.Id.ToString()));
        //    var reactions = _reactionRepository.ListForContentEntities(contentEntities.Select(e => e.Id.ToString()));
        //    var comments = _commentRepository.ListForContentEntities(contentEntities.Select(e => e.Id.ToString()));
        //    var users = _userRepository.Get(contentEntities.Select(e => e.CreatedByUserId).Union(comments.Select(c => c.CreatedByUserId)).ToList());

        //    foreach (var contentEntity in contentEntities)
        //    {
        //        contentEntity.Documents = documents.Where(d => d.ContentEntityId == contentEntity.Id.ToString()).ToList();
        //        contentEntity.Reactions = reactions.Where(r => r.ContentEntityId == contentEntity.Id.ToString()).ToList();
        //        if (!string.IsNullOrEmpty(currentUserId))
        //            contentEntity.UserReactions = contentEntity.Reactions.Where(r => r.UserId == currentUserId).ToList();

        //        contentEntity.CreatedBy = user;
        //    }

        //    return contentEntities;
        //}

        public List<ContentEntity> ListAggregate(string search, int page, int pageSize, string userId, bool loadDetails)
        {
            var feedIds = new List<string>();

            //aggregate feed ids from followed objects
            var memberships = _membershipRepository.List(m => !m.Deleted && m.UserId == userId);
            var communities = _workspaceRepository.Get(memberships.Where(m => m.CommunityEntityType == CommunityEntityType.Workspace).Select(m => m.CommunityEntityId).ToList());
            var nodes = _nodeRepository.Get(memberships.Where(m => m.CommunityEntityType == CommunityEntityType.Node).Select(m => m.CommunityEntityId).ToList());

            feedIds.AddRange(communities.Select(c => c.FeedId));
            feedIds.AddRange(nodes.Select(n => n.FeedId));

            //aggregate feed ids from followed users
            var followings = _followingRepository.List(f => f.UserIdFrom == userId);
            var followedUsers = _userRepository.Get(followings.Select(f => f.UserIdTo).ToList());

            feedIds.AddRange(followedUsers.Select(u => u.FeedId));

            //list content entities for those feeds
            var contentEntities = _contentEntityRepository.List(feedIds, (p) =>
                (string.IsNullOrEmpty(search) || p.Text.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                && (p.Status == ContentEntityStatus.Active)
                && (!p.Deleted),
                page, pageSize)
                .OrderByDescending(c => c.CreatedUTC)
                .ToList();

            if (!loadDetails)
                return contentEntities;

            //var reactions = _reactionRepository.ListForContentEntities(contentEntities.Select(e => e.Id.ToString()));
            var comments = _commentRepository.ListForContentEntities(contentEntities.Select(e => e.Id.ToString()));

            //foreach (var contentEntity in contentEntities)
            //{
            //    contentEntity.Reactions = reactions.Where(r => r.OriginId == contentEntity.Id.ToString()).ToList();
            //    if (!string.IsNullOrEmpty(userId))
            //        contentEntity.UserReaction = contentEntity.Reactions.SingleOrDefault(r => r.UserId == userId);

            //}

            EnrichEntitiesWithCreatorData(contentEntities);
            return contentEntities;
        }

        protected void EnrichCommentData(IEnumerable<Comment> comments, IEnumerable<User> users, IEnumerable<Mention> mentions, UserContentEntityRecord userContentEntityRecord, string currentUserId)
        {
            var commentIds = comments.Select(c => c.Id.ToString());
            var documents = _documentRepository.List(d => commentIds.Contains(d.CommentId) && !d.Deleted);
            var lastReadFeed = userContentEntityRecord?.LastReadUTC ?? DateTime.MinValue;
            var reactions = _reactionRepository.List(r => commentIds.Contains(r.OriginId) && !r.Deleted);

            foreach (var document in documents)
            {
                document.IsMedia = IsMedia(document);
            }

            foreach (var comment in comments)
            {
                comment.Reactions = reactions.Where(r => r.OriginId == comment.Id.ToString()).OrderByDescending(r => r.CreatedByUserId == currentUserId).ToList();
                comment.Documents = documents.Where(d => d.CommentId == comment.Id.ToString()).ToList();
                comment.UserMentions = mentions.Count(m => m.OriginId == comment.Id.ToString());
                comment.IsNew = comment.CreatedUTC > lastReadFeed;

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    comment.UserReaction = comment.Reactions.SingleOrDefault(r => r.UserId == currentUserId);
                }
            }

            EnrichEntitiesWithCreatorData(comments, users);
        }

        public async Task UpdateAsync(ContentEntity contentEntity)
        {
            await _contentEntityRepository.UpdateAsync(contentEntity);

            //process attachments
            if (contentEntity.DocumentsToAdd != null)
            {
                foreach (var document in contentEntity.DocumentsToAdd)
                {
                    document.FeedId = contentEntity.FeedId;
                    document.ContentEntityId = contentEntity.Id.ToString();
                    document.CreatedByUserId = contentEntity.UpdatedByUserId ?? contentEntity.CreatedByUserId;
                    document.CreatedUTC = contentEntity.UpdatedUTC ?? DateTime.UtcNow;
                    switch (document.Type)
                    {
                        case DocumentType.Document:
                            document.FileSize = document.Data.Length;
                            var documentId = await _documentRepository.CreateAsync(document);
                            await _storageService.CreateOrReplaceAsync(IStorageService.DOCUMENT_CONTAINER, documentId, document.Data);
                            break;
                        default:
                            throw new Exception($"Cannot create document of type {document.Type}");

                    }
                }
            }

            //process attachments
            if (contentEntity.DocumentsToDelete != null)
            {
                var existingAttachments = _documentRepository.List(d => d.ContentEntityId == contentEntity.Id.ToString() && d.CommentId == null && !d.Deleted);
                foreach (var documentId in contentEntity.DocumentsToDelete)
                {
                    //make sure attachment belongs to current comment
                    if (!existingAttachments.Any(d => d.Id.ToString() == documentId))
                        continue;

                    await _documentRepository.DeleteAsync(documentId);
                    await _storageService.DeleteAsync(IStorageService.DOCUMENT_CONTAINER, documentId);
                }
            }
        }

        public async Task DeleteAsync(string id)
        {
            var commentIds = _commentRepository.List(c => c.ContentEntityId == id && !c.Deleted).Select(c => c.Id.ToString()).ToList();

            await _contentEntityRepository.DeleteAsync(id);
            await _commentRepository.DeleteAsync(commentIds);
            await _mentionRepository.DeleteAsync(m => m.OriginId == id && !m.Deleted);
            await _mentionRepository.DeleteAsync(m => commentIds.Contains(m.OriginId) && !m.Deleted);
            await _userContentEntityRecordRepository.DeleteAsync(r => r.ContentEntityId == id && !r.Deleted);
            await _reactionRepository.DeleteAsync(r => r.OriginId == id && !r.Deleted);
        }
        public Feed GetFeed(string feedId)
        {
            return _feedRepository.Get(feedId);
        }

        public async Task<string> CreateReactionAsync(Reaction reaction)
        {
            return await _reactionRepository.CreateAsync(reaction);
        }

        public Reaction GetReaction(string id)
        {
            return _reactionRepository.Get(id);
        }

        public Reaction GetReaction(string originId, string userId)
        {
            return _reactionRepository.Get(r => r.OriginId == originId && r.UserId == userId && !r.Deleted);
        }

        public List<Reaction> ListReactions(string originId, string currentUserId)
        {
            var reactions = _reactionRepository.List(r => r.OriginId == originId && !r.Deleted);
            EnrichEntitiesWithCreatorData(reactions);

            return reactions.OrderByDescending(r => r.CreatedByUserId == currentUserId).ToList();
        }

        public async Task UpdateReactionAsync(Reaction reaction)
        {
            await _reactionRepository.UpdateAsync(reaction);
        }

        public async Task DeleteReactionAsync(string reactionId)
        {
            await _reactionRepository.DeleteAsync(reactionId);
        }

        public async Task<string> CreateCommentAsync(Comment comment)
        {
            //mark feed entity as updated
            await _feedEntityService.UpdateActivityAsync(comment.FeedId, comment.CreatedUTC, comment.CreatedByUserId);

            //mark content entity as updated
            await _contentEntityRepository.UpdateLastActivityAsync(comment.ContentEntityId, comment.CreatedUTC, comment.CreatedByUserId);

            //mark feed write
            await _userFeedRecordRepository.SetFeedWrittenAsync(comment.CreatedByUserId, comment.FeedId, DateTime.UtcNow);

            //mark content entity write
            await _userContentEntityRecordRepository.SetContentEntityWrittenAsync(comment.CreatedByUserId, comment.FeedId, comment.ContentEntityId, DateTime.UtcNow);

            //process notifications
            //await _notificationService.NotifyCommentPostedAsync(comment);

            //create comment
            var id = await _commentRepository.CreateAsync(comment);

            //process mentions
            comment.Mentions = GetMentions(comment.CreatedByUserId, comment.FeedId, comment.Text);
            foreach (var mention in comment.Mentions)
            {
                mention.CreatedByUserId = comment.CreatedByUserId;
                mention.CreatedUTC = comment.CreatedUTC;
                mention.OriginId = id;
                mention.OriginFeedId = comment.FeedId;
                mention.OriginType = MentionOrigin.Comment;

                if (mention.EntityType == FeedType.User)
                {
                    mention.Unread = true;
                    await _userFeedRecordRepository.SetFeedMentionAsync(mention.EntityId, comment.FeedId, DateTime.UtcNow);
                    await _userContentEntityRecordRepository.SetContentEntityMentionAsync(mention.EntityId, comment.FeedId, comment.ContentEntityId, DateTime.UtcNow);
                }

                await _mentionRepository.CreateAsync(mention);
            }

            //process attachments
            if (comment.DocumentsToAdd != null)
            {
                foreach (var document in comment.DocumentsToAdd)
                {
                    document.FeedId = comment.FeedId;
                    document.ContentEntityId = comment.ContentEntityId;
                    document.CommentId = id;
                    document.CreatedByUserId = comment.CreatedByUserId;
                    document.CreatedUTC = comment.CreatedUTC;
                    switch (document.Type)
                    {
                        case DocumentType.Document:
                            document.FileSize = document.Data.Length;
                            var documentId = await _documentRepository.CreateAsync(document);
                            await _storageService.CreateOrReplaceAsync(IStorageService.DOCUMENT_CONTAINER, documentId, document.Data);
                            break;
                        default:
                            throw new Exception($"Cannot create document of type {document.Type}");

                    }
                }
            }

            //process notifications
            await _notificationFacade.NotifyCreatedAsync(comment);

            //return
            return id;
        }

        private List<Mention> GetMentions(string currentUserId, string feedId, string text)
        {
            var res = new List<Mention>();
            foreach (Match match in Regex.Matches(text, MENTION_REGEX))
            {
                if (match.Groups[2].Value == EVERYONE)
                {
                    var members = _membershipRepository.List(m => m.CommunityEntityId == feedId && m.UserId != currentUserId && !m.Deleted);
                    foreach (var member in members)
                    {
                        res.Add(new Mention
                        {
                            EntityType = FeedType.User,
                            EntityId = member.UserId,
                        });
                    }
                }
                else
                {
                    res.Add(new Mention
                    {
                        EntityType = _urlService.GetFeedType(match.Groups[1].Value),
                        EntityId = match.Groups[2].Value,
                    });
                }
            }

            foreach (Match match in Regex.Matches(text, MENTION_REGEX_2))
            {
                if (match.Groups[2].Value == EVERYONE)
                {
                    var members = _membershipRepository.List(m => m.CommunityEntityId == feedId && m.UserId != currentUserId && !m.Deleted);
                    foreach (var member in members)
                    {
                        res.Add(new Mention
                        {
                            EntityType = FeedType.User,
                            EntityId = member.UserId,
                        });
                    }
                }
                else
                {
                    res.Add(new Mention
                    {
                        EntityType = FeedType.User,
                        EntityId = match.Groups[1].Value,
                    });
                }
            }

            return res;
        }

        public bool MentionEveryone(string text)
        {
            foreach (Match match in Regex.Matches(text, MENTION_REGEX))
            {
                if (match.Groups[2].Value == EVERYONE)
                    return true;
            }

            foreach (Match match in Regex.Matches(text, MENTION_REGEX_2))
            {
                if (match.Groups[2].Value == EVERYONE)
                    return true;
            }

            return false;
        }

        public Comment GetComment(string id)
        {
            return _commentRepository.Get(id);
        }

        public ListPage<Comment> ListComments(string contentEntityId, string userId, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var totalCommentCount = _commentRepository.Count(r => r.ContentEntityId == contentEntityId);
            var comments = _commentRepository.List(r => r.ContentEntityId == contentEntityId && !r.Deleted, page, pageSize, sortKey, sortAscending);
            var commentIds = comments.Select(c => c.Id.ToString());
            var users = _userRepository.Get(comments.Select(c => c.CreatedByUserId).ToList());
            var mentions = _mentionRepository.List(m => m.EntityId == userId && commentIds.Contains(m.OriginId) && m.Unread && !m.Deleted);
            var contentEntityRecord = _userContentEntityRecordRepository.Get(ucer => ucer.UserId == userId && ucer.ContentEntityId == contentEntityId);
            EnrichCommentData(comments, users, mentions, contentEntityRecord, userId);

            return new ListPage<Comment>(comments, (int)totalCommentCount);
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            await _commentRepository.UpdateAsync(comment);

            //process attachments
            if (comment.DocumentsToAdd != null)
            {
                foreach (var document in comment.DocumentsToAdd)
                {
                    document.FeedId = comment.FeedId;
                    document.ContentEntityId = comment.ContentEntityId;
                    document.CommentId = comment.Id.ToString();
                    document.CreatedByUserId = comment.UpdatedByUserId ?? comment.CreatedByUserId;
                    document.CreatedUTC = comment.UpdatedUTC ?? DateTime.UtcNow;

                    switch (document.Type)
                    {
                        case DocumentType.Document:
                            document.FileSize = document.Data.Length;
                            var documentId = await _documentRepository.CreateAsync(document);
                            await _storageService.CreateOrReplaceAsync(IStorageService.DOCUMENT_CONTAINER, documentId, document.Data);
                            break;
                        default:
                            throw new Exception($"Cannot create document of type {document.Type}");

                    }
                }
            }

            if (comment.DocumentsToDelete != null)
            {
                var existingAttachments = _documentRepository.List(d => d.CommentId == comment.Id.ToString() && !d.Deleted);
                foreach (var documentId in comment.DocumentsToDelete)
                {
                    //make sure attachment belongs to current comment
                    if (!existingAttachments.Any(d => d.Id.ToString() == documentId))
                        continue;

                    await _documentRepository.DeleteAsync(documentId);
                    await _storageService.DeleteAsync(IStorageService.DOCUMENT_CONTAINER, documentId);
                }
            }
        }

        public async Task DeleteCommentAsync(string commentId)
        {
            //delete mentions
            var mentionIds = _mentionRepository.List(m => m.OriginId == commentId && !m.Deleted).Select(m => m.Id.ToString()).ToList();
            await _mentionRepository.DeleteAsync(mentionIds);

            //delete comment
            await _commentRepository.DeleteAsync(commentId);
        }

        public ContentEntity GetDraftContentEntity(string userId)
        {
            return _contentEntityRepository.Get(ce => ce.CreatedByUserId == userId && ce.Status == ContentEntityStatus.Draft);
        }

        //public List<NodeFeedData> ListNodeMetadata(string userId)
        //{
        //    var nodes = _nodeRepository.List(n => !n.Deleted);
        //    var res = GetNodeMetadata(userId, nodes.ToArray()).Where(f => f.Feeds.Any()).ToList();
        //    res.Insert(0, new NodeFeedData { Id = ObjectId.Empty, Title = "JOGL Global", Feeds = new List<UserFeedRecord>() });

        //    return res;
        //}

        //public NodeFeedData GetNodeMetadata(string nodeId, string userId)
        //{
        //    var node = _nodeRepository.Get(nodeId);
        //    return GetNodeMetadata(userId, node).Single();
        //}

        //private List<NodeFeedData> GetNodeMetadata(string userId, params Node[] nodes)
        //{
        //    var allRelations = _relationRepository.List(r => !r.Deleted);
        //    var currentUserEventAttendances = _eventAttendanceRepository.List(ea => ea.UserId == userId && ea.Status == AttendanceStatus.Yes && !ea.Deleted);
        //    var currentUserMemberships = _membershipRepository.List(m => !m.Deleted && m.UserId == userId);

        //    var nodeIds = nodes.Select(n => n.Id.ToString()).ToList();
        //    EnrichCommunityEntitiesWithMembershipData(nodes, currentUserMemberships, allRelations, userId);

        //    var communityEntityIds = GetCommunityEntityIdsForNodes(allRelations, nodeIds);
        //    var events = _eventRepository.List(e => communityEntityIds.Contains(e.CommunityEntityId) && !e.Deleted);
        //    var feedEntityIds = GetFeedEntityIdsForNodes(allRelations, events, nodeIds);
        //    var needs = _needRepository.List(n => communityEntityIds.Contains(n.EntityId) && !n.Deleted);
        //    var documents = _documentRepository.List(d => feedEntityIds.Contains(d.FeedId) && d.ContentEntityId == null && d.CommentId == null && !d.Deleted);
        //    var papers = _paperRepository.List(p => communityEntityIds.Any(eId => p.FeedIds.Contains(eId)) && !p.Deleted);

        //    var allUserFeedRecords = _userFeedRecordRepository.List(r => r.UserId == userId && !r.Deleted);
        //    var activeUserFeedRecords = allUserFeedRecords.Where(ufr => (ufr.LastReadUTC.HasValue || ufr.LastMentionUTC.HasValue) && !ufr.Muted);
        //    var activeFeedIds = activeUserFeedRecords.Select(ufr => ufr.FeedId).ToList();
        //    var activeFeedEntities = _feedEntityService.GetFeedEntitySetForCommunities(activeFeedIds);
        //    EnrichCommunityEntitiesWithMembershipData(activeFeedEntities.CommunityEntities, currentUserMemberships, allRelations, userId);

        //    events = GetFilteredEvents(events, currentUserEventAttendances, currentUserMemberships, userId);
        //    needs = GetFilteredNeeds(needs);
        //    documents = GetFilteredDocuments(documents, userId);
        //    papers = GetFilteredPapers(papers);

        //    var mentions = _mentionRepository.List(m => m.EntityId == userId && activeFeedIds.Contains(m.OriginFeedId) && m.Unread && !m.Deleted);
        //    var contentEntities = _contentEntityRepository.List(ce => activeFeedIds.Contains(ce.FeedId) && !ce.Deleted);
        //    var userContentEntityRecords = _userContentEntityRecordRepository.List(ucer => ucer.UserId == userId && activeFeedIds.Contains(ucer.FeedId) && !ucer.Deleted);
        //    var userContentEntityRecordsEntityIds = userContentEntityRecords.Select(ucer => ucer.ContentEntityId);
        //    var userContentEntityRecordsComments = _commentRepository.List(c => userContentEntityRecordsEntityIds.Contains(c.ContentEntityId) && !c.Deleted);

        //    var unreadPosts = contentEntities.Where(ce => ce.CreatedUTC > (activeUserFeedRecords.SingleOrDefault(ufr => ce.FeedId == ufr.FeedId)?.LastReadUTC ?? DateTime.MaxValue)).ToList();
        //    var unreadMentions = mentions.ToList();
        //    var unreadThreads = contentEntities.Where(ce => userContentEntityRecordsComments.Any(c => c.ContentEntityId == ce.Id.ToString() && c.CreatedUTC > (userContentEntityRecords.SingleOrDefault(r => r.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue))).ToList();

        //    foreach (var ufr in activeUserFeedRecords)
        //    {
        //        ufr.UnreadPosts = unreadPosts.Count(ce => ce.FeedId == ufr.FeedId);
        //        ufr.UnreadMentions = unreadMentions.Count(m => m.OriginFeedId == ufr.FeedId);
        //        ufr.UnreadThreads = unreadThreads.Count(ce => ce.FeedId == ufr.FeedId);
        //    }

        //    var res = new List<NodeFeedData>();
        //    foreach (var node in nodes)
        //    {
        //        res.Add(GetNodeMetadata(node, userId, activeFeedEntities, allRelations, currentUserMemberships, currentUserEventAttendances, events, needs, documents, papers, allUserFeedRecords, unreadPosts, unreadMentions, unreadThreads));
        //    }

        //    return res;
        //}

        //private NodeFeedData GetNodeMetadata(Node node, string userId, FeedEntitySet feedEntitySet, List<Relation> allRelations, List<Membership> currentUserMemberships, List<EventAttendance> eventAttendances, List<Event> events, List<Need> needs, List<Document> documents, List<Paper> papers, List<UserFeedRecord> allUserFeedRecords, List<ContentEntity> unreadPosts, List<Mention> unreadMentions, List<ContentEntity> unreadThreads)
        //{
        //    var nfd = new NodeFeedData()
        //    {
        //        Id = node.Id,
        //        Title = node.Title,
        //        ShortTitle = node.ShortTitle,
        //        Description = node.Description,
        //        UpdatedUTC = node.UpdatedUTC,
        //        CreatedUTC = node.CreatedUTC,
        //        BannerId = node.BannerId,
        //        LogoId = node.LogoId,
        //        CreatedByUserId = node.CreatedByUserId,
        //        UpdatedByUserId = node.UpdatedByUserId,
        //        LastActivityUTC = node.LastActivityUTC,
        //        ContentPrivacy = node.ContentPrivacy,
        //        ListingPrivacy = node.ListingPrivacy,
        //        FeedId = node.FeedId,
        //        Permissions = node.Permissions,
        //        Feeds = new List<UserFeedRecord>()
        //    };

        //    var feedEntitiesIdsForNode = GetFeedEntityIdsForNode(allRelations, events, nfd.Id.ToString());

        //    events = events.Where(e => feedEntitiesIdsForNode.Contains(e.CommunityEntityId)).ToList();
        //    needs = needs.Where(n => feedEntitiesIdsForNode.Contains(n.CommunityEntityId)).ToList();
        //    documents = documents.Where(d => feedEntitiesIdsForNode.Contains(d.FeedEntityId)).ToList();
        //    papers = papers.Where(p => feedEntitiesIdsForNode.Any(id => p.FeedIds.Contains(id))).ToList();

        //    nfd.NewEvents = events.Any(e => !allUserFeedRecords.Any(ufr => ufr.FeedId == e.Id.ToString()));
        //    nfd.NewNeeds = needs.Any(n => (IsNeedForUser(n, userId) || nfd.Permissions.Contains(Permission.Read)) && !allUserFeedRecords.Any(ufr => ufr.FeedId == n.Id.ToString()));
        //    nfd.NewDocuments = documents.Any(d => !allUserFeedRecords.Any(ufr => ufr.FeedId == d.Id.ToString()));
        //    nfd.NewPapers = papers.Any(p => nfd.Permissions.Contains(Permission.Read) && !allUserFeedRecords.Any(ufr => ufr.FeedId == p.Id.ToString()));

        //    var activeUserFeedRecords = allUserFeedRecords.Where(ufr => ufr.LastReadUTC.HasValue || ufr.LastMentionUTC.HasValue || ufr.LastWriteUTC.HasValue);

        //    nfd.UnreadPostsInEvents = unreadPosts.Count(ce => events.Any(e => e.Id.ToString() == ce.FeedId));
        //    nfd.UnreadPostsInNeeds = unreadPosts.Count(ce => needs.Any(n => n.Id.ToString() == ce.FeedId));
        //    nfd.UnreadPostsInDocuments = unreadPosts.Count(ce => documents.Any(d => d.Id.ToString() == ce.FeedId));
        //    nfd.UnreadPostsInPapers = unreadPosts.Count(ce => papers.Any(p => p.Id.ToString() == ce.FeedId));

        //    nfd.UnreadMentionsInEvents = unreadMentions.Count(m => events.Any(e => IsEventForUser(e, eventAttendances, userId) && e.Id.ToString() == m.OriginFeedId));
        //    nfd.UnreadMentionsInNeeds = unreadMentions.Count(m => needs.Any(n => IsNeedForUser(n, userId) && n.Id.ToString() == m.OriginFeedId));
        //    nfd.UnreadMentionsInDocuments = unreadMentions.Count(m => documents.Any(d => d.Id.ToString() == m.OriginFeedId));
        //    nfd.UnreadMentionsInPapers = unreadMentions.Count(m => papers.Any(p => p.Id.ToString() == m.OriginFeedId));

        //    nfd.UnreadThreadsInEvents = unreadThreads.Count(ce => events.Any(e => IsEventForUser(e, eventAttendances, userId) && e.Id.ToString() == ce.FeedId));
        //    nfd.UnreadThreadsInNeeds = unreadThreads.Count(ce => needs.Any(n => IsNeedForUser(n, userId) && n.Id.ToString() == ce.FeedId));
        //    nfd.UnreadThreadsInDocuments = unreadThreads.Count(ce => documents.Any(d => d.Id.ToString() == ce.FeedId));
        //    nfd.UnreadThreadsInPapers = unreadThreads.Count(ce => papers.Any(p => p.Id.ToString() == ce.FeedId));

        //    foreach (var ufr in activeUserFeedRecords)
        //    {
        //        //populate feed entity; if entity can't be loaded, skip
        //        ufr.FeedEntity = _feedEntityService.GetEntityFromLists(ufr.FeedId, feedEntitySet);
        //        if (ufr.FeedEntity == null)
        //            continue;

        //        ////if not container, skip
        //        //if (!(ufr.FeedEntity is CommunityEntity))
        //        //    continue;

        //        ////if not readable, skip
        //        //if (!(ufr.FeedEntity as CommunityEntity).Permissions.Contains(Permission.Read))
        //        //    continue;

        //        //if not a container member, skip
        //        var membership = currentUserMemberships.SingleOrDefault(m => m.CommunityEntityId == ufr.FeedId);
        //        if (membership == null)
        //            continue;

        //        var communityEntity = ufr.FeedEntity as CommunityEntity;
        //        if (communityEntity?.Status == "draft")
        //            continue;

        //        switch (ufr.FeedEntity.FeedType)
        //        {
        //            case FeedType.Project:
        //            case FeedType.Workspace:
        //            case FeedType.Organization:
        //                if (ShouldAdd(allRelations, currentUserMemberships, ufr.FeedId, nfd.Id.ToString()))
        //                    nfd.Feeds.Add(ufr);
        //                break;
        //            case FeedType.CallForProposal:
        //                var communityId = (ufr.FeedEntity as CallForProposal).ParentCommunityEntityId;
        //                if (ShouldAdd(allRelations, currentUserMemberships, communityId, nfd.Id.ToString()))
        //                    nfd.Feeds.Add(ufr);
        //                break;
        //            case FeedType.Node:
        //                if (nfd.Id.ToString() == ufr.FeedId)
        //                    nfd.Feeds.Add(ufr);
        //                break;
        //        }
        //    }

        //    var unreadPostsInFeeds = unreadPosts.Count(ce => nfd.Feeds.Any(ufr => ufr.FeedId == ce.FeedId));
        //    nfd.UnreadPostsTotal = unreadPostsInFeeds + nfd.UnreadPostsInEvents + nfd.UnreadPostsInNeeds + nfd.UnreadPostsInDocuments + nfd.UnreadPostsInPapers;

        //    var unreadMentionsInFeeds = unreadMentions.Count(m => nfd.Feeds.Any(ufr => ufr.FeedId == m.OriginFeedId));
        //    nfd.UnreadMentionsTotal = unreadMentionsInFeeds + nfd.UnreadMentionsInEvents + nfd.UnreadMentionsInNeeds + nfd.UnreadMentionsInDocuments + nfd.UnreadMentionsInPapers;

        //    var unreadThreadsInFeeds = unreadThreads.Count(ce => nfd.Feeds.Any(ufr => ufr.FeedId == ce.FeedId));
        //    nfd.UnreadThreadsTotal = unreadThreadsInFeeds + nfd.UnreadThreadsInEvents + nfd.UnreadThreadsInNeeds + nfd.UnreadThreadsInDocuments + nfd.UnreadThreadsInPapers;

        //    return nfd;
        //}



        //private bool ShouldAdd(IEnumerable<Relation> allRelations, IEnumerable<Membership> currentUserMemberships, string communityEntityId, string nodeId)
        //{
        //    var nodeIds = GetNodesForCommunityEntityId(allRelations, communityEntityId);

        //    //if nodes exist where user is a member, discard other nodes
        //    var memberNodeIds = nodeIds.Where(nId => currentUserMemberships.Any(m => nId == m.CommunityEntityId)).ToList();
        //    if (memberNodeIds.Any())
        //        nodeIds = memberNodeIds;
        //    else if (nodeIds.Count() > 1)
        //        nodeIds = new List<string> { nodeIds.First() };

        //    return nodeIds.Contains(nodeId);
        //}

        public List<NodeFeedData> ListNodeMetadata(string userId)
        {
            var nodes = _nodeRepository.List(n => !n.Deleted);
            var res = GetNodeMetadata(userId, nodes.ToArray()).Where(f => f.Entities.Any()).ToList();
            res.Insert(0, new NodeFeedData { Id = ObjectId.Empty, Title = "JOGL Global", Entities = new List<CommunityEntity>() });

            return res;
        }

        public NodeFeedData GetDefaultNodeMetadata(string userId)
        {
            var allRelations = _relationRepository.List(r => !r.Deleted);
            var currentUserMemberships = _membershipRepository.List(m => !m.Deleted && m.UserId == userId);

            var nodeId = GetNodeIdsForMemberships(allRelations, currentUserMemberships).FirstOrDefault();

            if (nodeId == null)
                return null;

            var node = _nodeRepository.Get(nodeId);
            return GetNodeMetadata(userId, node).Single();
        }

        public NodeFeedData GetNodeMetadata(string nodeId, string userId)
        {
            var node = _nodeRepository.Get(nodeId);
            return GetNodeMetadata(userId, node).Single();
        }

        private List<NodeFeedData> GetNodeMetadata(string userId, params Node[] nodes)
        {
            var allRelations = _relationRepository.List(r => !r.Deleted);
            var currentUserEventAttendances = _eventAttendanceRepository.List(ea => ea.UserId == userId && ea.Status == AttendanceStatus.Yes && !ea.Deleted);
            var currentUserMemberships = _membershipRepository.List(m => !m.Deleted && m.UserId == userId);

            var nodeIds = nodes.Select(n => n.Id.ToString()).ToList();
            var communityEntityIds = GetCommunityEntityIdsForNodes(allRelations, nodeIds).Where(id => currentUserMemberships.Any(m => m.CommunityEntityId == id)).ToList();
            var communityEntities = _feedEntityService.GetFeedEntitySetForCommunities(communityEntityIds).CommunityEntities;

            var events = _eventRepository.List(e => communityEntityIds.Contains(e.CommunityEntityId) && !e.Deleted);
            events = GetFilteredEvents(events, currentUserEventAttendances, currentUserMemberships, userId, new List<EventTag>(), null);
            var feedEntityIds = GetFeedEntityIdsForNodes(allRelations, events, nodeIds);
            var needs = _needRepository.List(n => communityEntityIds.Contains(n.EntityId) && !n.Deleted);
            needs = GetFilteredFeedEntities(needs, userId);
            var documents = _documentRepository.List(d => feedEntityIds.Contains(d.FeedId) && d.ContentEntityId == null && d.CommentId == null && !d.Deleted);
            documents = GetFilteredDocuments(documents, userId);
            var papers = _paperRepository.List(p => communityEntityIds.Any(eId => p.FeedId == eId) && !p.Deleted);
            papers = GetFilteredFeedEntities(papers, userId);

            var allUserFeedRecords = _userFeedRecordRepository.List(r => r.UserId == userId && !r.Deleted);
            var activeUserFeedRecords = allUserFeedRecords.Where(ufr => (ufr.LastReadUTC.HasValue || ufr.LastWriteUTC.HasValue || ufr.LastMentionUTC.HasValue) && !ufr.Muted);
            var activeFeedIds = activeUserFeedRecords.Select(ufr => ufr.FeedId).ToList();

            var mentions = _mentionRepository.List(m => m.EntityId == userId && m.Unread && !m.Deleted);
            var contentEntities = _contentEntityRepository.List(ce => activeFeedIds.Contains(ce.FeedId) && !ce.Deleted);
            var userContentEntityRecords = _userContentEntityRecordRepository.List(ucer => ucer.UserId == userId && activeFeedIds.Contains(ucer.FeedId) && !ucer.Deleted);
            var userContentEntityRecordsEntityIds = userContentEntityRecords.Select(ucer => ucer.ContentEntityId);
            var userContentEntityRecordsComments = _commentRepository.List(c => userContentEntityRecordsEntityIds.Contains(c.ContentEntityId) && !c.Deleted);

            var unreadPosts = contentEntities.Where(ce => ce.CreatedUTC > (activeUserFeedRecords.SingleOrDefault(ufr => ce.FeedId == ufr.FeedId)?.LastReadUTC ?? DateTime.MaxValue)).ToList();
            var unreadMentions = mentions.ToList();
            var unreadThreads = contentEntities.Where(ce => userContentEntityRecordsComments.Any(c => c.ContentEntityId == ce.Id.ToString() && c.CreatedUTC > (userContentEntityRecords.SingleOrDefault(r => r.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue))).ToList();

            //initialize channels
            var channels = _channelRepository.List(c => communityEntityIds.Contains(c.CommunityEntityId) && currentUserMemberships.Any(m => m.CommunityEntityId == c.Id.ToString()) && !c.Deleted, SortKey.CreatedDate);
            EnrichChannelsWithMembershipData(channels, currentUserMemberships);
            foreach (var channel in channels)
            {
                channel.UnreadPosts = unreadPosts.Count(ce => ce.FeedId == channel.Id.ToString());
                channel.UnreadMentions = unreadMentions.Count(m => m.OriginFeedId == channel.Id.ToString());
                channel.UnreadThreads = unreadThreads.Count(ce => ce.FeedId == channel.Id.ToString());
            }

            //initialize community entities
            EnrichCommunityEntitiesWithMembershipData(communityEntities, new List<Membership>(), currentUserMemberships, allRelations, userId);
            EnrichCommunityEntitiesWithMembershipData(nodes, new List<Membership>(), currentUserMemberships, allRelations, userId);
            foreach (var communityEntity in communityEntities)
            {
                communityEntity.Channels = channels.Where(c => c.CommunityEntityId == communityEntity.Id.ToString()).OrderBy(c => c.Title).ToList();
            }

            //calculate results
            var res = new List<NodeFeedData>();
            foreach (var node in nodes)
            {
                res.Add(GetNodeMetadata(node, userId, communityEntities, allRelations, currentUserMemberships, currentUserEventAttendances, events, needs, documents, papers, allUserFeedRecords, unreadPosts, unreadMentions, unreadThreads));
            }

            return res;
        }

        private NodeFeedData GetNodeMetadata(Node node, string userId, List<CommunityEntity> communityEntities, List<Relation> allRelations, List<Membership> currentUserMemberships, List<EventAttendance> eventAttendances, List<Event> events, List<Need> needs, List<Document> documents, List<Paper> papers, List<UserFeedRecord> allUserFeedRecords, List<ContentEntity> unreadPosts, List<Mention> unreadMentions, List<ContentEntity> unreadThreads)
        {
            var nfd = new NodeFeedData()
            {
                Id = node.Id,
                Title = node.Title,
                HomeChannelId = node.HomeChannelId,
                ShortTitle = node.ShortTitle,
                Description = node.Description,
                UpdatedUTC = node.UpdatedUTC,
                CreatedUTC = node.CreatedUTC,
                BannerId = node.BannerId,
                LogoId = node.LogoId,
                CreatedByUserId = node.CreatedByUserId,
                UpdatedByUserId = node.UpdatedByUserId,
                LastActivityUTC = node.LastActivityUTC,
                ContentPrivacy = node.ContentPrivacy,
                ListingPrivacy = node.ListingPrivacy,
                FeedId = node.FeedId,
                Permissions = node.Permissions,
                Entities = new List<CommunityEntity>()
            };

            var feedEntitiesIdsForNode = GetFeedEntityIdsForNode(allRelations, events, nfd.Id.ToString());

            events = events.Where(e => feedEntitiesIdsForNode.Contains(e.CommunityEntityId)).ToList();
            needs = needs.Where(n => feedEntitiesIdsForNode.Contains(n.CommunityEntityId)).ToList();
            documents = documents.Where(d => feedEntitiesIdsForNode.Contains(d.FeedEntityId)).ToList();
            papers = papers.Where(p => feedEntitiesIdsForNode.Contains(p.FeedEntityId)).ToList();

            nfd.NewEvents = events.Any(e => !allUserFeedRecords.Any(ufr => ufr.FeedId == e.Id.ToString()));
            nfd.NewNeeds = needs.Any(n => (IsNeedForUser(n, userId) || nfd.Permissions.Contains(Permission.Read)) && !allUserFeedRecords.Any(ufr => ufr.FeedId == n.Id.ToString()));
            nfd.NewDocuments = documents.Any(d => !allUserFeedRecords.Any(ufr => ufr.FeedId == d.Id.ToString()));
            nfd.NewPapers = papers.Any(p => nfd.Permissions.Contains(Permission.Read) && !allUserFeedRecords.Any(ufr => ufr.FeedId == p.Id.ToString()));

            nfd.UnreadPostsInEvents = unreadPosts.Count(ce => events.Any(e => e.Id.ToString() == ce.FeedId));
            nfd.UnreadPostsInNeeds = unreadPosts.Count(ce => needs.Any(n => n.Id.ToString() == ce.FeedId));
            nfd.UnreadPostsInDocuments = unreadPosts.Count(ce => documents.Any(d => d.Id.ToString() == ce.FeedId));
            nfd.UnreadPostsInPapers = unreadPosts.Count(ce => papers.Any(p => p.Id.ToString() == ce.FeedId));

            nfd.UnreadMentionsInEvents = unreadMentions.Count(m => events.Any(e => IsEventForUser(e, eventAttendances, userId) && e.Id.ToString() == m.OriginFeedId));
            nfd.UnreadMentionsInNeeds = unreadMentions.Count(m => needs.Any(n => IsNeedForUser(n, userId) && n.Id.ToString() == m.OriginFeedId));
            nfd.UnreadMentionsInDocuments = unreadMentions.Count(m => documents.Any(d => d.Id.ToString() == m.OriginFeedId));
            nfd.UnreadMentionsInPapers = unreadMentions.Count(m => papers.Any(p => p.Id.ToString() == m.OriginFeedId));

            nfd.UnreadThreadsInEvents = unreadThreads.Count(ce => events.Any(e => IsEventForUser(e, eventAttendances, userId) && e.Id.ToString() == ce.FeedId));
            nfd.UnreadThreadsInNeeds = unreadThreads.Count(ce => needs.Any(n => IsNeedForUser(n, userId) && n.Id.ToString() == ce.FeedId));
            nfd.UnreadThreadsInDocuments = unreadThreads.Count(ce => documents.Any(d => d.Id.ToString() == ce.FeedId));
            nfd.UnreadThreadsInPapers = unreadThreads.Count(ce => papers.Any(p => p.Id.ToString() == ce.FeedId));

            foreach (var valuePair in GetNodeHierarchy(allRelations, communityEntities, nfd.Id.ToString()))
            {
                var communityEntity = communityEntities.SingleOrDefault(ce => ce.Id.ToString() == valuePair.Key);
                if (communityEntity == null)
                    continue;

                nfd.Entities.Add(communityEntity);
                communityEntity.Level = valuePair.Value;
            }

            //foreach (var communityEntity in communityEntities.Where(ce => feedEntitiesIdsForNode.Contains(ce.Id.ToString())))
            //{
            //    nfd.Entities.Add(communityEntity);
            //}

            var unreadPostsInChannels = unreadPosts.Count(ce => nfd.Entities.Any(e => e.Channels.Any(c => c.Id.ToString() == ce.FeedId)));
            nfd.UnreadPostsTotal = unreadPostsInChannels + nfd.UnreadPostsInEvents + nfd.UnreadPostsInNeeds + nfd.UnreadPostsInDocuments + nfd.UnreadPostsInPapers;

            var unreadMentionsInChannels = unreadMentions.Count(m => nfd.Entities.Any(e => e.Channels.Any(c => c.Id.ToString() == m.OriginFeedId)));
            nfd.UnreadMentionsTotal = unreadMentionsInChannels + nfd.UnreadMentionsInEvents + nfd.UnreadMentionsInNeeds + nfd.UnreadMentionsInDocuments + nfd.UnreadMentionsInPapers;

            var unreadThreadsInChannels = unreadThreads.Count(ce => nfd.Entities.Any(e => e.Channels.Any(c => c.Id.ToString() == ce.FeedId)));
            nfd.UnreadThreadsTotal = unreadThreadsInChannels + nfd.UnreadThreadsInEvents + nfd.UnreadThreadsInNeeds + nfd.UnreadThreadsInDocuments + nfd.UnreadThreadsInPapers;

            return nfd;
        }

        private Dictionary<string, int> GetNodeHierarchy(IEnumerable<Relation> allRelations, IEnumerable<CommunityEntity> communityEntities, string nodeId)
        {
            var res = new Dictionary<string, int>();
            var relations = new List<Relation>();

            //figure out relevant relations - pt.1
            foreach (var relation in allRelations)
            {
                if (relation.TargetCommunityEntityId == nodeId)
                    relations.Add(relation);
            }

            //figure out relevant relations - pt.2
            foreach (var relation in allRelations)
            {
                if (relations.Any(r => r.SourceCommunityEntityId == relation.TargetCommunityEntityId))
                    relations.Add(relation);
            }

            //add l0
            res.Add(nodeId, 0);
            foreach (var relation in relations.Where(r => r.TargetCommunityEntityId == nodeId).OrderBy(r => communityEntities.SingleOrDefault(c => c.Id.ToString() == r.SourceCommunityEntityId)?.Title))
            {
                //add l1
                if (relation.TargetCommunityEntityId == nodeId)
                    res.TryAdd(relation.SourceCommunityEntityId, 0);

                //add l2
                foreach (var subRelation in relations.Where(r => r.TargetCommunityEntityId == relation.SourceCommunityEntityId).OrderBy(r => communityEntities.SingleOrDefault(c => c.Id.ToString() == r.SourceCommunityEntityId)?.Title))
                {
                    res.TryAdd(subRelation.SourceCommunityEntityId, 1);
                }
            }

            return res;
        }

        public UserFeedRecord GetFeedRecord(string userId, string feedId)
        {
            return _userFeedRecordRepository.Get(r => r.UserId == userId && r.FeedId == feedId);
        }

        public async Task UpdateFeedRecordAsync(UserFeedRecord record)
        {
            await _userFeedRecordRepository.UpdateAsync(record);
        }

        public async Task SetFeedOpenedAsync(string feedId, string userId)
        {
            await _userFeedRecordRepository.SetFeedOpenedAsync(userId, feedId, DateTime.UtcNow);
        }

        public async Task<bool> SetFeedReadAsync(string feedId, string userId)
        {
            //quick hack to make call fail if feed id isn't valid user
            ObjectId.Parse(feedId);

            var existingUfr = _userFeedRecordRepository.Get(ufr => ufr.FeedId == feedId && ufr.UserId == userId);
            var newestContentEntity = _contentEntityRepository.GetNewest(ce => ce.FeedId == feedId);

            await _userFeedRecordRepository.SetFeedReadAsync(userId, feedId, DateTime.UtcNow);

            var mentions = _mentionRepository.List(m => m.EntityId == userId && m.OriginFeedId == feedId && m.OriginType == MentionOrigin.ContentEntity && m.Unread && !m.Deleted);
            foreach (var mention in mentions)
            {
                await _mentionRepository.SetMentionReadAsync(mention);
            }

            return mentions.Any() || (newestContentEntity?.CreatedUTC ?? DateTime.MinValue) > (existingUfr?.LastReadUTC ?? DateTime.MaxValue);
        }

        public async Task<bool> SetContentEntityReadAsync(string contentEntityId, string feedId, string userId)
        {
            //quick hack to make call fail if feed id isn't valid user
            ObjectId.Parse(feedId);

            var existingUcer = _userContentEntityRecordRepository.Get(ucer => ucer.ContentEntityId == contentEntityId && ucer.UserId == userId);
            var newestComment = _commentRepository.GetNewest(c => c.ContentEntityId == contentEntityId);

            await _userContentEntityRecordRepository.SetContentEntityReadAsync(userId, feedId, contentEntityId, DateTime.UtcNow);

            var comments = _commentRepository.List(c => c.ContentEntityId == contentEntityId);
            var commentIds = comments.Select(c => c.Id.ToString()).ToList();

            var mentions = _mentionRepository.List(m => m.EntityId == userId && m.OriginFeedId == feedId && commentIds.Contains(m.OriginId) && m.OriginType == MentionOrigin.Comment && m.Unread && !m.Deleted);
            foreach (var mention in mentions)
            {
                await _mentionRepository.SetMentionReadAsync(mention);
            }

            return mentions.Any() || (newestComment?.CreatedUTC ?? DateTime.MinValue) > (existingUcer?.LastReadUTC ?? DateTime.MaxValue);
        }

        public async Task SetCommentsReadAsync(List<string> commentIds, string userId)
        {
            var mentions = _mentionRepository.List(m => m.EntityId == userId && commentIds.Contains(m.OriginId) && m.OriginType == MentionOrigin.Comment && m.Unread && !m.Deleted);
            foreach (var mention in mentions)
            {
                await _mentionRepository.SetMentionReadAsync(mention);
            }
        }

        public async Task SetContentEntitiesReadAsync(List<string> contentEntityIds, string feedId, string userId)
        {
            foreach (var contentEntityId in contentEntityIds)
            {
                await _userContentEntityRecordRepository.SetContentEntityReadAsync(userId, feedId, contentEntityId, DateTime.UtcNow);
            }
        }

        public Draft GetDraft(string entityId, string userId)
        {
            return _draftRepository.Get(d => d.EntityId == entityId && d.UserId == userId);
        }

        public async Task SetDraftAsync(string entityId, string userId, string text)
        {
            await _draftRepository.SetDraftAsync(entityId, userId, text, DateTime.UtcNow);
        }

        public async Task<bool> ValidateFeedIntegrationAsync(FeedIntegration feedIntegration)
        {
            switch (feedIntegration.Type)
            {
                case FeedIntegrationType.GitHub:
                    var ghRepo = await _githubFacade.GetRepoAsync(feedIntegration.SourceId, feedIntegration.AccessToken);
                    return ghRepo != null;
                case FeedIntegrationType.HuggingFace:
                    var hfRepo = await _huggingfaceFacade.GetRepoAsync(feedIntegration.SourceId);
                    return hfRepo != null;
                case FeedIntegrationType.Arxiv:
                    var arxivCategories = _arxivFacade.ListCategories();
                    return arxivCategories.Contains(feedIntegration.SourceId);
                case FeedIntegrationType.JOGLAgentPublication:
                    return true;
                case FeedIntegrationType.PubMed:
                    var pmCategories = _pubmedFacade.ListCategories();
                    return pmCategories.Contains(feedIntegration.SourceId);
                default:
                    throw new Exception($"Unable to validate feed integration for type {feedIntegration.Type}");
            }
        }

        public async Task<string> ExchangeFeedIntegrationTokenAsync(FeedIntegrationType type, string authorizationCode)
        {
            switch (type)
            {
                case FeedIntegrationType.GitHub:
                    return await _githubFacade.GetAccessTokenAsync(authorizationCode);
                case FeedIntegrationType.HuggingFace:
                case FeedIntegrationType.Arxiv:
                case FeedIntegrationType.PubMed:
                    return null;
                default:
                    throw new Exception($"Unable to exchange feed integration token for type {type}");
            }
        }

        public List<string> ListFeedIntegrationOptions(FeedIntegrationType feedIntegrationType)
        {
            switch (feedIntegrationType)
            {
                case FeedIntegrationType.GitHub:
                case FeedIntegrationType.HuggingFace:
                    return new List<string>();
                case FeedIntegrationType.Arxiv:
                    return _arxivFacade.ListCategories();
                case FeedIntegrationType.PubMed:
                    return _pubmedFacade.ListCategories();
                default:
                    throw new Exception($"Unable to exchange feed integration token for type {feedIntegrationType}");
            }
        }

        public async Task<string> CreateFeedIntegrationAsync(FeedIntegration feedIntegration)
        {
            //mark feed write
            //await _userFeedRecordRepository.SetFeedWrittenAsync(feedIntegration.CreatedByUserId, feedIntegration.FeedId, DateTime.UtcNow);

            feedIntegration.LastActivityUTC = feedIntegration.CreatedUTC;
            return await _feedIntegrationRepository.CreateAsync(feedIntegration);
        }

        public FeedIntegration GetFeedIntegration(string id)
        {
            return _feedIntegrationRepository.Get(id);
        }

        public FeedIntegration GetFeedIntegration(string feedId, FeedIntegrationType type, string sourceId)
        {
            return _feedIntegrationRepository.Get(i => i.FeedId == feedId && i.Type == type && i.SourceId == sourceId && !i.Deleted);
        }

        public List<FeedIntegration> ListFeedIntegrations(string feedId, string search)
        {
            return _feedIntegrationRepository.SearchList(i => i.FeedId == feedId && !i.Deleted, search);
        }

        public List<FeedIntegration> AutocompleteFeedIntegrations(string feedId, string search)
        {
            return _feedIntegrationRepository.AutocompleteList(i => i.FeedId == feedId && !i.Deleted, search);
        }

        public async Task DeleteIntegrationAsync(FeedIntegration feedIntegration)
        {
            await _feedIntegrationRepository.DeleteAsync(feedIntegration);
        }
    }
}