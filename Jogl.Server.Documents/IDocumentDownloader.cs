namespace Jogl.Server.Documents
{
    public interface IDocumentDownloader
    {
        public Task<byte[]> DownloadFileAsync(string url);
    }
}