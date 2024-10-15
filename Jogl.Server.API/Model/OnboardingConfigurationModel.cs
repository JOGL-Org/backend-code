using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingConfigurationModel
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("presentation")]
        public OnboardingPresentationModel Presentation { get; set; }
     
        [JsonPropertyName("questionnaire")]
        public OnboardingQuestionnaireModel Questionnaire { get; set; }
    
        [JsonPropertyName("rules")]
        public OnboardingRulesModel Rules { get; set; }
    }
}