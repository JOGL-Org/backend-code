using Jogl.Server.Business;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Jogl.Server.AI.Agent
{
    public class UserSearchAgent : IAgent
    {
        private readonly IAIService _aiService;
        private readonly Search.ISearchService _searchService;
        private readonly IRelationService _relationService;
        private readonly ISystemValueRepository _systemValueRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserSearchAgent> _logger;

        public UserSearchAgent(IAIService aIService, Search.ISearchService searchService, IRelationService relationService, ISystemValueRepository systemValueRepository, IConfiguration configuration, ILogger<UserSearchAgent> logger)
        {
            _aiService = aIService;
            _searchService = searchService;
            _relationService = relationService;
            _systemValueRepository = systemValueRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetResponseAsync(IEnumerable<InputItem> messages, string? nodeId = default)
        {
            var queryPrompt = _systemValueRepository.Get(sv => sv.Key == "USER_SEARCH_QUERY_PROMPT");
            if (queryPrompt == null)
            {
                _logger.LogError("USER_SEARCH_QUERY_PROMPT system value missing");
                return null;
            }

            var resultPrompt = _systemValueRepository.Get(sv => sv.Key == "USER_SEARCH_RESULT_PROMPT");
            if (resultPrompt == null)
            {
                _logger.LogError("USER_SEARCH_RESULT_PROMPT system value missing");
                return null;
            }

            var exampleResult = new PromptResult()
            {
                Explanation = "explanation",
                ExtractedQuery = "topic or field",
                Success = false
            };

            var res = await _aiService.GetResponseAsync<PromptResult>(string.Format(queryPrompt.Value, JsonSerializer.Serialize(exampleResult)), messages, 0);
            if (!res.Success)
                return res.Explanation;

            _logger.LogInformation($"Extracted query: {res.ExtractedQuery}");



            var hubUsers = _relationService.ListUserIdsForNode(nodeId);
            var searchResults = await _searchService.SearchUsersAsync(res.ExtractedQuery);
            var searchResultsText = JsonSerializer.Serialize(searchResults.Select(u => new
            {
                UserURL = $"<{_configuration["App:URL"]}/user/{u.Document.Id}",
                Source = hubUsers.Contains(u.Document.Id) ? "Internal" : "External",
                u.Document.Name,
                SearchScore = u.SemanticSearch.RerankerScore,
                OriginalData = u.Document
            }));

            var explanationRes = await _aiService.GetResponseAsync(string.Format(resultPrompt.Value, res.ExtractedQuery, searchResultsText), messages);
            return explanationRes;
        }
    }
}
