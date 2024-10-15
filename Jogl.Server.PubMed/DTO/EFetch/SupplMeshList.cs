using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
    [XmlRoot(ElementName = "SupplMeshList")]
    public class SupplMeshList
    {
        [XmlElement(ElementName = "SupplMeshName")]
        public List<SupplMeshName> SupplMeshName { get; set; }
    }
}
