using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Core
{
    /// <summary>
    /// Represents a file format that contains a symbol art
    /// </summary>
    public interface ISymbolArtFileFormat
    {
        /// <summary>
        /// Name of the file format
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Loads the symbol art from the specified stream using the file format this instance represents.
        /// </summary>
        /// <param name="input">Stream to read symbol art from</param>
        /// <returns></returns>
        SymbolArt LoadFromStream(Stream input);

        /// <summary>
        /// Saves the specified symbol art to the specified stream, using the file format this instance represents.
        /// </summary>
        /// <param name="item">Symbol to save</param>
        /// <param name="output">Stream to write symbol to</param>
        void SaveToStream(SymbolArt item, Stream output);
    }

    /// <summary>
    /// Represents a file format that contains a symbol art
    /// </summary>
    /// <typeparam name="TImplementation">Type of the implementation-specific class used by the file format</typeparam>
    public interface ISymbolArtFileFormat<TImplementation> : ISymbolArtFileFormat
        where TImplementation : class
    {
        /// <summary>
        /// Convert an instance of <see cref="TImplementation"/> to a symbol art object.
        /// </summary>
        /// <param name="input">Instance to convert</param>
        /// <returns></returns>
        SymbolArt ToSymbolArt(TImplementation input);

        /// <summary>
        /// Converts a symbol art object to the class used by the file format implementation.
        /// </summary>
        /// <param name="input">Symbol art object to convert</param>
        /// <returns></returns>
        TImplementation FromSymbolArt(SymbolArt input);

        /// <summary>
        /// Loads an implementation-specific object representing a symbol art from the specified stream.
        /// </summary>
        /// <param name="inputStream">Stream to load symbol art from</param>
        /// <returns></returns>
        TImplementation LoadImplementationFromStream(Stream inputStream);

        /// <summary>
        /// Writes implementation-specific object representing a symbol art to the specified stream.
        /// </summary>
        /// <param name="outputStream">Stream to write implementation to</param>
        void WriteImplementationToStream(TImplementation item, Stream outputStream);
    }
}
