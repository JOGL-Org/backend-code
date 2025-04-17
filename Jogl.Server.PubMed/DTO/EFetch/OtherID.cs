using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
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
