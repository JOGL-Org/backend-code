using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingQuestionnaireItemModel
    {
        [JsonPropertyName("question")]
        public string Question { get; set; }
    }
}