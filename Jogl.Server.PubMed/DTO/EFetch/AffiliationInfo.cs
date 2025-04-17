using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "AffiliationInfo")]
	public class AffiliationInfo
	{
		[XmlElement(ElementName = "Affiliation")]
		public string Affiliation { get; set; }
	}
}
