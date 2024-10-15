using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "PersonalNameSubjectList")]
	public class PersonalNameSubjectList
	{		
		[XmlElement(ElementName = "PersonalNameSubject")]
		public List<PersonalNameSubject> PersonalNameSubject { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
