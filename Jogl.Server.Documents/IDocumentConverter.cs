using Jogl.Server.Data.Util;

namespace Jogl.Server.Documents
{
    public interface IDocumentConverter
    {
        public byte[] ConvertDocumentToPDF(FileData data);
    }
}