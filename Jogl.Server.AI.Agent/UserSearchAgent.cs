using Azure.Search.Documents.Models;
using Jogl.Server.AI.Agent.DTO;
using Jogl.Server.Business;
using Jogl.Server.DB;
using Jogl.Server.Search.Model;
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
        private readonly IPaperService _paperService;
        private readonly IResourceService _resourceService;
        private readonly ISystemValueRepository _systemValueRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserSearchAgent> _logger;

        public UserSearchAgent(IAIService aIService, Search.ISearchService searchService, IRelationService relationService, IPaperService paperService, IResourceService resourceService, ISystemValueRepository systemValueRepository, IConfiguration configuration, ILogger<UserSearchAgent> logger)
        {
            _aiService = aIService;
            _searchService = searchService;
            _relationService = relationService;
            _paperService = paperService;
            _resourceService = resourceService;
            _systemValueRepository = systemValueRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AgentResponse> GetInitialResponseAsync(IEnumerable<InputItem> messages, string? nodeId = default, string? interfaceType = default)
        {
            var queryPrompt = _systemValueRepository.Get(sv => sv.Key == "USER_SEARCH_QUERY_PROMPT");
            if (queryPrompt == null)
            {
                _logger.LogError("USER_SEARCH_QUERY_PROMPT system value missing");
                return new AgentResponse("An error has ocurred");
            }

            var resultPromptStartKey = $"USER_SEARCH_RESULT_START_PROMPT_{interfaceType}";
            var resultPromptProfileKey = $"USER_SEARCH_RESULT_PROFILE_PROMPT_{interfaceType}";
            var resultEndKey = $"USER_SEARCH_RESULT_END_{interfaceType}";
            var resultStartPrompt = _systemValueRepository.Get(sv => sv.Key == resultPromptStartKey);
            if (resultStartPrompt == null)
            {
                _logger.LogError("{resultPromptKey} system value missing", resultPromptStartKey);
                return new AgentResponse("An error has ocurred");
            }

            var resultPromptProfile = _systemValueRepository.Get(sv => sv.Key == resultPromptProfileKey);
            if (resultPromptProfile == null)
            {
                _logger.LogError("{resultPromptProfileKey} system value missing", resultPromptProfileKey);
                return new AgentResponse("An error has ocurred");
            }

            var resultEnd = _systemValueRepository.Get(sv => sv.Key == resultEndKey);
            if (resultEnd == null)
            {
                _logger.LogError("{resultEndKey} system value missing", resultEndKey);
                return new AgentResponse("An error has ocurred");
            }

            //get search query
            var exampleResult = new PromptResult()
            {
                Explanation = "explanation",
                ExtractedQuery = "topic or field",
                ExtractedGlobal = true,
                ExtractedConfiguration = "default or current",
                Success = true
            };

            var res = await _aiService.GetResponseAsync<PromptResult>(string.Format(queryPrompt.Value, JsonSerializer.Serialize(exampleResult)), messages, 0);
            if (!res.Success)
                return new AgentResponse(res.Explanation);

            _logger.LogInformation($"Extracted query: {res.ExtractedQuery}");

            //get search results
            var searchResults = new List<SearchResult<User>>();
            if (string.IsNullOrEmpty(nodeId) || res.ExtractedGlobal)
            {
                searchResults = await _searchService.SearchUsersAsync(res.ExtractedQuery, res.ExtractedConfiguration);
            }
            else
            {
                var hubUserIds = _relationService.ListUserIdsForNode(nodeId);
                searchResults = await _searchService.SearchUsersAsync(res.ExtractedQuery, res.ExtractedConfiguration, hubUserIds);
            }

            //load extra paper and resource data
            var papers = new Dictionary<string, List<Data.Paper>>();
            foreach (var searchResult in searchResults)
            {
                var userPapers = _paperService.ListForEntity(searchResult.Document.Id, searchResult.Document.Id, null, 1, int.MaxValue, Data.Util.SortKey.CreatedDate, false, false, false);
                papers.Add(searchResult.Document.Id, userPapers);
            }

            var resources = new Dictionary<string, List<Data.Resource>>();
            foreach (var searchResult in searchResults)
            {
                var userResources = _resourceService.ListForEntity(searchResult.Document.Id, searchResult.Document.Id, null, 1, int.MaxValue, Data.Util.SortKey.CreatedDate, false, false, false);
                resources.Add(searchResult.Document.Id, userResources);
            }

            //get text responses
            var searchResultData = searchResults.Select(u => new
            {
                UserURL = $"<{_configuration["App:URL"]}/user/{u.Document.Id}",
                //Source = hubUsers.Contains(u.Document.Id) ? "Internal" : "External",
                u.Document.Name,
                SearchScore = u.SemanticSearch.RerankerScore,
                OriginalData = u.Document,
                Papers = papers[u.Document.Id].Select(p => new
                {
                    p.Title,
                    p.Journal,
                    p.PublicationDate,
                    //p.Authors
                }),
                Repositories = resources[u.Document.Id].Where(r => r.Type == Data.ResourceType.Repository).Select(r => new
                {
                    r.Title,
                    Abstract = r.Data.Contains("Abstract") &&!r.Data["Abstract"].IsBsonNull ? r.Data["Abstract"].AsString : "",
                    Keywords = r.Data.Contains("Keywords") && !r.Data["Keywords"].IsBsonNull ? r.Data["Keywords"].AsString : "",
                    Language = r.Data.Contains("Language") &&!r.Data["Language"].IsBsonNull? r.Data["Language"].AsString : "",
                }),
                Highlights = u.SemanticSearch.Captions
            }).ToList();

            var startRes = await _aiService.GetResponseAsync(string.Format(resultStartPrompt.Value, res.ExtractedQuery, JsonSerializer.Serialize(searchResultData)), messages, 0.5m, 8192);
            var profileRes = new List<string>();
            foreach (var searchResult in searchResultData)
            {
                var profileMatch = await _aiService.GetResponseAsync(string.Format(resultPromptProfile.Value, res.ExtractedQuery, JsonSerializer.Serialize(searchResult)), messages, 0.5m, 8192);
                profileRes.Add(profileMatch);
            }

            return new AgentResponse { Text = [startRes,..profileRes, resultEnd.Value], Context = JsonSerializer.Serialize(searchResultData), OriginalQuery = res.ExtractedQuery };
        }

        public async Task<AgentResponse> GetFollowupResponseAsync(IEnumerable<InputItem> messages, string context, string originalQuery, string? interfaceType = default)
        {
            var promptKey = $"USER_SEARCH_FOLLOWUP_PROMPT_{interfaceType}";
            var prompt = _systemValueRepository.Get(sv => sv.Key == promptKey);
            if (prompt == null)
            {
                _logger.LogError("{promptKey} system value missing", promptKey);
                return new AgentResponse("An error has ocurred");
            }

            if (messages.Last()?.Text == "*qwertyuiopqwertyuiop*")
                return new AgentResponse(originalQuery);

            var promptText = string.Format(prompt.Value, context);
            var response = await _aiService.GetResponseAsync(promptText, messages, 0.5m, 8192);
            return new AgentResponse(response);
        }
    }
}
