using OpenSAE.Core;

namespace OpenSAE.Models
{
    public class SymbolArtSoundEffectOptionModel
    {
        public SymbolArtSoundEffectOptionModel(SymbolArtSoundEffect value, string text)
        {
            Value = value;
            Text = text;
        }

        public SymbolArtSoundEffect Value { get; set; }

        public string Text { get; set; }
    }
}
