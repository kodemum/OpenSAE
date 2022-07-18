using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenSAE.Models
{
    public abstract class SymbolArtItemModel : ObservableObject
    {
        public abstract string? Name { get; set; }

        public abstract bool Visible { get; set; }

        public abstract bool IsVisible { get; }

        public SymbolArtItemModel? Parent { get; protected set; }

        public virtual void Delete()
        {
            if (Parent != null)
            {
                Parent.Children.Remove(this);
            }
        }

        public IEnumerable<SymbolArtLayerModel> GetAllLayers()
        {
            if (this is SymbolArtLayerModel layer)
            {
                yield return layer;
            }
            else
            {
                foreach (var child in Children)
                {
                    if (child is SymbolArtLayerModel childLayer)
                    {
                        yield return childLayer;
                    }
                    else if (child is SymbolArtGroupModel group)
                    {
                        foreach (var groupLayer in group.GetAllLayers())
                        {
                            yield return groupLayer;
                        }
                    }
                }
            }
        }

        public ObservableCollection<SymbolArtItemModel> Children { get; }
            = new();
    }
}
