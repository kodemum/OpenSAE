using System.Xml.Serialization;

namespace OpenSAE.Core.SAML
{
    public class SamlBitmapImageLayer : SamlItem
    {
        [XmlAttribute("imageData")]
        public byte[]? ImageData { get; set; }

        [XmlAttribute("alpha")]
        public double Alpha { get; set; }

        [XmlAttribute("ltx")]
        public double Ltx { get; set; }

        [XmlAttribute("lty")]
        public double Lty { get; set; }

        [XmlAttribute("lbx")]
        public double Lbx { get; set; }

        [XmlAttribute("lby")]
        public double Lby { get; set; }

        [XmlAttribute("rtx")]
        public double Rtx { get; set; }

        [XmlAttribute("rty")]
        public double Rty { get; set; }

        [XmlAttribute("rbx")]
        public double Rbx { get; set; }

        [XmlAttribute("rby")]
        public double Rby { get; set; }
    }
}
