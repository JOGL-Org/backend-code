using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Anthropic.SDK;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Data;
using Jogl.Server.AI.DTO;

namespace Jogl.Server.AI
{
    public class ClaudeAIService : IAIService
    {
        private readonly IConfiguration _configuration;
        public ClaudeAIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<PromptResult> GetSearchQueryAsync(string query)
        {
            var client = new AnthropicClient(_configuration["Claude:APIKey"]);

            var prompts = new List<SystemMessage>
            {
                new SystemMessage($"The following message will be a natural language query. The user is searching for other users. We need to extract a search query representing a topic or area of expertise to feed into Azure AI Search. Respond with a json of the following format: {{\"success\": boolean, \"extractedQuery\": string, \"explanation\": string}}. ")
            };

            var parameters = new MessageParameters()
            {
                Messages = new List<Message> { new Message(RoleType.User, query) },
                Model = AnthropicModels.Claude35Sonnet,
                Stream = false,
                MaxTokens = 1024,
                Temperature = 0.5m,
                System = prompts
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters);
            if (response?.Message == null)
                throw new Exception($"Claude request failed for search query prompt");

            return JsonSerializer.Deserialize<PromptResult>(response.Message);
        }

        public async Task<string> ExplainSearchResultAsync(string query, object searchResult)
        {
            var client = new AnthropicClient(_configuration["Claude:APIKey"]);

            var prompts = new List<SystemMessage>
            {
                new SystemMessage($"The following is a json of a search result from Azure AI Search. Using the typical chatGTP style for answers (using a small intro, bullet points for the main content and bold font for important expressions), explain why the search result matches the original query {query}. If you think the match is weak, do not say that explicitly. Do not mention the original search query. Your response will be embedded in a front end for searching profiles, the end user has no notion of which data you are referencing and we need any explanation to quote the source text snippet")
            };

            var parameters = new MessageParameters()
            {
                Messages = new List<Message> { new Message(RoleType.User, JsonSerializer.Serialize(searchResult)) },
                Model = AnthropicModels.Claude35Sonnet,
                Stream = false,
                MaxTokens = 1024,
                Temperature = 0.5m,
                System = prompts
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters);
            if (response?.Message == null)
                throw new Exception($"Claude request failed for search result response prompt");

            return response.Message.ToString();
        }

        public async Task<string> GetResponseAsync(IEnumerable<string> contextData, IEnumerable<InputItem> userInputs)
        {
            var client = new AnthropicClient(_configuration["Claude:APIKey"]);

            var prompts = new List<SystemMessage>();
            prompts.Add(new SystemMessage($"The following is a concatenation of paper abstracts pertinent to a discussion:"));
            prompts.AddRange(contextData.Where(abs => !string.IsNullOrEmpty(abs)).Select(abs => new SystemMessage(abs)));
            prompts.Add(new SystemMessage($"Use the above as context. Keep your answers limited to a paragraph or so."));

            var parameters = new MessageParameters()
            {
                Messages = userInputs.Select(i => new Message(i.FromUser ? RoleType.User : RoleType.Assistant, i.Text)).ToList(),
                Model = AnthropicModels.Claude35Sonnet,
                Stream = false,
                MaxTokens = 1024,
                Temperature = 0.5m,
                PromptCaching = PromptCacheType.Messages,
                System = prompts
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters);
            if (response?.Message == null)
                throw new Exception($"Claude request failed for feed prompt");

            return response.Message.ToString();
        }

        public async Task<string> GetResponseAsync(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m)
        {
            var client = new AnthropicClient(_configuration["Claude:APIKey"]);

            var prompts = new List<SystemMessage>();
            prompts.Add(new SystemMessage(prompt));

            var parameters = new MessageParameters()
            {
                Messages = inputHistory.Select(i => new Message(i.FromUser ? RoleType.User : RoleType.Assistant, i.Text)).ToList(),
                Model = AnthropicModels.Claude35Sonnet,
                Stream = false,
                MaxTokens = 1024,
                Temperature = temperature,
                PromptCaching = PromptCacheType.Messages,
                System = prompts,
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters);
            if (response?.Message == null)
                throw new Exception($"Claude request failed for feed prompt");

            return response.Message.ToString();
        }

        public async Task<T> GetResponseAsync<T>(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m)
        {
            var client = new AnthropicClient(_configuration["Claude:APIKey"]);

            var prompts = new List<SystemMessage>();
            prompts.Add(new SystemMessage(prompt));

            var parameters = new MessageParameters()
            {
                Messages = inputHistory.Select(i => new Message(i.FromUser ? RoleType.User : RoleType.Assistant, i.Text)).ToList(),
                Model = AnthropicModels.Claude35Sonnet,
                Stream = false,
                MaxTokens = 1024,
                Temperature = temperature,
                PromptCaching = PromptCacheType.Messages,
                System = prompts
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters);
            if (response?.Message == null)
                throw new Exception($"Claude request failed for feed prompt");

            return JsonSerializer.Deserialize<T>(response.Message.ToString());
        }

        public async Task<decimal> GetBotScoreAsync<T>(T payload) where T : Entity
        {
            var client = new AnthropicClient(_configuration["Claude:APIKey"]);
            var json = JsonSerializer.Serialize(payload);
            var parameters = new MessageParameters()
            {
                Messages = new List<Message>()
                {
                    new Message(RoleType.User, json),
                },
                Model = AnthropicModels.Claude35Sonnet,
                Stream = false,
                MaxTokens = 1024,
                Temperature = 0,
                System = new List<SystemMessage>
                {
                    new SystemMessage( $"The following is a json of a {typeof(T).Name}"),
                    new SystemMessage( $"What is the probability that this is a bot, in percent?"),
                    new SystemMessage( $"Respond with number only"),
                },
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters);
            if (response?.Message == null)
                throw new Exception($"Claude request failed for {typeof(T).Name} ID {payload.Id.ToString()} ");

            decimal res;
            if (!decimal.TryParse(response.Message.ToString(), out res))
                throw new Exception($"Claude responded with something other than a number: {response.Message.ToString()}");

            return res;
        }
    }
}
