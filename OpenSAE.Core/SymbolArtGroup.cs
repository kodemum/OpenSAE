using System.Xml.Serialization;

namespace OpenSAE.Core
{
    public class SymbolArtGroup : SymbolArtItem
    {
        public List<SymbolArtItem> Children { get; set; }
            = new();

        public void RemoveAllEmpty()
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                SymbolArtItem? child = Children[i];
                
                if (child is SymbolArtGroup subGroup)
                {
                    subGroup.RemoveAllEmpty();

                    if (!subGroup.Visible || subGroup.Children.Count == 0)
                    {
                        Children.RemoveAt(i);
                    }
                }
                else
                {
                    if (!child.Visible)
                    {
                        Children.RemoveAt(i);
                    }
                }
            }
        }
    }
}
