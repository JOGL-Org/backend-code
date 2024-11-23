using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Jogl.Server.Loader.PubMed.DTO
{
    [XmlRoot(ElementName = "DescriptorRecord")]
    public class DescriptorRecord
    {
        [XmlElement(ElementName = "DescriptorUI")]
        public string DescriptorUI { get; set; }

        [XmlElement(ElementName = "DescriptorName")]
        public DescriptorName DescriptorName { get; set; }

        [XmlElement(ElementName = "TreeNumberList")]
        public TreeNumberList TreeNumberList { get; set; }

        [XmlAttribute(AttributeName = "DescriptorClass")]
        public int DescriptorClass { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
