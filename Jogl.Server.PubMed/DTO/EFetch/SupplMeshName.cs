using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
    [XmlRoot(ElementName = "SupplMeshName")]
    public class SupplMeshName
    {

        [XmlAttribute(AttributeName = "Type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "UI")]
        public string UI { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

}
