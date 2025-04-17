using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "Journal")]
	public class Journal
	{
		[XmlElement(ElementName = "ISSN")]
		public ISSN ISSN { get; set; }

		[XmlElement(ElementName = "JournalIssue")]
		public JournalIssue JournalIssue { get; set; }

		[XmlElement(ElementName = "Title")]
		public string Title { get; set; }

		[XmlElement(ElementName = "ISOAbbreviation")]
		public string ISOAbbreviation { get; set; }
	}
}
