using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpenSAE.Models
{
    public class SymbolArtPaletteModel : ObservableObject
    {
        private readonly SymbolArtModel _sa;

        public ObservableCollection<SymbolArtPaletteColorModel> Colors { get; }

        public SymbolArtPaletteModel(IUndoModel undoModel, SymbolArtModel sa)
        {
            _sa = sa;

            Dictionary<Color,SymbolArtPaletteColorModel> colors = new();

            foreach (var layer in sa.GetAllLayers())
            {
                if (colors.TryGetValue(layer.Color, out var color))
                {
                    color.Layers.Add(layer);
                }
                else
                {
                    colors.Add(layer.Color, new SymbolArtPaletteColorModel(undoModel, layer));
                }
            }

            Colors = new ObservableCollection<SymbolArtPaletteColorModel>(colors.Values);
        }
    }
}
