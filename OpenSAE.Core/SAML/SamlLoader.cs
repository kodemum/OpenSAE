using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenSAE.Core.SAML
{
    public class SamlLoader
    {
        public static SymbolArt LoadFromStream(Stream inputStream)
        {
            XmlSerializer xs = new(typeof(SymbolArt));

            var symbolArt = (SymbolArt?)xs.Deserialize(inputStream) ?? throw new NullReferenceException();

            symbolArt.FileFormat = SymbolArtFileFormat.SAML;

            return symbolArt;
        }

        public static void SaveToStream(Stream outputStream, SymbolArt symbolArt)
        {
            XmlSerializer xs = new(typeof(SymbolArt));

            xs.Serialize(outputStream, symbolArt);
        }
    }
}
