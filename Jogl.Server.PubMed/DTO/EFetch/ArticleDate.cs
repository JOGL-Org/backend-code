using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "ArticleDate")]
	public class ArticleDate
	{
		[XmlAttribute(AttributeName = "DateType")]
		public string DateType { get; set; }

		[XmlElement(ElementName = "Year")]
		public string Year { get; set; }

		[XmlElement(ElementName = "Month")]
		public string Month { get; set; }

		[XmlElement(ElementName = "Day")]
		public string Day { get; set; }
		
		[XmlText]
		public string Text { get; set; }
	}
}
