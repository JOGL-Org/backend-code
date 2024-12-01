using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB;
using Jogl.Server.Notifications;
using Jogl.Server.Storage;
using MongoDB.Bson;

namespace Jogl.Server.Business
{
    public class DocumentService : BaseService, IDocumentService
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IStorageService _storageService;
        private readonly INotificationFacade _notificationFacade;

        public DocumentService(IFolderRepository folderRepository, IStorageService storageService, INotificationFacade notificationFacade, IUserFollowingRepository followingRepository, IMembershipRepository membershipRepository, IInvitationRepository invitationRepository, IRelationRepository relationRepository, INeedRepository needRepository, IDocumentRepository documentRepository, IPaperRepository paperRepository, IResourceRepository resourceRepository, ICallForProposalRepository callForProposalsRepository, IProposalRepository proposalRepository, IContentEntityRepository contentEntityRepository, ICommentRepository commentRepository, IMentionRepository mentionRepository, IReactionRepository reactionRepository, IFeedRepository feedRepository, IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, IEventRepository eventRepository, IEventAttendanceRepository eventAttendanceRepository, IUserRepository userRepository, IChannelRepository channelRepository, IFeedEntityService feedEntityService) : base(followingRepository, membershipRepository, invitationRepository, relationRepository, needRepository, documentRepository, paperRepository, resourceRepository, callForProposalsRepository, proposalRepository, contentEntityRepository, commentRepository, mentionRepository, reactionRepository, feedRepository, userContentEntityRecordRepository, userFeedRecordRepository, eventRepository, eventAttendanceRepository, userRepository, channelRepository, feedEntityService)
        {
            _folderRepository = folderRepository;
            _storageService = storageService;
            _notificationFacade = notificationFacade;
        }

        public async Task<string> CreateAsync(Document document)
        {
            //create feed
            var feed = new Feed()
            {
                CreatedUTC = document.CreatedUTC,
                CreatedByUserId = document.CreatedByUserId,
                Type = FeedType.Document,
            };

            var id = await _feedRepository.CreateAsync(feed);

            //mark feed write
            await _userFeedRecordRepository.SetFeedWrittenAsync(document.CreatedByUserId, id, DateTime.UtcNow);

            //create document
            document.Id = ObjectId.Parse(id);
            await _documentRepository.CreateAsync(document);

            switch (document.Type)
            {
                case DocumentType.Document:
                    document.FileSize = document.Data.Length;
                    await _storageService.CreateOrReplaceAsync(IStorageService.DOCUMENT_CONTAINER, document.Id.ToString(), document.Data);
                    break;
            }


            //process notifications
            await _notificationFacade.NotifyCreatedAsync(document);

            return id;
        }

        [Obsolete]
        public async Task<Document> GetAsync(string documentId, string currentUserId, bool loadData = true)
        {
            var document = _documentRepository.Get(documentId);
            if (document == null)
                return null;

            switch (document.Type)
            {
                case DocumentType.Document:
                    if (loadData)
                        document.Data = await _storageService.GetDocumentAsync(IStorageService.DOCUMENT_CONTAINER, documentId);

                    break;
            }

            var entitySet = _feedEntityService.GetFeedEntitySet(document.FeedId);
            EnrichDocumentData(new List<Document> { document }, entitySet, currentUserId);
            document.Path = _feedEntityService.GetPath(document, currentUserId);
            RecordListing(currentUserId, document);

            return document;
        }

        public Document Get(string documentId)
        {
            return _documentRepository.Get(documentId);
        }

        public Document Get(string documentId, string currentUserId)
        {
            var document = _documentRepository.Get(documentId);
            if (document == null)
                return null;

            var entitySet = _feedEntityService.GetFeedEntitySet(document.FeedId);
            EnrichDocumentData(new List<Document> { document }, entitySet, currentUserId);
            document.Path = _feedEntityService.GetPath(document, currentUserId);
            RecordListing(currentUserId, document);

            return document;
        }

