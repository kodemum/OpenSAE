namespace OpenSAE.Core.SAR
{
    public enum SarHeaderFlags : byte
    {
        None = 0,
        Uncompressed = 0x4,
        Compressed = 0x84
    }
}
