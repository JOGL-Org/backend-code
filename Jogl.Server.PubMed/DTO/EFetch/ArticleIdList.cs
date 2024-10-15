using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "ArticleIdList")]
	public class ArticleIdList
	{
		[XmlElement(ElementName = "ArticleId")]
		public List<ArticleId> ArticleId { get; set; }
	}
}
