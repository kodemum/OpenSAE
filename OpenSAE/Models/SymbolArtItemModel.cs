using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace OpenSAE.Models
{
    public abstract class SymbolArtItemModel : ObservableObject
    {
        public abstract string? Name { get; set; }

        public abstract bool Visible { get; set; }

        public abstract bool IsVisible { get; }

        public ObservableCollection<SymbolArtItemModel> Children { get; }
            = new();
    }
}
