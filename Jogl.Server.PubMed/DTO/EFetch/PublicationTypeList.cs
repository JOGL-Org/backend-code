using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "PublicationTypeList")]
	public class PublicationTypeList
	{
		[XmlElement(ElementName = "PublicationType")]
		public List<PublicationType> PublicationType { get; set; }
	}
}
