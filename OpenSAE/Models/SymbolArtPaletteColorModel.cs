using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;

namespace OpenSAE.Models
{
    public class SymbolArtPaletteColorModel : ObservableObject
    {
        private readonly IUndoModel _undoModel;

        public SymbolArtPaletteColorModel(IUndoModel undoModel, SymbolArtLayerModel layer)
        {
            Layers.Add(layer);
            _undoModel = undoModel;
        }

        public List<SymbolArtLayerModel> Layers { get; } 
            = new();

        public Color Color
        {
            get => Layers[0].Color;
            set 
            {
                if (value != Color)
                {
                    using var scope = _undoModel.StartAggregateScope(
                        "Change palette color", 
                        this, 
                        nameof(Color), 
                        () => OnPropertyChanged(nameof(Color)), 
                        () => OnPropertyChanged(nameof(Color))
                    );

                    Layers.ForEach(x => x.Color = value);

                    OnPropertyChanged();
                }
            }
        }
    }
}
