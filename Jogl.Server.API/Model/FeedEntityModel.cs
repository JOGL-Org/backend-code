using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FeedEntityModel : BaseModel
    {
        [JsonPropertyName("opened")]
        public DateTime? LastOpenedUTC { get; set; }
    }
}