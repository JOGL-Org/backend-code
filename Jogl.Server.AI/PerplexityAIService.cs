using System.Text.Json;
using Jogl.Server.AI.DTO;
using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Jogl.Server.AI
{
    public class PerplexityAIService : IAIService
    {
        private readonly IConfiguration _configuration;
        public PerplexityAIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> ExplainSearchResultAsync(string query, object searchResult)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetBotScoreAsync<T>(T payload) where T : Entity
        {
            throw new NotImplementedException();
        }

        public Task<string> GetResponseAsync(IEnumerable<string> contextData, IEnumerable<InputItem> inputHistory)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetResponseAsync(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m, int maxTokens = 1024)
        {
            var restClient = new RestClient("https://api.perplexity.ai/");
            restClient.AddDefaultHeader("Authorization", "Bearer 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");
            var restRequest = new RestRequest("chat/completions", method: Method.Post);

            restRequest.AddBody(new
            {
                model = "sonar",
                // model = "sonar-medium-online", // Try a different model
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a helpful AI assistant with web search capability. Use search to provide accurate and up-to-date information when needed."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                max_tokens = 10000,
                temperature = 0.2m,
                top_p = 0.9,
                stream = false,
                web_search_options = new
                {
                    search_context_size = "low"
                }
            });

            var response = await restClient.ExecuteAsync<PerplexityResponse>(restRequest);
            if (response?.Data == null)
                throw new Exception($"Perplexity request failed for feed prompt");

            var resultText = response.Data.Choices[0].Message.Content;
            return resultText;
        }

        public async Task<T> GetResponseAsync<T>(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m, int maxTokens = 1024)
        {
            var restClient = new RestClient("https://api.perplexity.ai/");
            restClient.AddDefaultHeader("Authorization", "Bearer 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");
            var restRequest = new RestRequest("chat/completions", method: Method.Post);

            restRequest.AddBody(new
            {
                model = "sonar",
                messages = new[]
                {
                    //new
                    //{
                    //    role = "system",
                    //    content = "You are a helpful assistant that provides accurate and informative responses based on available information."
                    //},
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                max_tokens = 1000,
                temperature,
                top_p = 0.9,
                stream = false
            });

            var response = await restClient.ExecuteAsync<PerplexityResponse>(restRequest);
            if (response?.Data == null)
                throw new Exception($"Perplexity request failed for feed prompt");

            var resultText = response.Data.Choices[0].Message.Content;
            return JsonSerializer.Deserialize<T>(resultText);
        }

        public async Task<T> GetResponseAsync<T>(string prompt, T sampleObject, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5M)
        {
            var restClient = new RestClient("https://api.perplexity.ai/");
            restClient.AddDefaultHeader("Authorization", "Bearer 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");
            var restRequest = new RestRequest("chat/completions", method: Method.Post);

            restRequest.AddBody(new
            {
                model = "sonar",
                messages = new[]
                {
                    //new
                    //{
                    //    role = "system",
                    //    content = "You are a helpful assistant that provides accurate and informative responses based on available information."
                    //},
                    new
                    {
                        role = "user",
                        content = $"{prompt}. Return ONLY a JSON of the following structure: {JsonSerializer.Serialize(sampleObject)}, no other text. If no information is available to you, return an empty json array."
                    }
                },
                max_tokens = 1000,
                temperature,
                top_p = 0.9,
                stream = false
            });

            var response = await restClient.ExecuteAsync<PerplexityResponse>(restRequest);
            if (response?.Data == null)
                throw new Exception($"Perplexity request failed for feed prompt");

            var resultText = response.Data.Choices[0].Message.Content.Replace("```json", string.Empty).Replace("```", string.Empty);
            return JsonSerializer.Deserialize<T>(resultText);
        }

        public Task<PromptResult> GetSearchQueryAsync(string query)
        {
            throw new NotImplementedException();
        }
    }
}
