using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Core.SAR
{
    /// <summary>
    /// Basic decompression-only implementation of PRS
    /// </summary>
    internal static class PrsDecompressor
    {
        public static byte[] Decompress(byte[] input)
        {
            using var msInput = new MemoryStream(input);
            using var msOutput = new MemoryStream();

            Decompress(msInput, msOutput);

            return msOutput.ToArray();
        }

        public static void Decompress(Stream input, Stream output)
        {
            using var br = new BinaryReader(input, Encoding.Default, true);
            using var bw = new BinaryWriter(output, Encoding.Default, true);

            int bitPos = 0;
            byte currentByte = 0;

            while (true)
            {
                if (GetFlag(ref bitPos, ref currentByte, input))
                {
                    // literal
                    bw.Write(br.ReadByte());
                    continue;
                }

                int offset = 0, size = 0;

                if (GetFlag(ref bitPos, ref currentByte, input))
                {
                    offset = br.ReadUInt16();
                    if (offset == 0)
                    {
                        break;
                    }

                    size = offset & 7;
                    offset = (offset >> 3) | -0x2000;
                    
                    if (size == 0)
                    {
                        size = br.ReadByte() + 10;
                    }
                    else
                    {
                        size += 2;
                    }
                }
                else
                {
                    var flag = GetFlag(ref bitPos, ref currentByte, input) ? 1 : 0;
                    size = GetFlag(ref bitPos, ref currentByte, input) ? 1 : 0;
                    size = (size | (flag << 1)) + 2;
                    offset = br.ReadByte() | -0x100;
                }

                for (int i = 0; i < size; i++)
                {
                    if (offset > 0)
                    {
                        throw new Exception("Incorrect offset");
                    }

                    bw.Seek(offset, SeekOrigin.Current);
                    byte newByte = output.ReadByteSafe();
                    bw.Seek(-1, SeekOrigin.Current);
                    bw.Seek(-offset, SeekOrigin.Current);
                    bw.Write(newByte);
                }
            }
        }

        private static bool GetFlag(ref int bitPos, ref byte currentByte, Stream source)
        {
            if (bitPos == 0)
            {
                currentByte = source.ReadByteSafe();
                bitPos = 8;
            }

            int flag = currentByte & 1;
            currentByte >>= 1;
            bitPos--;
            
            return flag != 0;
        }
    }
}
