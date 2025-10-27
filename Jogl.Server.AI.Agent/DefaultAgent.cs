using Azure.Search.Documents.Models;
using Jogl.Server.AI.Agent.DTO;
using Jogl.Server.Business;
using Jogl.Server.DB;
using Jogl.Server.Search.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Jogl.Server.AI.Agent
{
    public class DefaultAgent : IAgent
    {
        private readonly IAIService _aiService;
        private readonly Search.ISearchService _searchService;
        private readonly IRelationService _relationService;
        private readonly IPaperService _paperService;
        private readonly IResourceService _resourceService;
        private readonly ISystemValueRepository _systemValueRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DefaultAgent> _logger;

        public DefaultAgent(IAIService aIService, Search.ISearchService searchService, IRelationService relationService, IPaperService paperService, IResourceService resourceService, ISystemValueRepository systemValueRepository, IConfiguration configuration, ILogger<DefaultAgent> logger)
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

        public async Task<AgentResponse> GetInitialResponseAsync(string message, string nodeId, string? interfaceType = default)
        {
            var queryPrompt = _systemValueRepository.Get(sv => sv.Key == "USER_SEARCH_QUERY_PROMPT");
            if (queryPrompt == null)
            {
                _logger.LogError("USER_SEARCH_QUERY_PROMPT system value missing");
                return new AgentResponse("An error has ocurred");
            }

            var resultPromptStartKey = $"USER_SEARCH_RESULT_START_PROMPT_{interfaceType}";
            var resultPromptProfileKey = $"USER_SEARCH_RESULT_PROFILE_PROMPT_{interfaceType}";
            var resultPromptEndKey = $"USER_SEARCH_RESULT_END_PROMPT_{interfaceType}";
            var resultStartPrompt = _systemValueRepository.Get(sv => sv.Key == resultPromptStartKey);
            if (resultStartPrompt == null)
            {
                _logger.LogError("{resultPromptKey} system value missing", resultPromptStartKey);
                return new AgentResponse("An error has ocurred");
            }

            var resultProfilePrompt = _systemValueRepository.Get(sv => sv.Key == resultPromptProfileKey);
            if (resultProfilePrompt == null)
            {
                _logger.LogError("{resultPromptProfileKey} system value missing", resultPromptProfileKey);
                return new AgentResponse("An error has ocurred");
            }

            var resultEndPrompt = _systemValueRepository.Get(sv => sv.Key == resultPromptEndKey);
            if (resultEndPrompt == null)
            {
                _logger.LogError("{resultPromptEndKey} system value missing", resultPromptEndKey);
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

            List<InputItem> messages = [new InputItem { FromUser = true, Text = message }];

            var res = await _aiService.GetResponseAsync<PromptResult>(string.Format(queryPrompt.Value, JsonSerializer.Serialize(exampleResult)), messages, 0);
            if (!res.Success)
                return new AgentResponse(res.Explanation);

            _logger.LogInformation($"Extracted query: {res.ExtractedQuery}");

            //get search results
            var hubUserIds = _relationService.ListUserIdsForNode(nodeId);
            var searchResults = await _searchService.SearchUsersAsync(res.ExtractedQuery, res.ExtractedConfiguration, hubUserIds);

            ////load extra paper and resource data
            //var papers = new Dictionary<string, List<Data.Paper>>();
            //foreach (var searchResult in searchResults)
            //{
            //    var userPapers = _paperService.ListForEntity(searchResult.Document.Id, searchResult.Document.Id, null, 1, int.MaxValue, Data.Util.SortKey.CreatedDate, false, false, false);
            //    papers.Add(searchResult.Document.Id, userPapers);
            //}

            //var resources = new Dictionary<string, List<Data.Resource>>();
            //foreach (var searchResult in searchResults)
            //{
            //    var userResources = _resourceService.ListForEntity(searchResult.Document.Id, searchResult.Document.Id, null, 1, int.MaxValue, Data.Util.SortKey.CreatedDate, false, false, false);
            //    resources.Add(searchResult.Document.Id, userResources);
            //}

            //get text responses
            var searchResultData = searchResults.Select(u => new
            {
                URL = $"{_configuration["App:URL"]}/user/{u.Document.Id}",
                u.Document.Id,
                u.Document.Name,
                u.Document.Bio,
                u.Document.ShortBio,
                u.Document.Current,
                u.Document.Current_Companies,
                u.Document.Current_Roles,
                //u.Document.Repositories_Title,
                //u.Document.Repositories_Keywords,
                //u.Document.Repositories_Languages,
                //u.Document.Projects_Title,
                u.Document.Links,
                //u.Document.Explanation,
                SearchScore = u.SemanticSearch.RerankerScore,
                Highlights = u.SemanticSearch.Captions
            }).ToList();

            //var startRes = await _aiService.GetResponseAsync(string.Format(resultStartPrompt.Value, res.ExtractedQuery, JsonSerializer.Serialize(searchResultData)), messages, 0.5m, 8192);
            //messages.Add(new InputItem { FromUser = false, Text = startRes });

            var explanations = await _aiService.GetResponseAsync(new List<UserExplanation> { new UserExplanation { Id = "68d1c356053c99369231d683", Explanation = "Professor of microbiology at Sorbonne" }, new UserExplanation { Id = "68d1c492053c99369231f1c3", Explanation = "Has published extensively about synthetic microbiology" } }, string.Format(resultProfilePrompt.Value, res.ExtractedQuery), [new InputItem { FromUser = true, Text = JsonSerializer.Serialize(searchResultData) }], 0m);
            var profileRes = BuildSearchResponse(searchResults, explanations);
            messages.Add(new InputItem { FromUser = false, Text = profileRes });

            var endRes = await _aiService.GetResponseAsync(string.Format(resultEndPrompt.Value, res.ExtractedQuery, JsonSerializer.Serialize(searchResultData)), messages, 0.5m, 8192);

            return new AgentResponse { Text = [/*startRes,*/ profileRes, endRes], Context = JsonSerializer.Serialize(searchResultData), OriginalQuery = res.ExtractedQuery };
        }

        private string BuildSearchResponse(List<SearchResult<User>> users, List<UserExplanation> explanations)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<ul>");
            foreach (var user in users)
            {
                var explanation = explanations.SingleOrDefault(e => e.Id == user.Document.Id);
                if (string.IsNullOrEmpty(explanation?.Explanation))
                {
                    continue;
                }

                sb.AppendLine("<li><div>");
                sb.Append("<a href=\"");
                sb.Append($"{_configuration["App:URL"]}/user/{user.Document.Id}");
                sb.Append("\"");
                foreach (var link in user.Document.Links)
                {
                    sb.Append($" data-{link.Type}=");
                    sb.Append($"\"{link.URL}\"");
                }
                sb.Append("><strong>");
                sb.Append(user.Document.Name);
                sb.Append("</strong></a></div>");
                sb.Append(explanation.Explanation);
                sb.AppendLine("</li>");
            }

            sb.AppendLine("</ul>");

            return sb.ToString();
        }

        private class UserExplanation
        {
            public string Id { get; set; }
            public string Explanation { get; set; }
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

            var promptText = string.Format(prompt.Value, context, originalQuery);
            var response = await _aiService.GetResponseAsync(promptText, messages, 0.5m, 8192);
            return new AgentResponse(response);
        }

        public async Task<AgentConversationResponse> GetOnboardingResponseAsync(IEnumerable<InputItem> messages)
        {
            var stopPhrase = "Alright, let’s move on then";

            var conversationPromptKey = $"ONBOARDING_CONVERSATION_PROMPT";
            var conversationPrompt = _systemValueRepository.Get(sv => sv.Key == conversationPromptKey);
            if (conversationPrompt == null)
            {
                _logger.LogError("{promptKey} system value missing", conversationPromptKey);
                return new AgentConversationResponse("An error has ocurred", false);
            }

            var conversationPromptText = string.Format(conversationPrompt.Value, stopPhrase);
            var response = await _aiService.GetResponseAsync(conversationPromptText, messages, 0.5m, 8192);

            if (!response.Contains(stopPhrase))
            {
                return new AgentConversationResponse(response, false);
            }

            var outputPromptKey = $"ONBOARDING_OUTPUT_PROMPT";
            var outputPrompt = _systemValueRepository.Get(sv => sv.Key == outputPromptKey);
            if (outputPrompt == null)
            {
                _logger.LogError("{promptKey} system value missing", outputPromptKey);
                return new AgentConversationResponse("An error has ocurred", false);
            }

            var outputPromptText = outputPrompt.Value;
            var outputResponse = await _aiService.GetResponseAsync(outputPromptText, messages, 0.5m, 8192);

            return new AgentConversationResponse(response, true, outputResponse);
        }

        public async Task<AgentResponse> GetFirstSearchResponseAsync(string current)
        {
            var promptKey = $"FIRST_SEARCH_PROMPT";
            var prompt = _systemValueRepository.Get(sv => sv.Key == promptKey);
            if (prompt == null)
            {
                _logger.LogError("{promptKey} system value missing", promptKey);
                return new AgentResponse("An error has ocurred");
            }

            var response = await _aiService.GetResponseAsync(prompt.Value, [new InputItem { FromUser = true, Text = current }], 0.5m, 8192);
            return new AgentResponse(response);
        }

        public async Task<AgentResponse> GetProfileResponseAsync(IEnumerable<InputItem> messages, Data.User user)
        {
            var promptKey = $"USER_OWN_PROFILE_PROMPT";
            var prompt = _systemValueRepository.Get(sv => sv.Key == promptKey);
            if (prompt == null)
            {
                _logger.LogError("{promptKey} system value missing", promptKey);
                return new AgentResponse("An error has ocurred");
            }

            var promptText = string.Format(prompt.Value, JsonSerializer.Serialize(user));
            var response = await _aiService.GetResponseAsync(promptText, messages, 0.5m, 8192);
            return new AgentResponse(response);
        }

        public async Task<string> GetChannelTitleAsync(string message)
        {
            var response = await _aiService.GetResponseAsync("Summarize the topic of this query or question in a few words. MAXIMUM six words.", [new InputItem { FromUser = true, Text = message }], 0.5m, 8192);
            return response.Trim('.');
        }
    }
}
