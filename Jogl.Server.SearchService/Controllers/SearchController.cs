using Microsoft.AspNetCore.Mvc;
using Jogl.Server.Search;
using Jogl.Server.AI;
using System.Net;
using Swashbuckle.AspNetCore.Annotations;
using Jogl.Server.SearchService.Models;
using Jogl.Server.DB;

namespace Jogl.Server.SearchService.Controllers
{
    [ApiController]
    [Route("search")]
    public class SearchController : ControllerBase
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IBusinessSearchService _searchService;
        private readonly IAIService _aiService;

        public SearchController(INodeRepository nodeRepository, IBusinessSearchService searchService, IAIService aiService)
        {
            _nodeRepository = nodeRepository;
            _searchService = searchService;
            _aiService = aiService;
        }

        [HttpPost]
        [SwaggerOperation($"Adds a new event for the specified community entity.")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The user search results", typeof(ResponseModel<User>))]
        public async Task<IActionResult> Search([FromBody] SearchModel model)
        {
            var promptResult = await _aiService.GetSearchQueryAsync(model.Query);
            if (!promptResult.Success)
                return Ok(new ResponseModel<User> { Text = promptResult.Explanation });

            var res = await _searchService.SearchUsersAsync(promptResult.ExtractedQuery, model.NodeId);
            foreach (var user in res)
            {
                user.Explanation = await _aiService.ExplainSearchResultAsync(promptResult.ExtractedQuery, user);
            }

            return Ok(new ResponseModel<User> { Results = res });
        }

        [HttpGet]
        [Route("hubs")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Hub data", typeof(List<OptionModel>))]
        public async Task<IActionResult> ListNodes()
        {
            var nodes = _nodeRepository.Query(n => true).ToList();
            var nodeModels = nodes.Select(n => new OptionModel { Id = n.Id.ToString(), Text = n.Title }).ToList();
            return Ok(nodeModels);
        }
    }
}
