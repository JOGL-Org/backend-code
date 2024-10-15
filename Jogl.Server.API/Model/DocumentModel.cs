using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class DocumentModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Name { get; set; }

        [JsonPropertyName("file_name")]
        public string Filename { get; set; }

        [JsonPropertyName("file_type")]
        public string Filetype { get; set; }

        [JsonPropertyName("file_size_bytes")]
        public int FileSize { get; set; }

        [JsonPropertyName("url")]
        public string URL { get; set; }

        [JsonPropertyName("type")]
        public DocumentType Type { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("document_url")]
        public string DocumentUrl { get; set; }

        [JsonPropertyName("image_url_sm")]
        public string ImageUrlSmall { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }

        [JsonPropertyName("feed_entity")]
        public EntityMiniModel? FeedEntity { get; set; }

        [JsonPropertyName("users")]
        public List<UserMiniModel> Users { get; set; }

        [JsonPropertyName("feed_stats")]
        public FeedStatModel FeedStats { get; set; }

        [JsonPropertyName("status")]
        public ContentEntityStatus Status { get; set; }

        [Obsolete]
        [JsonPropertyName("visibility")]
        public ContentEntityVisibility Visibility { get; set; }

        [JsonPropertyName("default_visibility")]
        public FeedEntityVisibility? DefaultVisibility { get; set; }

        [JsonPropertyName("user_visibility")]
        public List<FeedEntityUserVisibilityModel>? UserVisibility { get; set; }

        [JsonPropertyName("communityentity_visibility")]
        public List<FeedEntityCommunityEntityVisibilityModel>? CommunityEntityVisibility { get; set; }

        [JsonPropertyName("image_id")]
        public string ImageId { get; set; }

        [JsonPropertyName("folder_id")]
        public string FolderId { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }

        [JsonPropertyName("is_folder")]
        public bool IsFolder => false;

        [JsonPropertyName("is_new")]
        public bool IsNew { get; set; }

        [JsonPropertyName("is_media")]
        public bool IsMedia { get; set; }

        [JsonPropertyName("permissions")]
        public List<Permission> Permissions { get; set; }

        [JsonPropertyName("path")]
        public List<EntityMiniModel> Path { get; set; }
    }
}