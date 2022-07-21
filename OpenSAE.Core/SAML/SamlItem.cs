using System.Xml.Serialization;

namespace OpenSAE.Core.SAML
{
    public abstract class SamlItem
    {
        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("visible")]
        public bool Visible { get; set; }
    }
}
