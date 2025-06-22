using Jogl.Server.AI.Agent.DTO;
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

        public async Task<AgentResponse> GetInitialResponseAsync(IEnumerable<InputItem> messages, Dictionary<string, string> emailHandles, string? nodeId = default, string? interfaceType = default)
        {
            var queryPrompt = _systemValueRepository.Get(sv => sv.Key == "USER_SEARCH_QUERY_PROMPT");
            if (queryPrompt == null)
            {
                _logger.LogError("USER_SEARCH_QUERY_PROMPT system value missing");
                return new AgentResponse { Text = "An error has ocurred" };
            }

            var resultPromptKey = $"USER_SEARCH_RESULT_PROMPT_{interfaceType}";
            var resultPrompt = _systemValueRepository.Get(sv => sv.Key == resultPromptKey);
            if (resultPrompt == null)
            {
                _logger.LogError("{resultPromptKey} system value missing", resultPromptKey);
                return new AgentResponse { Text = "An error has ocurred" };
            }

            var exampleResult = new PromptResult()
            {
                Explanation = "explanation",
                ExtractedQuery = "topic or field",
                ExtractedConfiguration = "default or current",
                Success = false
            };

            var res = await _aiService.GetResponseAsync<PromptResult>(string.Format(queryPrompt.Value, JsonSerializer.Serialize(exampleResult)), messages, 0);
            if (!res.Success)
                return new AgentResponse { Text = res.Explanation };

            _logger.LogInformation($"Extracted query: {res.ExtractedQuery}");

            var hubUsers = _relationService.ListUserIdsForNode(nodeId);
            var searchResults = await _searchService.SearchUsersAsync(res.ExtractedQuery, res.ExtractedConfiguration);
            var searchResultsText = JsonSerializer.Serialize(searchResults.Select(u => new
            {
                UserURL = $"<{_configuration["App:URL"]}/user/{u.Document.Id}",
                Handle = emailHandles.ContainsKey(u.Document.Email) ? $"@{emailHandles[u.Document.Email]}" : "",
                Source = hubUsers.Contains(u.Document.Id) ? "Internal" : "External",
                u.Document.Name,
                SearchScore = u.SemanticSearch.RerankerScore,
                OriginalData = u.Document,
                Highlights = u.SemanticSearch.Captions
            }));

            var explanationRes = await _aiService.GetResponseAsync(string.Format(resultPrompt.Value, res.ExtractedQuery, searchResultsText), messages);
            return new AgentResponse { Text = explanationRes, Context = searchResultsText };
        }

        public async Task<AgentResponse> GetFollowupResponseAsync(IEnumerable<InputItem> messages, string context, string? interfaceType = default)
        {
            var promptKey = $"USER_SEARCH_FOLLOWUP_PROMPT_{interfaceType}";
            var prompt = _systemValueRepository.Get(sv => sv.Key == promptKey);
            if (prompt == null)
            {
                _logger.LogError("{promptKey} system value missing", promptKey);
                return new AgentResponse { Text = "An error has ocurred" };
            }

            var promptText = string.Format(prompt.Value, context);
            var response = await _aiService.GetResponseAsync(promptText, messages, 0.5m);
            return new AgentResponse { Text = response };
        }
    }
}
