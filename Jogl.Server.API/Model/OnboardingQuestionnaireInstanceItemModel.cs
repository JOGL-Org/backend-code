using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingQuestionnaireInstanceItemModel
    {
        [JsonPropertyName("question")]
        public string Question { get; set; }
        
        [JsonPropertyName("answer")]
        public string Answer { get; set; }
    }
}