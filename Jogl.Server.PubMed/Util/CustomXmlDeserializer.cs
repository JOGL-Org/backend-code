/*using RestSharp;
using RestSharp.Serializers.Xml; 
using System.Xml;

public class CustomXmlDeserializer : IRestSerializer {
    private XmlReaderSettings xmlReaderSettings;

    public CustomXmlDeserializer(XmlReaderSettings settings) {
        xmlReaderSettings = settings;
    }

    public T Deserialize<T>(RestResponse response) {
        using (var stringReader = new StringReader(response.Content))
        using (var xmlReader = XmlReader.Create(stringReader, xmlReaderSettings)) {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(xmlReader);
        }
    }

    public string[] SupportedContentTypes { get; } = {
        "application/xml", "text/xml"
    };

    public string ContentType { get; set; } = "application/xml";
}*/