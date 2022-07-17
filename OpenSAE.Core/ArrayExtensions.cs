using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Core
{
    public static class ArrayExtensions
    {
        public static int GetMinIndexBy(this SymbolArtPoint[] array, bool byX)
        {
            int minIndex = 0;
            int minValue = byX ? array[0].X : array[0].Y;

            for (int i = 1; i < array.Length; i++)
            {
                var target = byX ? array[i].X : array[i].Y;

                if (target < minValue)
                {
                    minIndex = i;
                    minValue = target;
                }
            }

            return minIndex;
        }

        public static SymbolArtPoint GetMinBy(this SymbolArtPoint[] array, bool byX)
        {
            return array[array.GetMinIndexBy(byX)];
        }
    }
}
