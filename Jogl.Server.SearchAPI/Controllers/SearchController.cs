using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.Search;
using Jogl.Server.SearchAPI.Services;
using Jogl.Server.Business;

namespace Jogl.Server.SearchAPI.Controllers
{
    [Authorize]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly IRelationService _relationService;
        private readonly IContextService _contextService;
        private readonly IConfiguration _configuration;

        public SearchController(ISearchService searchService, IRelationService relationService, IContextService contextService, IConfiguration configuration)
        {
            _searchService = searchService;
            _relationService = relationService;
            _contextService = contextService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("id/external")]
        [SwaggerOperation($"Returns a list of external ids matching a search query")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The search results", typeof(List<IdResult>))]
        public async Task<IActionResult> ExternalIds([FromBody] string query)
        {
            var userIds = _relationService.ListUserIdsForNode(_contextService.CurrentNodeId);
            var results = await _searchService.SearchUsersAsync(query, userIds: userIds, minScore: 1);

            var resultModels = results.Select(r => new IdResult { Id = r.Document.Id, SearchScore = r.SemanticSearch.RerankerScore ?? 0 }).ToList();
            return Ok(resultModels);
        }
    }
}
