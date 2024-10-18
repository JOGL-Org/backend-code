using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "comment")]
    public class Comment
    {

        [XmlAttribute(AttributeName = "arxiv")]
        public string Arxiv { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}