using System.Text.Json.Serialization;

namespace Jogl.Server.AI.DTO
{
    public class PromptResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("extractedQuery")]
        public string ExtractedQuery { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }
    }
}
