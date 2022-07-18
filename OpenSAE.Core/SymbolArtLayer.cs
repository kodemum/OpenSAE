using System.Xml.Serialization;

namespace OpenSAE.Core
{
    public class SymbolArtLayer : SymbolArtItem
    {
        [XmlAttribute("type")]
        public int Type { get; set; }

        [XmlAttribute("color")]
        public string? Color { get; set; }

        [XmlAttribute("alpha")]
        public double Alpha { get; set; }

        [XmlAttribute("ltx")]
        public short Ltx
        {
            get => Vertex1.RoundedX;
            set => Vertex1 = new SymbolArtPoint(value, Vertex1.Y);
        }

        [XmlAttribute("lty")]
        public short Lty
        {
            get => Vertex1.RoundedY;
            set => Vertex1 = new SymbolArtPoint(Vertex1.X, value);
        }

        [XmlAttribute("lbx")]
        public short Lbx
        {
            get => Vertex2.RoundedX;
            set => Vertex2 = new SymbolArtPoint(value, Vertex2.Y);
        }

        [XmlAttribute("lby")]
        public short Lby
        {
            get => Vertex2.RoundedY;
            set => Vertex2 = new SymbolArtPoint(Vertex2.X, value);
        }

        [XmlAttribute("rtx")]
        public short Rtx
        {
            get => Vertex3.RoundedX;
            set => Vertex3 = new SymbolArtPoint(value, Vertex3.Y);
        }

        [XmlAttribute("rty")]
        public short Rty
        {
            get => Vertex3.RoundedY;
            set => Vertex3 = new SymbolArtPoint(Vertex3.X, value);
        }

        [XmlAttribute("rbx")]
        public short Rbx
        {
            get => Vertex4.RoundedX;
            set => Vertex4 = new SymbolArtPoint(value, Vertex4.Y);
        }

        [XmlAttribute("rby")]
        public short Rby
        {
            get => Vertex4.RoundedY;
            set => Vertex4 = new SymbolArtPoint(Vertex4.X, value);
        }

        // Symbol arts have limited precision, the coordinates used do not support floating point.
        // but because this makes manipulating them not loose precision, we want to use floating points
        // whenever we manipulate them in the app.
        // For this we use the SymbolArtPoint struct

        /// <summary>
        /// First vertex - top left
        /// </summary>
        [XmlIgnore]
        public SymbolArtPoint Vertex1 { get; set; }

        /// <summary>
        /// Second vertex - bottom left
        /// </summary>
        [XmlIgnore]
        public SymbolArtPoint Vertex2 { get; set; }

        /// <summary>
        /// Third vertex - top right
        /// </summary>
        [XmlIgnore]
        public SymbolArtPoint Vertex3 { get; set; }

        /// <summary>
        /// Fouth vertex - bottom right
        /// </summary>
        [XmlIgnore]
        public SymbolArtPoint Vertex4 { get; set; }
    }
}
