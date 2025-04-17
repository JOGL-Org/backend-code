using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "AuthorList")]
	public class AuthorList
	{
		[XmlAttribute(AttributeName = "CompleteYN")]
		public string CompleteYN { get; set; }
		
		[XmlElement(ElementName = "Author")]
		public List<Author> Author { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
