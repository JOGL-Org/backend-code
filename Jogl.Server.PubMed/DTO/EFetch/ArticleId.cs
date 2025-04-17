using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "ArticleId")]
	public class ArticleId
	{
		[XmlAttribute(AttributeName = "IdType")]
		public string IdType { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
