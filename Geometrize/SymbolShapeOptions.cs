using Geometrize.Shape;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geometrize
{
    public class SymbolShapeOptions
    {
        public int[] ShapeTypes { get; set; }

        public List<SymbolShapeDefinition> SymbolDefinitions { get; set; } = new List<SymbolShapeDefinition>();
    }
}
