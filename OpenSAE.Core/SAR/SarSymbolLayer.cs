namespace OpenSAE.Core.SAR
{
    public class SarSymbolLayer
    {
        public SarSymbolLayer(uint flag1, uint flag2, SarSymbolVertex vertex1, SarSymbolVertex vertex2, SarSymbolVertex vertex3, SarSymbolVertex vertex4)
        {
            Flag1 = flag1;
            Flag2 = flag2;
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Vertex3 = vertex3;
            Vertex4 = vertex4;
        }

        public SarSymbolLayer(SymbolArtLayer layer)
        {
            Flag1 = 0;
            Flag2 = 0;

            IsHidden = !layer.Visible;
            Alpha = (byte)Math.Round(layer.Alpha * 7);
            ColorR = (byte)(layer.Color.R >> 2);
            ColorG = (byte)(layer.Color.G >> 2);
            ColorB = (byte)(layer.Color.B >> 2);
            SymbolId = (short)layer.SymbolId;

            Vertex1 = SarSymbolVertex.FromPoint(layer.Vertex1);
            Vertex2 = SarSymbolVertex.FromPoint(layer.Vertex2);
            Vertex3 = SarSymbolVertex.FromPoint(layer.Vertex3);
            Vertex4 = SarSymbolVertex.FromPoint(layer.Vertex4);
        }

        public bool IsHidden
        {
            get => (Flag1 & SarFileConstants.Layer_IsHidden) != 0;
            set
            {
                if (value)
                    Flag1 |= SarFileConstants.Layer_IsHidden;
                else
                    Flag1 &= ~SarFileConstants.Layer_IsHidden;
            }
        }

        public short SymbolId
        {
            get => (short)((Flag1 & SarFileConstants.Mask_SymbolId) >> 21);
            set
            {
                Flag1 &= ~SarFileConstants.Mask_SymbolId;
                Flag1 |= (uint)(value << 21);
            }
        }

        public byte Alpha // => (byte)((Flag1 >> 18) & 7);
        {
            get => (byte) ((Flag1 & SarFileConstants.Mask_Alpha) >> 18);
            set
            {
                Flag1 &= ~SarFileConstants.Mask_Alpha;
                Flag1 |= (uint)(value << 18);
            }
        }

        public byte ColorR //=> (byte)((Flag1 >> 0) & 63);
        {
            get => (byte)(Flag1 & SarFileConstants.Mask_ColorR);
            set
            {
                Flag1 &= ~SarFileConstants.Mask_ColorR;
                Flag1 |= value;
            }
        }

        public byte ColorG //=> (byte)((Flag1 >> 6) & 63);
        {
            get => (byte)((Flag1 & SarFileConstants.Mask_ColorG) >> 6);
            set
            {
                Flag1 &= ~SarFileConstants.Mask_ColorG;
                Flag1 |= (uint)(value << 6);
            }
        }

        public byte ColorB //=> (byte)((Flag1 >> 12) & 63);
        {
            get => (byte)((Flag1 & SarFileConstants.Mask_ColorB) >> 12);
            set
            {
                Flag1 &= ~SarFileConstants.Mask_ColorB;
                Flag1 |= (uint)(value << 12);
            }
        }

        public byte ColorX
        {
            get => (byte)(Flag2 & SarFileConstants.Mask_ColorR);
            set
            {
                Flag2 &= ~SarFileConstants.Mask_ColorR;
                Flag2 |= value;
            }
        }

        public byte ColorY
        {
            get => (byte)((Flag2 & SarFileConstants.Mask_ColorG) >> 6);
            set
            {
                Flag2 &= ~SarFileConstants.Mask_ColorG;
                Flag2 |= (uint)(value << 6);
            }
        }

        public byte ColorZ
        {
            get => (byte)((Flag2 & SarFileConstants.Mask_ColorB) >> 12);
            set
            {
                Flag2 &= ~SarFileConstants.Mask_ColorB;
                Flag2 |= (uint)(value << 12);
            }
        }

        public uint Flag1 { get; set; }

        public uint Flag2 { get; set; }

        public SarSymbolVertex Vertex1 { get; set; }

        public SarSymbolVertex Vertex2 { get; set; }

        public SarSymbolVertex Vertex3 { get; set; }

        public SarSymbolVertex Vertex4 { get; set; }
    }
}