        public async Task<Document> GetDataAsync(string documentId, string currentUserId)
        {
            var document = _documentRepository.Get(documentId);
            if (document == null)
                return null;

            document.Data = await _storageService.GetDocumentAsync(IStorageService.DOCUMENT_CONTAINER, documentId);

            var entitySet = _feedEntityService.GetFeedEntitySet(document.FeedId);
            EnrichDocumentData(new List<Document> { document }, entitySet, currentUserId);
            RecordListing(currentUserId, document);

            return document;
        }

        public Document GetDraft(string entityId, string currentUserId)
        {
            var document = _documentRepository.Get(d => !d.Deleted && d.Status == ContentEntityStatus.Draft && d.FeedId == entityId && d.CreatedByUserId == currentUserId);
            if (document == null)
                return null;

            var entitySet = _feedEntityService.GetFeedEntitySet(document.FeedId);
            EnrichDocumentData(new List<Document> { document }, entitySet, currentUserId);
            document.Path = _feedEntityService.GetPath(document, currentUserId);

            return document;
        }

        public List<Document> ListForEntity(string currentUserId, string entityId, string folderId, DocumentFilter? type, string search, int page, int pageSize)
        {
            var documents = _documentRepository
                .Query(d => d.FeedId == entityId && d.FolderId == folderId && d.ContentEntityId == null)
                .Search(search)
                .ToList();

            var entitySet = _feedEntityService.GetFeedEntitySet(entityId);

            var filteredDocuments = GetFilteredDocuments(documents, entitySet, currentUserId, type, page, pageSize);
            EnrichDocumentData(filteredDocuments, entitySet, currentUserId);
            RecordListings(currentUserId, filteredDocuments);

            return filteredDocuments;
        }

        public ListPage<Document> ListForChannel(string currentUserId, string channelId, DocumentFilter type, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var documents = _documentRepository
                .Query(d => d.FeedId == channelId)
                .Search(search)
                .WithLastOpenedUTC()
                .Sort(sortKey, sortAscending)
                .ToList();

            var entitySet = _feedEntityService.GetFeedEntitySet(channelId);

            var filteredDocuments = GetFilteredDocuments(documents, entitySet, currentUserId, type);
            EnrichDocumentData(filteredDocuments, entitySet, currentUserId);
            RecordListings(currentUserId, filteredDocuments);

            return new ListPage<Document>(GetPage(filteredDocuments, page, pageSize), filteredDocuments.Count);
        }

        public ListPage<Document> ListForNode(string currentUserId, string nodeId,  List<string> communityEntityIds, DocumentFilter? type, FeedEntityFilter? filter, string search, int page, int pageSize, SortKey sortKey, bool ascending)
        {
            var entityIds = GetFeedEntityIdsForNode(nodeId);

            if (communityEntityIds != null && communityEntityIds.Any())
                entityIds = entityIds.Where(communityEntityIds.Contains).ToList();

            var documents = _documentRepository
                .Query(d => entityIds.Contains(d.FeedId) && d.ContentEntityId == null)
                .Search(search)
                .WithLastOpenedUTC()
                .Sort(sortKey, ascending)
                .ToList();

            var entitySet = _feedEntityService.GetFeedEntitySet(entityIds);

            var filteredDocuments = GetFilteredDocuments(documents, entitySet, currentUserId, type, filter);

            var filteredDocumentPage = GetPage(filteredDocuments, page, pageSize);
            EnrichDocumentData(filteredDocumentPage, entitySet, currentUserId);
            RecordListings(currentUserId, filteredDocumentPage);

            return new ListPage<Document>(filteredDocumentPage, filteredDocuments.Count);
        }

        public long CountForNode(string userId, string nodeId, string search)
        {
            var entityIds = GetFeedEntityIdsForNode(nodeId);

            var documents = _documentRepository.Query(d => entityIds.Contains(d.FeedId) && d.ContentEntityId == null).Search(search).ToList();
            var entitySet = _feedEntityService.GetFeedEntitySet(entityIds);
            var filteredDocuments = GetFilteredDocuments(documents, entitySet, userId);

            return filteredDocuments.Count;
        }

