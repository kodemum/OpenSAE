using System.Windows;

namespace OpenSAE.Core
{
    public struct SymbolArtPoint : IEquatable<SymbolArtPoint>
    {
        public SymbolArtPoint(short x, short y)
        {
            X = x;
            Y = y;
        }

        public SymbolArtPoint(Point point)
        {
            X = (short)Math.Round(point.X);
            Y = (short)Math.Round(point.Y);
        }

        public short X { get; }

        public short Y { get; }

        public static bool operator ==(SymbolArtPoint a, SymbolArtPoint b)
            => a.X == b.X && a.Y == b.Y;

        public static bool operator !=(SymbolArtPoint a, SymbolArtPoint b)
            => a.X != b.X || a.Y != b.Y;

        public static SymbolArtPoint operator +(SymbolArtPoint a, SymbolArtPoint b)
            => new((short)(a.X + b.X), (short)(a.Y + b.Y));

        public static SymbolArtPoint operator -(SymbolArtPoint a, SymbolArtPoint b)
            => new((short)(a.X - b.X), (short)(a.Y - b.Y));

        public static implicit operator Point (SymbolArtPoint point) => new(point.X, point.Y);

        public override bool Equals(object? obj)
        {
            return obj is SymbolArtPoint point && Equals(point);
        }

        public bool Equals(SymbolArtPoint other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
