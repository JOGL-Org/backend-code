using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Data;
using GenerativeAI;
using GenerativeAI.Types;
using System;

namespace Jogl.Server.AI
{
    public class GeminiAIService : IAIService
    {
        private readonly IConfiguration _configuration;
        public GeminiAIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        //public async Task<string> ExplainSearchResultAsync(string query, object searchResult)
        //{
        //    var client = new AnthropicClient(_configuration["Claude:APIKey"]);

        //    var prompts = new List<SystemMessage>
        //    {
        //        new SystemMessage($"The following is a json of a search result from Azure AI Search. Using the typical chatGTP style for answers (using a small intro, bullet points for the main content and bold font for important expressions), explain why the search result matches the original query {query}. If you think the match is weak, do not say that explicitly. Do not mention the original search query. Your response will be embedded in a front end for searching profiles, the end user has no notion of which data you are referencing and we need any explanation to quote the source text snippet")
        //    };

        //    var parameters = new MessageParameters()
        //    {
        //        Messages = new List<Message> { new Message(RoleType.User, JsonSerializer.Serialize(searchResult)) },
        //        Model = AnthropicModels.Claude35Sonnet,
        //        Stream = false,
        //        MaxTokens = 1024,
        //        Temperature = 0.5m,
        //        System = prompts
        //    };

        //    var response = await client.Messages.GetClaudeMessageAsync(parameters);
        //    if (response?.Message == null)
        //        throw new Exception($"Claude request failed for search result response prompt");

        //    return response.Message.ToString();
        //}

        public async Task<string> GetResponseAsync(IEnumerable<string> contextData, IEnumerable<InputItem> userInputs)
        {
            var client = new GoogleAi(_configuration["Gemini:APIKey"]);

            var prompts = new List<Part>();
            prompts.Add(new Part($"The following is a concatenation of paper abstracts pertinent to a discussion:"));
            prompts.AddRange(contextData.Where(abs => !string.IsNullOrEmpty(abs)).Select(abs => new Part(abs)));
            prompts.Add(new Part($"Use the above as context. Keep your answers limited to a paragraph or so."));

            var googleModel = client.CreateGenerativeModel("models/gemini-2.0-flash-thinking-exp-1219");
            var response = await googleModel.GenerateContentAsync(new GenerativeAI.Types.GenerateContentRequest
            {
                SystemInstruction = new Content { Parts = prompts },
                Contents = userInputs.Select(i => new Content { Role = i.FromUser ? Roles.User : Roles.Model, Parts = [new Part(i.Text)] }).ToList()
            });

            if (response.Candidates.FirstOrDefault().Content.Parts.FirstOrDefault().Text == null)
                throw new Exception($"Claude request failed for feed prompt");

            return response.Candidates.FirstOrDefault().Content.Parts.FirstOrDefault().Text;
        }

        public async Task<string> GetResponseAsync(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m, int maxTokens = 1024)
        {
            var client = new GoogleAi(_configuration["Gemini:APIKey"]);

            var googleModel = client.CreateGenerativeModel("models/gemini-2.0-flash-thinking-exp-1219");
            var response = await googleModel.GenerateContentAsync(new GenerativeAI.Types.GenerateContentRequest
            {
                SystemInstruction = new Content { Parts = [new Part(prompt)] },
                Contents = inputHistory.Select(i => new Content { Role = i.FromUser ? Roles.User : Roles.Model, Parts = [new Part(i.Text)] }).ToList(),
            });

            if (string.IsNullOrEmpty(response.Text))
                throw new Exception($"Gemini request failed");

            return response.Text();
        }

        public async Task<T> GetResponseAsync<T>(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m, int maxTokens = 1024)
        {
            var client = new GoogleAi(_configuration["Gemini:APIKey"]);

            var googleModel = client.CreateGenerativeModel("models/gemini-2.0-flash-thinking-exp-1219");
            var response = await googleModel.GenerateContentAsync(new GenerativeAI.Types.GenerateContentRequest
            {
                SystemInstruction = new Content { Parts = [new Part(prompt)] },
                Contents = inputHistory.Select(i => new Content { Role = i.FromUser ? Roles.User : Roles.Model, Parts = [new Part(i.Text)] }).ToList()
            });

            if (response.ToString() == null)
                throw new Exception($"Claude request failed for feed prompt");

            var responseText = response.ToString().Replace("```", string.Empty).Replace("json", string.Empty);
            return JsonSerializer.Deserialize<T>(responseText);
        }

        public async Task<decimal> GetBotScoreAsync<T>(T payload) where T : Entity
        {
            return 0;
        }
    }
}
