using Jogl.Server.Data.Util;
using Syncfusion.Presentation;
using Syncfusion.PresentationRenderer;
using Syncfusion.DocIORenderer;
using Syncfusion.DocIO;
using Syncfusion.XlsIORenderer;
using Syncfusion.XlsIO;
using Syncfusion.Pdf;
using Syncfusion.DocIO.DLS;

namespace Jogl.Server.Documents
{
    public class DocumentConverter : IDocumentConverter
    {
        public byte[] ConvertDocumentToPDF(FileData file)
        {
            using (var stream = new MemoryStream(file.Data))
                switch (file.Filetype)
                {
                    //case "ppt":
                    case "pptx":
                    //case "application/vnd.ms-powerpoint":
                    case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                        using (var ppt = Presentation.Open(stream))
                        using (var pdfDocumentPpt = PresentationToPdfConverter.Convert(ppt))
                            return SerializePDF(pdfDocumentPpt);
                    case "doc":
                    case "application/msword":
                        var doc = new WordDocument(stream, Syncfusion.DocIO.FormatType.Doc);
                        using (var rendererDoc = new DocIORenderer())
                        using (var pdfDocumentDoc = rendererDoc.ConvertToPDF(doc))
                            return SerializePDF(pdfDocumentDoc);
                    case "docx":
                    case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                        var docx = new WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
                        using (var rendererDocx = new DocIORenderer())
                        using (var pdfDocumentDocx = rendererDocx.ConvertToPDF(docx))
                            return SerializePDF(pdfDocumentDocx);
                    case "xls":
                    case "xlsx":
                    case "application/vnd.ms-excel":
                    case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                        using (var excelEngine = new ExcelEngine())
                        {
                            IApplication application = excelEngine.Excel;
                            application.DefaultVersion = ExcelVersion.Xlsx;
                            IWorkbook workbook = application.Workbooks.Open(stream);
                            var rendererXlsx = new XlsIORenderer();
                            using (var pdfDocumentXls = rendererXlsx.ConvertToPDF(workbook))
                                return SerializePDF(pdfDocumentXls);
                        }

                    default:
                        throw new NotSupportedException($"Cannot convert format {file.Filetype} to PDF");
                }
        }

        public byte[] ConvertDocumentToPNG(FileData file)
        {
            using (var stream = new MemoryStream(file.Data))
                switch (file.Filetype)
                {
                    case "xls":
                    case "xlsx":
                    case "application/vnd.ms-excel":
                    case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                        using (var excelEngine = new ExcelEngine())
                        {
                            IApplication application = excelEngine.Excel;
                            application.DefaultVersion = ExcelVersion.Xlsx;
                            IWorkbook workbook = application.Workbooks.Open(stream);
                            var rendererXlsx = new XlsIORenderer();
                            using (var outStream = new MemoryStream())
                            {
                                var worksheet = workbook.Worksheets[0];
                                rendererXlsx.ConvertToImage(worksheet, 1, 1, 30, 10, outStream);
                                return outStream.ToArray();
                            }
                        }

                    default:
                        throw new NotSupportedException($"Cannot convert format {file.Filetype} toJPG PDF");
                }
        }

        private byte[] SerializePDF(PdfDocument pdf)
        {
            using (var stream = new MemoryStream())
            {
                pdf.Save(stream);
                return stream.ToArray();
            }
        }
    }
}