using System.Text.Json.Serialization;

namespace Jogl.Server.HuggingFace.DTO
{
    public abstract class Repo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("private")]
        public bool Private { get; set; }

        public abstract string Url { get; }
    }
}