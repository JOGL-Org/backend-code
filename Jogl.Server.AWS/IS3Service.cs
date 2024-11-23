using Amazon;

namespace Jogl.Server.API.AWS
{
    public interface IS3Service
    {
        Task<List<string>> ListObjectsAsync(RegionEndpoint region, string bucket, string prefix, string sinceKey);
        Task<string> DownloadObjectAsync(RegionEndpoint region, string bucket, string key);
    }
}
