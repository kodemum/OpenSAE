namespace OpenSAE.Core.SAR
{
    public class SarSymbolFile
    {
        public uint AuthorId { get; set; }

        public byte Height { get; set; }

        public byte Width { get; set; }

        public byte SoundEffect { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public List<SarSymbolLayer> Layers { get; set; }
            = new();
    }
}
