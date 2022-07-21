using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Core
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads a single byte from the stream; throws an exception if at end of stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException">Thrown if the stream is at the end</exception>
        public static byte ReadByteSafe(this Stream stream)
        {
            int result = stream.ReadByte();

            if (result == -1)
            {
                throw new EndOfStreamException();
            }

            return (byte)result;
        }

        /// <summary>
        /// Reads the specified amount of bytes from the stream into a new array.
        /// </summary>
        /// <param name="count">Number of bytes to read from stream</param>
        public static byte[] ReadBytes(this Stream stream, int count)
        {
            byte[] result = new byte[count];

            int read = 0;

            while (read < count)
            {
                read += stream.Read(result, read, count - read);
            }

            return result;
        }
    }
}
