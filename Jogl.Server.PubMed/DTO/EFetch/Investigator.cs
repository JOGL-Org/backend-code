using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "Investigator")]
	public class Investigator
	{
		[XmlAttribute(AttributeName = "ValidYN")]
		public string ValidYN { get; set; }

		[XmlElement(ElementName = "LastName")]
		public string LastName { get; set; }

		[XmlElement(ElementName = "ForeName")]
		public string ForeName { get; set; }

		[XmlElement(ElementName = "Initials")]
		public string Initials { get; set; }

		[XmlElement(ElementName = "Suffix")]
		public string Suffix { get; set; }

		[XmlElement(ElementName = "CollectiveName")]
		public string CollectiveName { get; set; }

		[XmlElement(ElementName = "Identifier")]
		public Identifier Identifier { get; set; }

		[XmlElement(ElementName = "AffiliationInfo")]
		public List<AffiliationInfo> AffiliationInfo { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
