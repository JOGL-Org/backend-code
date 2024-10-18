using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "itemsPerPage")]
    public class ItemsPerPage
    {

        [XmlAttribute(AttributeName = "opensearch")]
        public string Opensearch { get; set; }

        [XmlText]
        public int Text { get; set; }
    }
}