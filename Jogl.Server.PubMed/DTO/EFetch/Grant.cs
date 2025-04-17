using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "Grant")]
	public class Grant
	{
		[XmlElement(ElementName = "GrantID")]
		public string GrantID { get; set; }

		[XmlElement(ElementName = "Acronym")]
		public string Acronym { get; set; }

		[XmlElement(ElementName = "Agency")]
		public string Agency { get; set; }

		[XmlElement(ElementName = "Country")]
		public string Country { get; set; }
	}
}
