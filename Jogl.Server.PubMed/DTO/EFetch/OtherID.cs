using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "OtherID")]
	public class OtherID
	{

		[XmlAttribute(AttributeName = "Source")]
		public string Source { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
