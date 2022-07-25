using System.Xml.Serialization;

namespace OpenSAE.Core.SAML
{
    public class SamlGroup : SamlItem
    {
        [XmlElement(ElementName = "g", Type = typeof(SamlGroup))]
        [XmlElement(ElementName = "layer", Type = typeof(SamlLayer))]
        [XmlElement(ElementName = "bitmapLayer", Type = typeof(SamlBitmapImageLayer))]
        public List<SamlItem> Children { get; set; }
            = new();
    }
}
