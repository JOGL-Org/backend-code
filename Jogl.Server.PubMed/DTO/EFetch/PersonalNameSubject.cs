using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "PersonalNameSubject")]
	public class PersonalNameSubject
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

		[XmlText]
		public string Text { get; set; }
	}
}
