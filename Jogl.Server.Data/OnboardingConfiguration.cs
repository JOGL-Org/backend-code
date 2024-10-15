namespace Jogl.Server.Data
{
    public class OnboardingConfiguration
    {
        public bool Enabled { get; set; }
        public OnboardingPresentation Presentation { get; set; }
        public OnboardingQuestionnaire Questionnaire { get; set; }
        public OnboardingRules Rules { get; set; }
    }
}