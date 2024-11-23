using System.Xml.Serialization;

namespace Jogl.Server.Loader.PubMed.DTO
{
    [XmlRoot(ElementName = "TreeNumberList")]
    public class TreeNumberList
    {

        [XmlElement(ElementName = "TreeNumber")]
        public List<string> TreeNumber { get; set; }
    }
}
