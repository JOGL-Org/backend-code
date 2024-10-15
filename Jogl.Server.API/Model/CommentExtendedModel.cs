using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CommentExtendedModel : CommentModel
    {
        [JsonPropertyName("content_entity")]
        public ContentEntityModel? ContentEntity { get; set; }
    }
}