using Jogl.Server.Data;

namespace Jogl.Server.URL
{
    public interface IUrlService
    {
        string GetUrl(FeedEntity entity);
        string GetContentEntityUrl(string contentEntityId);
        string GetUrl(FeedEntity entity, Channel channel);
        string GetUrl(string path);
        string GetImageUrl(string imageId);

        string GetUrlFragment(CommunityEntityType type);
        string GetUrlFragment(FeedType type);
        FeedType GetFeedType(string fragment);

        string GetOneTimeLoginLink(string email, string code);
    }
}