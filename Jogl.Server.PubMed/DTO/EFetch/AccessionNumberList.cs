using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{

    [XmlRoot(ElementName = "AccessionNumberList")]
    public class AccessionNumberList
    {
        [XmlElement(ElementName = "AccessionNumber")]
        public List<string> AccessionNumber { get; set; }
    }
}
