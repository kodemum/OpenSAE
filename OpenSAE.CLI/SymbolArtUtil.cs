using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.CLI
{
    internal static class SymbolArtUtil
    {
        public static string SerializeInfoToJson(SymbolArt sa)
        {
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                format = sa.FileFormat.ToString(),
                name = sa.Name,
                height = sa.Height,
                width = sa.Width,
                soundEffect = sa.Sound.ToString(),
                authorId = sa.AuthorId,
                symbolCount = GetLayerCount(sa)
            });
        }

        public static int GetLayerCount(SymbolArtGroup group)
        {
            int count = 0;

            foreach (var item in group.Children)
            {
                if (item is SymbolArtGroup subGroup)
                {
                    count += GetLayerCount(subGroup);
                }
                else
                {
                    count++;
                }
            }

            return count;
        }
    }
}
