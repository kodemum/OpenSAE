using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenSAE.Core.SAR
{
    public class SarFileFormat : ISymbolArtFileFormat<SarSymbolFile>
    {
        private static readonly byte[] _headerMagic 
            = new byte[] { 0x73, 0x61, 0x72 }; // 'sar'

        private static readonly byte[] _encryptionKey 
            = new byte[] { 9, 7, 193, 43 }; //0x09, 0x07, 0xc1, 0x2b

        public string Name => "SAR";

        public SymbolArt ToSymbolArt(SarSymbolFile input)
        {
            // what is up with the sizes?
            int width = input.Width switch
            {
                64 => 32,
                _ => input.Width
            };
            int height = input.Width switch
            {
                193 => 96,
                64 => 32,
                _ => input.Height
            };

            return new SymbolArt()
            {
                Name = input.Name,
                Visible = true,
                AuthorId = input.AuthorId,
                FileFormat = SymbolArtFileFormat.SAR,
                Height = height,
                Width = width,
                Sound = (SymbolArtSoundEffect)input.SoundEffect,
                Children = new List<SymbolArtItem>()
                {
                    new SymbolArtGroup()
                    {
                        Name = "Group",
                        Visible = true,
                        Children = input.Layers.Select((layer, index) => (SymbolArtItem)new SymbolArtLayer()
                        {
                            // SAR files have no name, we'll generate one from the index and selected symbol for display
                            Name = null,
                            Index = index,
                            Alpha = (double)layer.Alpha / 7,
                            Color = SymbolArtColorHelper.ConvertToColor(layer.ColorR, layer.ColorG, layer.ColorB),
                            SymbolId = layer.SymbolId,
                            Visible = !layer.IsHidden,
                            Vertex1 = layer.Vertex1.ToPoint(),
                            Vertex2 = layer.Vertex2.ToPoint(),
                            Vertex3 = layer.Vertex3.ToPoint(),
                            Vertex4 = layer.Vertex4.ToPoint(),
                        }).ToList()
                    }
                }
            };
        }

        public SarSymbolFile FromSymbolArt(SymbolArt input)
        {
            // SAR has no concept of groups and so we must flatten
            // all layers in the input file
            return new SarSymbolFile()
            {
                Width = input.Width switch
                {
                    32 => 64,
                    _ => (byte)input.Width
                },
                Height = input.Width switch
                {
                    32 => 64,
                    193 => 96,
                    192 => 96,
                    _ => (byte)input.Height
                },
                Name = input.Name ?? string.Empty,
                AuthorId = input.AuthorId,
                SoundEffect = (byte)input.Sound,
                Layers = GetAllLayers(input).Select(x => new SarSymbolLayer(x)).ToList()
            };
        }

        private static IEnumerable<SymbolArtLayer> GetAllLayers(ISymbolArtGroup group)
        {
            foreach (var item in group.Children)
            {
                if (item is SymbolArtLayer layer)
                {
                    yield return layer;
                }
                else if (item is ISymbolArtGroup subGroup)
                {
                    foreach (var subLayer in GetAllLayers(subGroup))
                    {
                        yield return subLayer;
                    }
                }
            }
        }

        public SarSymbolFile LoadImplementationFromStream(Stream input)
        {
            if (!IsIdenticalTo(_headerMagic, input.ReadBytes(3)))
                throw new Exception("Magic characters not found - file does not appear to be SAR");

            var flag = (SarHeaderFlags)input.ReadByteSafe();

            if (flag != SarHeaderFlags.Uncompressed && flag != SarHeaderFlags.Compressed)
                throw new Exception($"Unknown SAR header flag {flag}");

            byte[] decryptedContent = DecryptBlowfish(input);

            if (flag == SarHeaderFlags.Compressed)
            {
                // Why the XOR, SEGA?
                for (int i = 0; i < decryptedContent.Length; i++)
                {
                    decryptedContent[i] ^= 0x95;
                }

                decryptedContent = PrsDecompressor.Decompress(decryptedContent);
            }

            return ParseContent(decryptedContent);
        }

        public SymbolArt LoadFromStream(Stream input) 
            => ToSymbolArt(LoadImplementationFromStream(input));

        public void SaveToStream(SymbolArt item, Stream output)
        {
            WriteImplementationToStream(FromSymbolArt(item), output);
        }

        public void WriteImplementationToStream(SarSymbolFile item, Stream outputStream)
        {
            using var writer = new BinaryWriter(outputStream, Encoding.Unicode, true);

            writer.Write(_headerMagic);
            writer.Write((byte)SarHeaderFlags.Uncompressed);

            byte[] content = GenerateContent(item);

            writer.Write(EncryptBlowfish(content));
        }

        private static byte[] GenerateContent(SarSymbolFile item)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((uint)item.AuthorId);
            writer.Write((byte)item.Layers.Count);
            writer.Write((byte)item.Height);
            writer.Write((byte)item.Width);
            writer.Write((byte)item.SoundEffect);

            foreach (var layer in item.Layers)
            {
                WriteVertex(writer, layer.Vertex1);
                WriteVertex(writer, layer.Vertex2);
                WriteVertex(writer, layer.Vertex3);
                WriteVertex(writer, layer.Vertex4);

                writer.Write((uint)layer.Flag1);
                writer.Write((uint)layer.Flag2);
            }

            writer.Write(Encoding.Unicode.GetBytes(item.Name));

            return ms.ToArray();
        }

        private static SarSymbolFile ParseContent(byte[] decryptedContent)
        {
            var reader = new BinaryReader(new MemoryStream(decryptedContent));

            uint authorId = reader.ReadUInt32();
            byte layerCount = reader.ReadByte();
            byte height = reader.ReadByte();
            byte width = reader.ReadByte();
            byte soundEffect = reader.ReadByte();

            SarSymbolFile result = new()
            {
                AuthorId = authorId,
                Height = height,
                Width = width,
                SoundEffect = soundEffect,
            };

            for (int i = 0; i < layerCount; i++)
            {
                var vertex1 = ReadVertex(reader);
                var vertex2 = ReadVertex(reader);
                var vertex3 = ReadVertex(reader);
                var vertex4 = ReadVertex(reader);

                uint flag1 = reader.ReadUInt32();
                uint flag2 = reader.ReadUInt32();

                result.Layers.Add(new SarSymbolLayer(flag1, flag2, vertex1, vertex2, vertex3, vertex4));
            }

            // rest of content should be a string with the symbol art name
            string name = Encoding.Unicode.GetString(decryptedContent, (int)reader.BaseStream.Position, decryptedContent.Length - (int)reader.BaseStream.Position);

            result.Name = name;

            return result;
        }

        private static SarSymbolVertex ReadVertex(BinaryReader reader)
        {
            return new SarSymbolVertex(reader.ReadByte(), reader.ReadByte());
        }

        private static void WriteVertex(BinaryWriter writer, SarSymbolVertex vertex)
        {
            writer.Write((byte)vertex.X);
            writer.Write((byte)vertex.Y);
        }

        private static bool IsIdenticalTo(byte[] targetValue, byte[] actualValue)
        {
            if (targetValue.Length != actualValue.Length)
                return false;

            for (int i = 0; i < targetValue.Length; i++)
            {
                if (targetValue[i] != actualValue[i])
                    return false;
            }

            return true;
        }

        private static byte[] DecryptBlowfish(Stream input)
        {
            long contentLength = input.Length - input.Position;
            byte[] encryptedContent = new byte[(contentLength / 8) * 8];

            int toRead = encryptedContent.Length;
            int read;

            while ((read = input.Read(encryptedContent, 0, toRead)) != 0)
            {
                toRead -= read;
            }

            var engine = new BlowfishEngineLittleEndian();
            engine.Init(false, _encryptionKey);

            byte[] decryptedContent = new byte[contentLength];

            int offset = 0;

            while (true)
            {
                offset += engine.ProcessBlock(encryptedContent, offset, decryptedContent, offset);

                if (offset == encryptedContent.Length)
                    break;
            }

            // For God knows what reason, only full blocks are encrypted (IE there's no padding)
            // and any data at the end that does not fill a block is just left unencrypted ( ﾉ ﾟｰﾟ)ﾉ
            if (encryptedContent.Length < contentLength)
            {
                input.Read(decryptedContent, encryptedContent.Length, (int)contentLength - encryptedContent.Length);
            }

            return decryptedContent;
        }

        private static byte[] EncryptBlowfish(byte[] input)
        {
            long contentLength = input.Length;
            long lengthToEncrypt = (contentLength / 8) * 8;
            
            byte[] encryptedContent = new byte[contentLength];

            var engine = new BlowfishEngineLittleEndian();
            engine.Init(true, _encryptionKey);

            int offset = 0;

            while (true)
            {
                offset += engine.ProcessBlock(input, offset, encryptedContent, offset);

                if (offset == lengthToEncrypt)
                    break;
            }

            // add overflow data unencrypted - see comment in decryption method
            if (lengthToEncrypt < contentLength)
            {
                Array.Copy(input, lengthToEncrypt, encryptedContent, lengthToEncrypt, contentLength - lengthToEncrypt);
            }

            return encryptedContent;
        }
    }
}
