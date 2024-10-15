using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FolderUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("parent_folder_id")]
        public string? ParentFolderId { get; set; }
    }
}