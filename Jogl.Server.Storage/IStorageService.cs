namespace Jogl.Server.Storage
{
    public interface IStorageService
    {
        public const string DOCUMENT_CONTAINER = "doc";
        public const string IMAGE_CONTAINER = "img";

        Task CreateOrReplaceAsync(string container, string id, byte[] data);
        Task DeleteAsync(string container, string id);
        Task<byte[]> GetDocumentAsync(string container, string id);
        Task<bool> DocumentExistsAsync(string container, string id);
    }
}