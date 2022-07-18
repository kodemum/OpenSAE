using System.Diagnostics;
using System.Windows;

namespace OpenSAE.Core
{
    /// <summary>
    /// Represents X and Y coordinates in a symbol art. Immutable.
    /// Stores coordinates using <see cref="double"/>s but symbol arts only supports
    /// straight <see cref="short"/>s. This is done to prevent accumulation of rounding errors
    /// when resizing symbols or symbol groups.
    /// </summary>
    [DebuggerDisplay("x={PreciseX}, y={PreciseY}")]
    public struct SymbolArtPoint : IEquatable<SymbolArtPoint>
    {
        public SymbolArtPoint(short x, short y)
        {
            X = x;
            Y = y;
            RoundedX = x;
            RoundedY = y;
        }

        public SymbolArtPoint(double x, double y)
        {
            RoundedX = (short)Math.Round(x);
            RoundedY = (short)Math.Round(y);

            X = x;
            Y = y;
        }

        public SymbolArtPoint(Point point)
            : this(point.X, point.Y)
        {
        }

        /// <summary>
        /// Gets the X coordinate of the point
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the Y coordinate of the point
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Gets the rounded X coordinate of the point
        /// </summary>
        public short RoundedX { get; }

        /// <summary>
        /// Gets the rounded Y coordinate of the point
        /// </summary>
        public short RoundedY { get; }

        /// <summary>
        /// Creates a copy of the point with additional precision removed
        /// </summary>
        /// <returns></returns>
        public SymbolArtPoint Round() => new(RoundedX, RoundedY);

        public static bool operator ==(SymbolArtPoint a, SymbolArtPoint b)
            => a.X == b.X && a.Y == b.Y;

        public static bool operator !=(SymbolArtPoint a, SymbolArtPoint b)
            => a.X != b.X || a.Y != b.Y;

        public static SymbolArtPoint operator +(SymbolArtPoint a, SymbolArtPoint b)
            => new(a.X + b.X, a.Y + b.Y);

        public static SymbolArtPoint operator +(SymbolArtPoint a, Vector b)
            => new(a.X + b.X, a.Y + b.Y);

        public static SymbolArtPoint operator -(SymbolArtPoint a, SymbolArtPoint b)
            => new(a.X - b.X, a.Y - b.Y);

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
