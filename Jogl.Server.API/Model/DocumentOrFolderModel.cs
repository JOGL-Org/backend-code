using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class DocumentOrFolderModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string? URL { get; set; }

        [JsonPropertyName("type")]
        public DocumentType? Type { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }

        [JsonPropertyName("feed_stats")]
        public FeedStatModel? FeedStats { get; set; }

        [JsonPropertyName("feed_entity")]
        public EntityMiniModel? FeedEntity { get; set; }

        [JsonPropertyName("image_id")]
        public string? ImageId { get; set; }

        [JsonPropertyName("folder_id")]
        public string? FolderId { get; set; }

        [JsonPropertyName("is_folder")]
        public bool IsFolder { get; set; }
        
        [JsonPropertyName("is_new")]
        public bool IsNew { get; set; }

        [JsonPropertyName("parent_folder_id")]
        public string? ParentFolderId { get; set; }
    }
}