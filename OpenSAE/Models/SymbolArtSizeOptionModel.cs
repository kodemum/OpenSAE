using OpenSAE.Core;

namespace OpenSAE.Models
{
    public class SymbolArtSizeOptionModel
    {
        public SymbolArtSizeOptionModel(SymbolArtSize value, string text)
        {
            Value = value;
            Text = text;
        }

        public SymbolArtSize Value { get; set; }

        public string Text { get; set; }
    }
}