        public List<Document> ListAllDocuments(string currentUserId, string entityId, string search, int page, int pageSize)
        {
            var documents = _documentRepository.Query(d => d.FeedId == entityId && d.ContentEntityId == null).Search(search).ToList();

            var feedEntitySet = _feedEntityService.GetFeedEntitySet(entityId);
            var filteredDocuments = GetFilteredDocuments(documents, feedEntitySet, currentUserId);
            EnrichDocumentData(filteredDocuments, feedEntitySet, currentUserId);
            RecordListings(currentUserId, filteredDocuments);

            return GetPage(filteredDocuments, page, pageSize);
        }

        public List<Document> ListAllDocuments(string currentUserId, string entityId)
        {
            var documents = _documentRepository.Query(d => d.FeedId == entityId && d.ContentEntityId == null).ToList();

            var feedEntitySet = _feedEntityService.GetFeedEntitySet(entityId);
            var filteredDocuments = GetFilteredDocuments(documents, feedEntitySet, currentUserId);
            EnrichDocumentData(filteredDocuments, feedEntitySet, currentUserId);
            RecordListings(currentUserId, filteredDocuments);

            return filteredDocuments;
        }

        public async Task UpdateAsync(Document document)
        {
            var existingDocument = _documentRepository.Get(document.Id.ToString());
            if (existingDocument.Status != ContentEntityStatus.Draft && document.Status == ContentEntityStatus.Draft)
                throw new Exception($"Can not set document status back into draft");

            await _documentRepository.UpdateAsync(document);
            await _notificationFacade.NotifyUpdatedAsync(document);
        }

        public async Task DeleteAsync(string id)
        {
            var document = _documentRepository.Get(id);
            switch (document.Type)
            {
                case DocumentType.Document:
                    await _storageService.DeleteAsync(IStorageService.DOCUMENT_CONTAINER, id);
                    break;
                case DocumentType.JoglDoc:
                    break;
            }

            await DeleteFeedAsync(id);
            await _documentRepository.DeleteAsync(id);
        }

        protected void EnrichDocumentData(IEnumerable<Document> documents, FeedEntitySet feedEntitySet, string currentUserId)
        {
            var users = _userRepository.Get(documents.Select(d => d.CreatedByUserId).Union(documents.SelectMany(d => d.UserIds ?? new List<string>())).ToList());
            var contentEntities = _contentEntityRepository.List(ce => documents.Any(d => d.Id.ToString() == ce.FeedId) && !ce.Deleted);
            //var comments = _commentRepository.List(co => documents.Any(d => d.Id.ToString() == co.FeedId) && !co.Deleted);
            var documentIds = documents.Select(e => e.Id.ToString());
            var userFeedRecords = _userFeedRecordRepository.List(ufr => ufr.UserId == currentUserId && documentIds.Contains(ufr.FeedId));
            var userContentEntityRecords = _userContentEntityRecordRepository.List(ucer => ucer.UserId == currentUserId && documentIds.Contains(ucer.FeedId) && !ucer.Deleted);
            var mentions = _mentionRepository.List(m => m.EntityId == currentUserId && m.Unread && documentIds.Contains(m.OriginFeedId) && !m.Deleted);

            foreach (var doc in documents)
            {
                var feedRecord = userFeedRecords.SingleOrDefault(ufr => ufr.FeedId == doc.Id.ToString());

                doc.FeedEntity = _feedEntityService.GetEntityFromLists(doc.FeedId, feedEntitySet);
                doc.Users = users.Where(u => doc.UserIds?.Contains(u.Id.ToString()) == true).ToList();
                doc.PostCount = contentEntities.Count(ce => ce.FeedId == doc.Id.ToString());
                doc.NewPostCount = contentEntities.Count(ce => ce.FeedId == doc.Id.ToString() && ce.CreatedUTC > (feedRecord?.LastReadUTC ?? DateTime.MinValue));
                doc.NewMentionCount = mentions.Count(m => m.OriginFeedId == doc.Id.ToString());
                doc.NewThreadActivityCount = contentEntities.Count(ce => ce.FeedId == doc.Id.ToString() && ce.LastActivityUTC > (userContentEntityRecords.SingleOrDefault(ucer => ucer.ContentEntityId == ce.Id.ToString())?.LastReadUTC ?? DateTime.MaxValue));
                doc.IsNew = feedRecord == null;
                doc.IsMedia = IsMedia(doc);
            }

            EnrichFeedEntitiesWithVisibilityData(documents);
            EnrichDocumentsWithPermissions(documents, currentUserId);
            EnrichEntitiesWithCreatorData(documents, users);
        }

