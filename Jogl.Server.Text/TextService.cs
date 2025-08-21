using HtmlAgilityPack;

namespace Jogl.Server.Text
{
    public class TextService : ITextService
    {
        public string StripHtml(string text)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(text);
            return doc.DocumentNode.InnerText;
        }
    }
}
