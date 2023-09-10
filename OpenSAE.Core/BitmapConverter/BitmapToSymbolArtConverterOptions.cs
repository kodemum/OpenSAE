using System.Windows.Media;

namespace OpenSAE.Core.BitmapConverter
{
    public class BitmapToSymbolArtConverterOptions
    {
        public int ResizeImageHeight { get; set; } = 384;

        public bool RespectEdges { get; set; } = true;

        public double SymbolOpacity { get; set; } = 1d / 7 * 5;

        public int ShapesPerStep { get; set; } = 1200;

        public int MutationsPerStep { get; set; } = 170;

        public int MaxSymbolCount { get; set; } = 225;

        public bool IncludeBackground { get; set; } = true;

        public ShapeType[] ShapeTypes { get; set; } = new ShapeType[] { ShapeType.Rotated_Ellipse, ShapeType.Rotated_Symbols };

        public List<Symbol> ShapeSymbolsToUse { get; set; } = new();

        public Color BackgroundColor { get; set; }
            = Colors.White;
    }
}