        protected void EnrichFolderData(IEnumerable<Folder> folders)
        {
            var users = _userRepository.Get(folders.Select(d => d.CreatedByUserId).ToList());
            EnrichEntitiesWithCreatorData(folders, users);
        }

        public async Task<string> CreateFolderAsync(Folder folder)
        {
            return await _folderRepository.CreateAsync(folder);
        }

        public Folder GetFolder(string folderId)
        {
            return _folderRepository.Get(folderId);
        }

        public List<Folder> ListAllFolders(string entityId, string search, int page, int pageSize)
        {
            var folders = _folderRepository
                .Query(f => f.FeedId == entityId)
                .Search(search)
                .Page(page, pageSize)
                .ToList();

            EnrichFolderData(folders);
            return folders;
        }

        public List<Folder> ListAllFolders(string entityId)
        {
            var folders = _folderRepository
                .Query(f => f.FeedId == entityId)
                .ToList();

            EnrichFolderData(folders);
            return folders;
        }

        public List<Folder> ListFolders(string entityId, string parentFolderId, string search, int page, int pageSize)
        {
            return _folderRepository
                .Query(f => f.ParentFolderId == parentFolderId && f.FeedId == entityId && !f.Deleted)
                .Search(search)
                .Page(page, pageSize)
                .ToList();
        }

        public async Task UpdateFolderAsync(Folder folder)
        {
            await _folderRepository.UpdateAsync(folder);
        }

        public async Task DeleteFolderAsync(string id)
        {
            var folder = _folderRepository.Get(id);

            var documents = _documentRepository.Query(d => d.FeedId == folder.FeedId && d.FolderId == id).ToList();
            foreach (var document in documents)
            {
                await DeleteAsync(document.Id.ToString());
            }

            var subfolders = _folderRepository.Query(f => f.FeedId == folder.FeedId && f.ParentFolderId == id).ToList();
            foreach (var subfolder in subfolders)
            {
                await DeleteFolderAsync(subfolder.Id.ToString());
            }

            await _folderRepository.DeleteAsync(id);
        }

        public List<FeedEntity> ListFeedEntitiesForNodeDocuments(string currentUserId, string nodeId, List<CommunityEntityType> types, DocumentFilter? type, string search, int page, int pageSize)
        {
            var entityIds = GetFeedEntityIdsForNode(nodeId);

            var documents = _documentRepository.Query(d => entityIds.Contains(d.FeedId)).Search(search).ToList();
            var entitySet = _feedEntityService.GetFeedEntitySet(entityIds);

            var filteredDocuments = GetFilteredDocuments(documents, entitySet, currentUserId, type);
            EnrichDocumentData(filteredDocuments, entitySet, currentUserId);

            return GetPage(filteredDocuments.Select(e => e.FeedEntity).DistinctBy(e => e.Id), page, pageSize);
        }

        public List<Entity> ListPortfolioForUser(string currentUserId, string userId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending)
        {
            var papers = _paperRepository.Query(p => p.FeedId == userId).Search(search).ToList();
            var documents = _documentRepository.Query(d => d.FeedId == userId && d.Type == DocumentType.JoglDoc).Search(search).ToList();
            var filteredDocuments = GetFilteredJoglDocs(documents, currentUserId);

            EnrichPapersWithPermissions(papers, currentUserId);
            EnrichDocumentsWithPermissions(filteredDocuments, currentUserId);

            var entities = new List<FeedEntity>();
            entities.AddRange(papers);
            entities.AddRange(filteredDocuments);

            EnrichFeedEntitiesWithFeedStats(entities);

            return GetPage(entities.Cast<Entity>().OrderByDescending(e => e.CreatedUTC), page, pageSize);
        }
    }
}