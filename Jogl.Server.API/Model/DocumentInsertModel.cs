using Jogl.Server.API.Validators;
using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    [DocumentValidation]
    public class DocumentInsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string Name { get; set; }

        [JsonPropertyName("file_name")]
        public string? Filename { get; set; }

        [JsonPropertyName("url")]
        public string? URL { get; set; }

        [JsonPropertyName("type")]
        public DocumentType Type { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("user_ids")]
        public List<string>? UserIds { get; set; }

        [JsonPropertyName("status")]
        public ContentEntityStatus? Status { get; set; }

        [JsonPropertyName("default_visibility")]
        public FeedEntityVisibility? DefaultVisibility { get; set; }

        [JsonPropertyName("user_visibility")]
        public List<FeedEntityUserVisibilityUpsertModel>? UserVisibility { get; set; }

        [JsonPropertyName("communityentity_visibility")]
        public List<FeedEntityCommunityEntityVisibilityUpsertModel>? CommunityEntityVisibility { get; set; }

        [JsonPropertyName("image_id")]
        public string? ImageId { get; set; }

        [JsonPropertyName("folder_id")]
        public string? FolderId { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }
    }
}