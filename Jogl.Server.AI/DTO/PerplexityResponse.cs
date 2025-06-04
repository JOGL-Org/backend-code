using System.Text.Json.Serialization;

namespace Jogl.Server.AI.DTO
{
    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }

        [JsonPropertyName("delta")]
        public Delta Delta { get; set; }
    }

    public class Delta
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class PerplexityResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created")]
        public int Created { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }

        [JsonPropertyName("citations")]
        public List<string> Citations { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonPropertyName("search_context_size")]
        public string SearchContextSize { get; set; }
    }


}
