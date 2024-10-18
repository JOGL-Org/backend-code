using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "doi")]
    public class Doi
    {

        [XmlAttribute(AttributeName = "arxiv")]
        public string Arxiv { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}