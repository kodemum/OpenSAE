using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;

namespace OpenSAE.Models
{
    public class SymbolArtPaletteColorModel : ObservableObject
    {
        public SymbolArtPaletteColorModel(SymbolArtLayerModel layer)
        {
            Layers.Add(layer);
        }

        public List<SymbolArtLayerModel> Layers { get; } 
            = new();

        public Color Color
        {
            get => Layers[0].Color;
            set 
            {
                Layers.ForEach(x => x.Color = value);
                OnPropertyChanged();
            }
        }
    }
}
