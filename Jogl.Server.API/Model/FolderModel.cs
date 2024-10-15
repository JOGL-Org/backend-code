using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FolderModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("parent_folder_id")]
        public string ParentFolderId { get; set; }

        [JsonPropertyName("is_folder")]
        public bool IsFolder => true;
    }
}