using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "title")]
    public class Title
    {

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}