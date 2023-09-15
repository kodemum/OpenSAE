// Generated by Haxe 4.3.1

using Geometrize.Rasterizer;
using System.Collections.Generic;

namespace Geometrize.Shape
{
    public interface IShape
    {
        IReadOnlyList<Scanline> Rasterize();

        void Mutate();

        IShape Clone();

        double[] GetRawShapeData();
    }
}