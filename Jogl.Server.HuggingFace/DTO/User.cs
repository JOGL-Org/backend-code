using System.Text.Json.Serialization;

namespace Jogl.Server.HuggingFace.DTO
{
    public class User
    {
        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("fullname")]
        public string Fullname { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("isPro")]
        public bool IsPro { get; set; }

        [JsonPropertyName("isHf")]
        public bool IsHf { get; set; }

        [JsonPropertyName("isMod")]
        public bool IsMod { get; set; }

        [JsonIgnore()]
        public string URL
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return null;

                return $"https://huggingface.co/{Name}";
            }
        }
    }
}