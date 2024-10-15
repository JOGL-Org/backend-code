namespace Jogl.Server.Data
{
    public class CommunityEntityFollowing : Entity
    {
        public string UserIdFrom { get; set; }
        public string CommunityEntityId { get; set; }
        public CommunityEntityType CommunityEntityType { get; set; }
    }
}