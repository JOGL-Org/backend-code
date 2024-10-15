using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Anthropic.SDK;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Data;

namespace Jogl.Server.AI
{
    public class ClaudeAIService : IAIService
    {
        private readonly IConfiguration _configuration;
        public ClaudeAIService(IConfiguration configuration)
        {
            _configuration = configuration;
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
