using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "AffiliationInfo")]
	public class AffiliationInfo
	{
		[XmlElement(ElementName = "Affiliation")]
		public string Affiliation { get; set; }
	}
}
