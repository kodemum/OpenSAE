namespace OpenSAE.Core.BitmapConverter
{
    public class BitmapToSymbolArtConverterOptions
    {
        public int ResizeImageHeight { get; set; } = 160;

        public bool RespectEdges { get; set; } = false;

        public double SymbolOpacity { get; set; } = 1d / 7 * 5;

        public int ShapesPerStep { get; set; } = 150;

        public int MutationsPerStep { get; set; } = 170;

        public int MaxSymbolCount { get; set; } = 225;

        public bool IncludeBackground { get; set; } = true;

        public ShapeType[] ShapeTypes { get; set; } = new ShapeType[] { ShapeType.Rotated_Rectangle, ShapeType.Ellipse };
    }
}
