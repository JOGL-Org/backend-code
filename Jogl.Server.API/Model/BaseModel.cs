using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    [JsonDerivedType(typeof(DocumentModel))]
    [JsonDerivedType(typeof(FolderModel))]
    public class BaseModel
    {
        [JsonPropertyName("created")]
        public DateTime CreatedUTC { get; set; }

        [JsonPropertyName("created_by_user_id")]
        public string CreatedByUserId { get; set; }

        [JsonPropertyName("updated")]
        public DateTime? UpdatedUTC { get; set; }

        [JsonPropertyName("last_activity")]
        public DateTime? LastActivityUTC { get; set; }
    }
}