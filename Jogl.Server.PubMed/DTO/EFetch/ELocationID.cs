using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "ELocationID")]
	public class ELocationID
	{
		[XmlAttribute(AttributeName = "EIdType")]
		public string EIdType { get; set; }

		[XmlAttribute(AttributeName = "ValidYN")]
		public string ValidYN { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
