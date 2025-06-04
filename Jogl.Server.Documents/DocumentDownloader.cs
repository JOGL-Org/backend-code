using RestSharp;

namespace Jogl.Server.Documents
{
    public class DocumentDownloader : IDocumentDownloader
    {
        public async Task<byte[]> DownloadFileAsync(string url)
        {
            var client = new RestClient();
            var request = new RestRequest(url, Method.Get);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Failed to download file. Status code: {response.StatusCode}, Error message: {response.ErrorMessage}");

            return response.RawBytes;
        }
    }
}