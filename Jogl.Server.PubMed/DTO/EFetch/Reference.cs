using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "Reference")]
	public class Reference
	{
		[XmlElement(ElementName = "Citation")]
		public string Citation { get; set; }

		[XmlElement(ElementName = "ArticleIdList")]
		public ArticleIdList ArticleIdList { get; set; }
	}
}
