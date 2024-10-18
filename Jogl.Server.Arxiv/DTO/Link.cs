using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "link")]
    public class Link
    {

        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }

        [XmlAttribute(AttributeName = "rel")]
        public string Rel { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
    }
}