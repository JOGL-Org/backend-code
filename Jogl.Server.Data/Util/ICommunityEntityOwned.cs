namespace Jogl.Server.Data.Util
{
    public interface ICommunityEntityOwned
    {
        public string CommunityEntityId { get; }
        public CommunityEntity CommunityEntity { get; set; }
    }
}
