using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "ChemicalList")]
	public class ChemicalList
	{
		[XmlElement(ElementName = "Chemical")]
		public List<Chemical> Chemical { get; set; }
	}
}
