using Microsoft.AspNetCore.Mvc;
using Jogl.Server.Search;
using Jogl.Server.AI;
using System.Net;
using Swashbuckle.AspNetCore.Annotations;

namespace Jogl.Server.API.Controllers
{
    [ApiController]
    [Route("search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly IAIService _aiService;

        public SearchController(ISearchService searchService, IAIService aiService)
        {
            _searchService = searchService;
            _aiService = aiService;
        }

        [HttpPost]
        //[SwaggerOperation($"Adds a new event for the specified community entity.")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The user search results", typeof(ResponseModel<User>))]
        public async Task<IActionResult> Search([FromBody] SearchModel model)
        {
            var promptResult = await _aiService.GetSearchQueryAsync(model.Query);
            if (!promptResult.Success)
                return Ok(new ResponseModel<User> { Text = promptResult.Explanation });

            var res = await _searchService.SearchUsersAsync(promptResult.ExtractedQuery);
            foreach (var user in res)
            {
                user.Explanation = await _aiService.ExplainSearchResultAsync(promptResult.ExtractedQuery, user);
            }

            return Ok(new ResponseModel<User> { Results = res });
        }
    }
}
