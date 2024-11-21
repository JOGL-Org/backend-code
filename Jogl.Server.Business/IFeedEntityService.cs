using Jogl.Server.Data.Util;
using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface IFeedEntityService
    {
        string GetPrintName(FeedType feedType);
        FeedEntitySet GetFeedEntitySet(IEnumerable<string> feedIds);
        FeedEntitySet GetFeedEntitySetForCommunities(IEnumerable<string> feedIds);
        void PopulateFeedEntities(IEnumerable<ICommunityEntityOwned> entities);
        FeedEntitySet GetFeedEntitySet(string feedId);
        FeedEntity GetEntityFromLists(string entityId, FeedEntitySet entitySet);
        Task UpdateActivityAsync(string entityId, DateTime updatedUTC, string updatedByUserId);
        List<FeedEntity> GetPath(FeedEntity feedEntity, string currentUserId);
        List<FeedEntity> GetPath(string entityId, string currentUserId);

        public FeedEntity GetEntity(string id);
        public FeedEntity GetEntity(string id, string userId);
    }
}
