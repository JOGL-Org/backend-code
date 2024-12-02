namespace Jogl.Server.Data
{
    public class InvitationKey : Entity
    {
        public string CommunityEntityId { get; set; }
        public CommunityEntityType CommunityEntityType { get; set; }
        public string InviteKey { get; set; }
    }
}