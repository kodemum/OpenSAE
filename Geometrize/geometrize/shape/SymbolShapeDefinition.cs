namespace geometrize.shape
{
    public class SymbolShapeDefinition
    {
        public int SymbolId { get; set; }

        public (int x, int x2)[] SymbolScanlines { get; set; }

        public bool HorizontallySymmetric { get; set; }

        public bool VerticallySymmetric { get; set; }
    }
}
