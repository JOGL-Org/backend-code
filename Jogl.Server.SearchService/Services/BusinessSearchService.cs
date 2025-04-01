using Jogl.Server.Business;

namespace Jogl.Server.Search
{
    public class BusinessSearchService : IBusinessSearchService
    {
        private readonly ISearchService _searchService;
        private readonly IRelationService _relationService;

        public BusinessSearchService(ISearchService searchService, IRelationService relationService)
        {
            _searchService = searchService;
            _relationService = relationService;
        }

        public Task<List<User>> SearchUsersAsync(string query, string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return _searchService.SearchUsersAsync(query);

            var userIds = _relationService.ListUserIdsForNode(nodeId);
            return _searchService.SearchUsersAsync(query, userIds);
        }
    }
}
