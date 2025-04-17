using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "PublicationType")]
	public class PublicationType
	{
		[XmlAttribute(AttributeName = "UI")]
		public string UI { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
