using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingPresentationItemUpsertModel
    {
        [JsonPropertyName("image_id")]
        public string ImageId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}