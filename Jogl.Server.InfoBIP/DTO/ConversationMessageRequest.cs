using System.Text.Json.Serialization;

namespace Jogl.Server.InfoBIP.DTO
{
    public class ConversationMessageRequest
    {
        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("content")]
        public MessageContent Content { get; set; }
    }

    public class MessageContent
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("mediaUrl")]
        public string MediaUrl { get; set; }
        
        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        //[JsonPropertyName("templateName")]
        //public string TemplateName { get; set; }

        //[JsonPropertyName("templateData")]
        //public TemplateData TemplateData { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }
    }
}
