using System.Windows;
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
            get => (short)Math.Round(Vertex1.X);
            set => Vertex1 = new Point(value, Vertex1.Y);
        }

        [XmlAttribute("lty")]
        public short Lty
        {
            get => (short)Math.Round(Vertex1.Y);
            set => Vertex1 = new Point(Vertex1.X, value);
        }

        [XmlAttribute("lbx")]
        public short Lbx
        {
            get => (short)Math.Round(Vertex2.X);
            set => Vertex2 = new Point(value, Vertex2.Y);
        }

        [XmlAttribute("lby")]
        public short Lby
        {
            get => (short)Math.Round(Vertex2.Y);
            set => Vertex2 = new Point(Vertex2.X, value);
        }

        [XmlAttribute("rtx")]
        public short Rtx
        {
            get => (short)Math.Round(Vertex3.X);
            set => Vertex3 = new Point(value, Vertex3.Y);
        }

        [XmlAttribute("rty")]
        public short Rty
        {
            get => (short)Math.Round(Vertex3.Y);
            set => Vertex3 = new Point(Vertex3.X, value);
        }

        [XmlAttribute("rbx")]
        public short Rbx
        {
            get => (short)Math.Round(Vertex4.X);
            set => Vertex4 = new Point(value, Vertex4.Y);
        }

        [XmlAttribute("rby")]
        public short Rby
        {
            get => (short)Math.Round(Vertex4.Y);
            set => Vertex4 = new Point(Vertex4.X, value);
        }

        // Symbol arts have limited precision, the coordinates used do not support floating point.
        // but because this makes manipulating them not loose precision, we want to use floating points
        // whenever we manipulate them in the app.
        // For this we use the Point struct

        /// <summary>
        /// First vertex - top left
        /// </summary>
        [XmlIgnore]
        public Point Vertex1 { get; set; }

        /// <summary>
        /// Second vertex - bottom left
        /// </summary>
        [XmlIgnore]
        public Point Vertex2 { get; set; }

        /// <summary>
        /// Third vertex - top right
        /// </summary>
        [XmlIgnore]
        public Point Vertex3 { get; set; }

        /// <summary>
        /// Fouth vertex - bottom right
        /// </summary>
        [XmlIgnore]
        public Point Vertex4 { get; set; }
    }
}
