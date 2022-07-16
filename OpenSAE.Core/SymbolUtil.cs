namespace OpenSAE.Core
{
    public static class SymbolUtil
    {
        private static List<(int From, int To)> _validRanges = new()
        {
            (1, 86),
            (241, 293),
            (321, 359),
            (401, 439),
            (481, 517),
            (561, 585),
            (609, 613),
            (641, 704),
            (721, 754)
        };

        public static bool IsKnownSymbolId(int symbol)
        {
            symbol++;

            foreach ((int From, int To) in _validRanges)
            {
                if (symbol >= From && symbol <= To)
                    return true;
            }

            return false;
        }

        public static string? GetSymbolPackUri(int symbol)
        {
            if (IsKnownSymbolId(symbol))
            {
                return $"pack://application:,,,/OpenSAE.Core;component/assets/{symbol + 1}.png";
            }
            else
            {
                return null;
            }
        }
    }
}
