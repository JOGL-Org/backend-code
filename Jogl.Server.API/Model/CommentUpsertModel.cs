using Jogl.Server.API.Validators;
using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CommentUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("reply_to_id")]
        public string? ReplyToId { get; set; }

        [JsonPropertyName("documents_to_add")]
        [DocumentValidation(DocumentType.Document)]
        public List<DocumentInsertModel>? DocumentsToAdd { get; set; }

        [JsonPropertyName("documents_to_delete")]
        public List<string>? DocumentsToDelete { get; set; }
    }
}