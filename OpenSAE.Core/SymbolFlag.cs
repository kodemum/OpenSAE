namespace OpenSAE.Core
{
    [Flags]
    public enum SymbolFlag
    {
        None = 0,
        HorizontallySymmetric = 1,
        VerticallySymmetric = 2,
        Symmetric = 4,

        FullySymmetric = HorizontallySymmetric | VerticallySymmetric | Symmetric
    }
}
