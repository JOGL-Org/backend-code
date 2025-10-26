using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Data;
using GenerativeAI;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.AI
{
    public class GeminiAIService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiAIService> _logger;

        public GeminiAIService(IConfiguration configuration, ILogger<GeminiAIService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetResponseAsync(IEnumerable<string> contextData, IEnumerable<InputItem> userInputs)
        {
            return string.Empty;
        }

        public async Task<string> GetResponseAsync(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m, int maxTokens = 102400)
        {
            var client = new GoogleAi(_configuration["Gemini:APIKey"]);

            var googleModel = client.CreateGenerativeModel("models/gemini-2.5-flash");
            var response = await googleModel.GenerateContentAsync(new GenerateContentRequest
            {
                SystemInstruction = new Content { Parts = [new Part(prompt)] },
                Contents = inputHistory.Select(i => new Content { Role = i.FromUser ? Roles.User : Roles.Model, Parts = [new Part(i.Text)] }).ToList(),
            });

            if (string.IsNullOrEmpty(response.Text))
                throw new Exception($"Gemini request failed");

            return response.Text().Trim();
        }

        public async Task<string> GetResponseAsync(string prompt, IEnumerable<InputItem> inputHistory, IEnumerable<string> allowedValues, decimal? temperature = 0.5m, int maxTokens = 102400)
        {
            var client = new GoogleAi(_configuration["Gemini:APIKey"]);
            var googleModel = client.CreateGenerativeModel("models/gemini-2.5-flash");

            var allowedValuesArray = allowedValues.ToArray();
            var schema = new Schema
            {
                Type = SchemaType.STRING.ToString(),
                Enum = allowedValuesArray.ToList()
            };

            var request = new GenerateContentRequest
            {
                SystemInstruction = new Content { Parts = [new Part(prompt)] },
                Contents = inputHistory.Select(i => new Content { Role = i.FromUser ? Roles.User : Roles.Model, Parts = [new Part(i.Text)] }).ToList(),
                GenerationConfig = new GenerationConfig
                {
                    Temperature = (float?)temperature,
                    MaxOutputTokens = maxTokens,
                    ResponseSchema = schema
                }
            };

            var response = await googleModel.GenerateContentAsync(request);

            if (string.IsNullOrEmpty(response.Text))
                throw new Exception($"Gemini request failed");

            return response.Text().Trim();
        }

        public async Task<T> GetResponseAsync<T>(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m, int maxTokens = 102400)
        {
            var client = new GoogleAi(_configuration["Gemini:APIKey"]);

            var googleModel = client.CreateGenerativeModel("models/gemini-2.5-flash");
            var response = await googleModel.GenerateContentAsync(new GenerativeAI.Types.GenerateContentRequest
            {
                SystemInstruction = new Content { Parts = [new Part(prompt)] },
                Contents = inputHistory.Select(i => new Content { Role = i.FromUser ? Roles.User : Roles.Model, Parts = [new Part(i.Text)] }).ToList(),
                GenerationConfig = new GenerationConfig
                {
                    Temperature = (float?)temperature,
                    MaxOutputTokens = maxTokens,
                }
            });

            if (response.ToString() == null)
                throw new Exception($"Claude request failed for feed prompt");

            var responseText = response.ToString().Replace("```", string.Empty).Replace("json", string.Empty);
            try
            {
                return JsonSerializer.Deserialize<T>(responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserialize response: {response}", responseText);
                throw;
            }
        }

        public async Task<T> GetResponseAsync<T>(T sample, string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m, int maxTokens = 102400)
        {
            var client = new GoogleAi(_configuration["Gemini:APIKey"]);

            var googleModel = client.CreateGenerativeModel("models/gemini-2.5-flash");
            var response = await googleModel.GenerateContentAsync(new GenerativeAI.Types.GenerateContentRequest
            {
                SystemInstruction = new Content { Parts = [new Part(prompt), new Part($"CRITICAL: Return a JSON like this ```json{JsonSerializer.Serialize(sample)}```, NEVER return anything else than this json.")] },
                Contents = inputHistory.Select(i => new Content { Role = i.FromUser ? Roles.User : Roles.Model, Parts = [new Part(i.Text)] }).ToList(),
                GenerationConfig = new GenerationConfig
                {
                    Temperature = (float?)temperature,
                    MaxOutputTokens = maxTokens,
                }
            });

            if (response.ToString() == null)
                throw new Exception($"Claude request failed for feed prompt");

            var responseText = response.ToString().Replace("```", string.Empty).Replace("json", string.Empty);
            try
            {
                return JsonSerializer.Deserialize<T>(responseText);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserialize response: {response}", responseText);
                throw;
            }
        }

        public async Task<decimal> GetBotScoreAsync<T>(T payload) where T : Entity
        {
            return 0;
        }
    }
}
