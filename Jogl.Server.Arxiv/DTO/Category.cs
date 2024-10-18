using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "category")]
    public class Category
    {

        [XmlAttribute(AttributeName = "term")]
        public string Term { get; set; }

        [XmlAttribute(AttributeName = "scheme")]
        public string Scheme { get; set; }
    }
}