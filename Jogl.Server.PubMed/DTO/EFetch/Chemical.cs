using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "Chemical")]
	public class Chemical
	{
		[XmlElement(ElementName = "RegistryNumber")]
		public string RegistryNumber { get; set; }

		[XmlElement(ElementName = "NameOfSubstance")]
		public NameOfSubstance NameOfSubstance { get; set; }
	}
}
