namespace OpenSAE.Core.BitmapConverter
{
    internal class GeometrizeShape
    {
        public ShapeType Type { get; }

        public System.Windows.Media.Color Color { get; }

        public double Score { get; }

        public double[] Points { get; }

        public GeometrizeShape(ShapeType type, System.Windows.Media.Color color, double score, double[] points)
        {
            Type = type;
            Color = color;
            Score = score;
            Points = points;
        }
    }
}
