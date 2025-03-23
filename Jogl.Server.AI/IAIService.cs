using Jogl.Server.Data;

namespace Jogl.Server.AI
{
    public interface IAIService
    {
        Task<PromptResult> GetSearchQueryAsync(string query);
        Task<string> ExplainSearchResultAsync(string query, object searchResult);
        Task<string> GetResponseAsync(IEnumerable<string> contextData, IEnumerable<InputItem> inputHistory);
        Task<decimal> GetBotScoreAsync<T>(T payload) where T : Entity;
    }

    public class InputItem
    {
        public bool FromUser { get; set; }
        public string Text { get; set; }
    }
}
