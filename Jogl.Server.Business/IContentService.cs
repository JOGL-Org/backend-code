using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IContentService
    {
        Feed GetFeed(string feedId);

        Task<string> CreateAsync(ContentEntity entity);
        ContentEntity Get(string entityId);
        ContentEntity GetDetail(string entityId, string userId);

        Discussion GetDiscussion(string currentUserId, string feedId, ContentEntityType? type, ContentEntityFilter filter, string search, int page, int pageSize);
        List<ContentEntity> ListMessageEntities(string feedId);
        ListPage<ContentEntity> ListPostContentEntities(string currentUserId, string feedId, ContentEntityType? type, string search, int page, int pageSize);
        ListPage<ContentEntity> ListMentionContentEntities(string currentUserId, string feedId, ContentEntityType? type, string search, int page, int pageSize);
        ListPage<ContentEntity> ListThreadContentEntities(string currentUserId, string feedId, ContentEntityType? type, string search, int page, int pageSize);
        List<DiscussionItem> ListContentEntitiesForNode(string currentUserId, string nodeId, int page, int pageSize);
        List<DiscussionItem> ListThreadsForNode(string currentUserId, string nodeId, int page, int pageSize);
        List<DiscussionItem> ListMentionsForNode(string currentUserId, string nodeId, int page, int pageSize);
        bool ListForNodeHasNewContent(string currentUserId, string nodeId);
        Task UpdateAsync(ContentEntity contentEntity);
        Task DeleteAsync(string id);

        Task<string> CreateReactionAsync(Reaction reaction);
        Reaction GetReaction(string id);
        Reaction GetReaction(string originId, string userId);
        List<Reaction> ListReactions(string originId, string currentUserId);
        Task UpdateReactionAsync(Reaction reaction);
        Task DeleteReactionAsync(string reactionId);

        Task<string> CreateCommentAsync(Comment comment);
        Comment GetComment(string id);
        //Comment GetComment(string feedId, string contentEntityId, ReactionType type, string userId);
        ListPage<Comment> ListComments(string contentEntityId, string userId, int page, int pageSize, SortKey sortKey, bool sortAscending);
        Task UpdateCommentAsync(Comment comment);
        Task DeleteCommentAsync(string commentId);

        ContentEntity GetDraftContentEntity(string userId);

        //List<NodeFeedData> ListNodeMetadata(string userId);
        List<NodeFeedData> ListNodeMetadata(string userId);
        NodeFeedData GetNodeMetadata(string nodeId, string userId);
        UserFeedRecord GetFeedRecord(string userId, string feedId);
        Task UpdateFeedRecordAsync(UserFeedRecord record);

        Task SetFeedOpenedAsync(string feedId, string userId);
        Task<bool> SetFeedReadAsync(string feedId, string userId);
        Task SetCommentsReadAsync(List<string> commentIds, string userId);
        Task<bool> SetContentEntityReadAsync(string contentEntityId, string feedId, string userId);
        Task SetContentEntitiesReadAsync(List<string> contentEntities, string feedId, string userId);

        bool MentionEveryone(string text);
        bool HasNewContent(string currentUserId, string feedId);

        Draft GetDraft(string entityId, string userId);
        Task SetDraftAsync(string entityId, string userId, string text);

        Task<bool> ValidateFeedIntegrationAsync(FeedIntegration feedIntegration);
        Task<string> ExchangeFeedIntegrationTokenAsync(FeedIntegrationType type, string authorizationCode);
        List<string> ListFeedIntegrationOptions(FeedIntegrationType feedIntegrationType);
        Task<string> CreateFeedIntegrationAsync(FeedIntegration feedIntegration);
        FeedIntegration GetFeedIntegration(string id);
        FeedIntegration GetFeedIntegration(string feedId, FeedIntegrationType type, string sourceId);
        List<FeedIntegration> ListFeedIntegrations(string feedId, string search);
        List<FeedIntegration> AutocompleteFeedIntegrations(string feedId, string search);
        Task DeleteIntegrationAsync(FeedIntegration feedIntegration);
    }
}