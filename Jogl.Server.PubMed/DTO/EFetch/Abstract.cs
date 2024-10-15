using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "Abstract")]
	public class Abstract
	{
		[XmlElement(ElementName = "AbstractText")]
		public List<AbstractText> AbstractText { get; set; }

		[XmlElement(ElementName = "CopyrightInformation")]
		public string CopyrightInformation { get; set; }
	}
}
