using Geometrize.Shape;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geometrize
{
    public class ShapeAddResult
    {
        public IShape Shape {  get; set; }

        public double Score { get; set; }

        public int Color { get; set; }
    }
}
