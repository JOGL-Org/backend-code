using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingPresentationItemModel
    {
        [JsonPropertyName("image_id")]
        public string ImageId { get; set; }
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}