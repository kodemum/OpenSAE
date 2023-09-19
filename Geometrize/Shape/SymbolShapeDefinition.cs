namespace Geometrize.Shape
{
    public class SymbolShapeDefinition
    {
        public int SymbolId { get; set; }

        public byte[,] SymbolScanlines { get; set; }

        public bool HorizontallySymmetric { get; set; }

        public bool VerticallySymmetric { get; set; }
    }
}
