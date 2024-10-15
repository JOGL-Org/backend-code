using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "ISSN")]
	public class ISSN
	{

		[XmlAttribute(AttributeName = "IssnType")]
		public string IssnType { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
