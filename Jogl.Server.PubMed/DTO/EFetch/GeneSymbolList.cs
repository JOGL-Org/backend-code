using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
    [XmlRoot(ElementName = "GeneSymbolList")]
    public class GeneSymbolList
    {
        [XmlElement(ElementName = "GeneSymbol")]
        public List<string> GeneSymbol { get; set; }
    }
}