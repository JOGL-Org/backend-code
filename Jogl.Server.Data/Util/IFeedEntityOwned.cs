namespace Jogl.Server.Data.Util
{
    public interface IFeedEntityOwned
    {
        public string FeedEntityId { get; }
        public FeedEntity FeedEntity { get; }
    }
}
