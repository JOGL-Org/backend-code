using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ReactionUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }
    }
}