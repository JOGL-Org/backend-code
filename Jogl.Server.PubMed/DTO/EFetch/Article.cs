using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "Article")]
	public class Article
	{
		[XmlAttribute(AttributeName = "PubModel")]
		public string PubModel { get; set; }

		[XmlElement(ElementName = "Journal")]
		public Journal Journal { get; set; }

		[XmlElement(ElementName = "ArticleTitle")]
		public ArticleTitle ArticleTitle { get; set; }

		[XmlElement(ElementName = "Pagination")]
		public Pagination Pagination { get; set; }

		[XmlElement(ElementName = "ELocationID")]
		public List<ELocationID> ELocationID { get; set; }

		[XmlElement(ElementName = "Abstract")]
		public Abstract Abstract { get; set; }

		[XmlElement(ElementName = "AuthorList")]
		public AuthorList AuthorList { get; set; }

		[XmlElement(ElementName = "Language")]
		public string Language { get; set; }

		[XmlElement(ElementName = "DataBankList")]
		public DataBankList DataBankList { get; set; }

		[XmlElement(ElementName = "GrantList")]
		public GrantList GrantList { get; set; }

		[XmlElement(ElementName = "PublicationTypeList")]
		public PublicationTypeList PublicationTypeList { get; set; }

		[XmlElement(ElementName = "VernacularTitle")]
		public string VernacularTitle { get; set; }

		[XmlElement(ElementName = "ArticleDate")]
		public ArticleDate ArticleDate { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
