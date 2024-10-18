using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "totalResults")]
    public class TotalResults
    {

        [XmlAttribute(AttributeName = "opensearch")]
        public string Opensearch { get; set; }

        [XmlText]
        public int Text { get; set; }
    }
}