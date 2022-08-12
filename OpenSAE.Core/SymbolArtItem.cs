using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace OpenSAE.Core
{
    [XmlInclude(typeof(SymbolArtLayer))]
    [XmlInclude(typeof(SymbolArtGroup))]
    [XmlInclude(typeof(SymbolArt))]
    [XmlInclude(typeof(SymbolArtBitmapImageLayer))]
    public abstract class SymbolArtItem
    {
        public string? Name { get; set; }

        public bool Visible { get; set; }

        public string Serialize()
        {
            var stringBuilder = new StringBuilder();
            using var xmlWriter = XmlWriter.Create(stringBuilder);

            new XmlSerializer(typeof(SymbolArtItem)).Serialize(xmlWriter, this);

            xmlWriter.Close();

            return stringBuilder.ToString();
        }

        public static SymbolArtItem Deserialize(string input)
        {
            using var textReader = new StringReader(input);
            using var xmlReader = XmlReader.Create(textReader);

            return (SymbolArtItem?)new XmlSerializer(typeof(SymbolArtItem)).Deserialize(xmlReader) 
                ?? throw new Exception("Unable to deserialize symbol art item");
        }
    }
}
