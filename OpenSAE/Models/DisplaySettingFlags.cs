using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Models
{
    [Flags]
    public enum DisplaySettingFlags
    {
        None = 0,
        NaturalSymbolSelection = 1,
        DarkBackground = 2,
        RestrictToAffineManipulation = 4,
        IngameRenderMode = 8,
    }
}
