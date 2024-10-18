using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "primary_category")]
    public class PrimaryCategory
    {

        [XmlAttribute(AttributeName = "arxiv")]
        public string Arxiv { get; set; }

        [XmlAttribute(AttributeName = "term")]
        public string Term { get; set; }

        [XmlAttribute(AttributeName = "scheme")]
        public string Scheme { get; set; }
    }
}