using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "DateCreated")]
	public class DateCreated
	{
		[XmlElement(ElementName = "Year")]
		public string Year { get; set; }

		[XmlElement(ElementName = "Month")]
		public string Month { get; set; }

		[XmlElement(ElementName = "Day")]
		public string Day { get; set; }
	}
}
