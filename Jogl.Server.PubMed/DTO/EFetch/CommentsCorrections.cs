using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "CommentsCorrections")]
	public class CommentsCorrections
	{
		[XmlAttribute(AttributeName = "RefType")]
		public string RefType { get; set; }

		[XmlElement(ElementName = "RefSource")]
		public string RefSource { get; set; }

		[XmlElement(ElementName = "PMID")]
		public PMID PMID { get; set; }

		[XmlElement(ElementName = "Note")]
		public string Note { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
