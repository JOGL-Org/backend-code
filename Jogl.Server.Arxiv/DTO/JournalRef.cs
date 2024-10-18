using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "journal_ref")]
    public class JournalRef
    {

        [XmlAttribute(AttributeName = "arxiv")]
        public string Arxiv { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}