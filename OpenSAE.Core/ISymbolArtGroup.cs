namespace OpenSAE.Core
{
    public interface ISymbolArtGroup
    {
        string? Name { get; set; }

        bool Visible { get; set; }

        List<SymbolArtItem> Children { get; set; }
    }
}
