using System.Windows;
using System.Xml.Serialization;

namespace OpenSAE.Core.SAML
{
    public class SamlLayer : SamlItem
    {
        [XmlAttribute("type")]
        public int Type { get; set; }

        [XmlAttribute("color")]
        public string? Color { get; set; }

        [XmlAttribute("alpha")]
        public double Alpha { get; set; }

        [XmlAttribute("ltx")]
        public short Ltx { get; set; }

        [XmlAttribute("lty")]
        public short Lty { get; set; }

        [XmlAttribute("lbx")]
        public short Lbx { get; set; }

        [XmlAttribute("lby")]
        public short Lby { get; set; }

        [XmlAttribute("rtx")]
        public short Rtx { get; set; }

        [XmlAttribute("rty")]
        public short Rty { get; set; }

        [XmlAttribute("rbx")]
        public short Rbx { get; set; }

        [XmlAttribute("rby")]
        public short Rby { get; set; }
    }
}
