using Azure;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.Storage
{
    public class BlobStorageService : IStorageService
    {
        private readonly IConfiguration _configuration;

        public BlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task<BlobClient> GetClientAsync(string container, string id)
        {
            var sharedKeyCredential = new StorageSharedKeyCredential(_configuration["Azure:Storage:Account"], _configuration["Azure:Storage:Key"]);
            var serviceClient = new BlobServiceClient(new Uri($"https://{_configuration["Azure:Storage:Account"]}.blob.core.windows.net"), sharedKeyCredential);
            var blobContainer = serviceClient.GetBlobContainerClient(container);
            await blobContainer.CreateIfNotExistsAsync();

            return blobContainer.GetBlobClient(id);
        }

        public async Task CreateOrReplaceAsync(string container, string id, byte[] data)
        {
            var client = await GetClientAsync(container, id);
            await client.UploadAsync(BinaryData.FromBytes(data), overwrite: true);
        }

        public async Task DeleteAsync(string container, string id)
        {
            var client = await GetClientAsync(container, id);
            await client.DeleteAsync();
        }

        public async Task<byte[]> GetDocumentAsync(string container, string id)
        {
            var client = await GetClientAsync(container, id);
            var res = await client.DownloadContentAsync();
            return res.Value.Content.ToArray();
        }

        public async Task<bool> DocumentExistsAsync(string container, string id)
        {
            var client = await GetClientAsync(container, id);
            return await client.ExistsAsync();
        }
    }
}