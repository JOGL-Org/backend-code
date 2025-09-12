using Jogl.Server.AI.DTO;
using Jogl.Server.Data;

namespace Jogl.Server.AI
{
    public interface IAIService
    {
        //Task<PromptResult> GetSearchQueryAsync(string query);
        //Task<string> ExplainSearchResultAsync(string query, object searchResult);
        Task<string> GetResponseAsync(IEnumerable<string> contextData, IEnumerable<InputItem> inputHistory);
        Task<string> GetResponseAsync(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m, int maxTokens = 102400);
        Task<string> GetResponseAsync(string prompt, IEnumerable<InputItem> inputHistory, IEnumerable<string> allowedValues, decimal? temperature = 0.5m, int maxTokens = 102400);
        Task<T> GetResponseAsync<T>(string prompt, IEnumerable<InputItem> inputHistory, decimal? temperature = 0.5m, int maxTokens = 102400);
        Task<decimal> GetBotScoreAsync<T>(T payload) where T : Entity;
    }

    public class InputItem
    {
        public bool FromUser { get; set; }
        public string Text { get; set; }
    }
}
