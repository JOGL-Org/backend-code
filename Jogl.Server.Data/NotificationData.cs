namespace Jogl.Server.Data
{
    public enum NotificationDataKey { CommunityEntity, ContentEntity, User, Role, Invitation, FeedEntity }

    public class NotificationData
    {
        public NotificationDataKey Key { get; set; }
        public string? EntityId { get; set; }
        public CommunityEntityType? CommunityEntityType { get; set; }
        public OnboardingConfiguration? CommunityEntityOnboarding { get; set; }
        public ContentEntityType? ContentEntityType { get; set; }
        public string? EntityTitle { get; set; }
        public string? EntitySubtype { get; set; }
        public string? EntityLogoId { get; set; }
        public string? EntityBannerId { get; set; }
        public string? EntityHomeChannelId { get; set; }
        public bool EntityOnboardingAnswersAvailable { get; set; }

        [Obsolete] 
        public bool EntityOnboardingEnabled { get; set; }
    }
}