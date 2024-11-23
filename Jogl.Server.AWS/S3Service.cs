using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Jogl.Server.API.AWS
{
    public class S3Service : IS3Service
    {
        public async Task<List<string>> ListObjectsAsync(RegionEndpoint region, string bucket, string prefix, string sinceKey)
        {
            using (var client = new AmazonS3Client(new AnonymousAWSCredentials(), region))
            {
                var list = new List<string>();
                while (true)
                {
                    var response = await client.ListObjectsV2Async(new ListObjectsV2Request
                    {
                        BucketName = bucket,
                        Prefix = prefix,
                        StartAfter = sinceKey,
                    });

                    list.AddRange(response.S3Objects.Select(o => o.Key));
                    if (response.S3Objects.Count < 1000)
                        return list;
                    
                    sinceKey = response.S3Objects[1000 - 1].Key;
                }
            }
        }

        public async Task<string> DownloadObjectAsync(RegionEndpoint region, string bucket, string key)
        {
            using (var client = new AmazonS3Client(new AnonymousAWSCredentials(), region))
            using (var response = await client.GetObjectAsync(new GetObjectRequest { BucketName = bucket, Key = key }))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await response.ResponseStream.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(memoryStream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
        }
    }
}
