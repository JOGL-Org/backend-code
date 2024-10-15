using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingQuestionnaireModel
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        
        [JsonPropertyName("items")]
        public List<OnboardingQuestionnaireItemModel> items { get; set; }
    }
}