using System.Xml.Serialization;

namespace Jogl.Server.Loader.PubMed.DTO
{
    [XmlRoot(ElementName = "DescriptorName")]
    public class DescriptorName
    {

        [XmlElement(ElementName = "String")]
        public string String { get; set; }
    }
}
