using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "GrantList")]
	public class GrantList
	{
		[XmlAttribute(AttributeName = "CompleteYN")]
		public string CompleteYN { get; set; }

		[XmlElement(ElementName = "Grant")]
		public List<Grant> Grant { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
