using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ReactionExtendedModel : ReactionModel
    {
        [JsonPropertyName("content_entity")]
        public ContentEntityModel? ContentEntity { get; set; }
    }
}