using Geometrize.Shape;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geometrize
{
    public class ShapeAddResult
    {
        public IShape Shape {  get; set; }

        public double Score { get; set; }

        public Rgba32 Color { get; set; }
    }
}
