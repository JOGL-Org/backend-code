namespace Jogl.Server.Data
{
    public class OnboardingQuestionnaireInstance : Entity
    {
        public string CommunityEntityId { get; set; }
        public string UserId { get; set; }
        public List<OnboardingQuestionnaireInstanceItem> Items { get; set; }
        public DateTime CompletedUTC { get; set; }
    }
}