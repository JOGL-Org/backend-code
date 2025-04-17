using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "ArticleIdList")]
	public class ArticleIdList
	{
		[XmlElement(ElementName = "ArticleId")]
		public List<ArticleId> ArticleId { get; set; }
	}
}
