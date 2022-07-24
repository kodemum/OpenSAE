using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenSAE.Core.SAML
{
    /// <summary>
    /// Implementation of the SAML symbol art file format whose origins are unknown but probably
    /// was made by the creator of the old japanesese symbol art editor.
    /// </summary>
    public class SamlFileFormat : ISymbolArtFileFormat<SamlSymbolFile>
    {
        private int _loadLayerIndex;

        public string Name => "SAML";

        public SymbolArt LoadFromStream(Stream inputStream)
        {
            return ToSymbolArt(LoadImplementationFromStream(inputStream));
        }

        /// <inheritdoc />
        public SymbolArt ToSymbolArt(SamlSymbolFile input)
        {
            _loadLayerIndex = 0;

            return new SymbolArt()
            {
                Name = input.Name,
                AuthorId = input.AuthorId,
                FileFormat = SymbolArtFileFormat.SAML,
                Height = input.Height,
                Width = input.Width,
                Sound = input.Sound,
                Visible = input.Visible,
                Children = input.Children.Select(ConvertItem).ToList()
            };
        }

        public SymbolArtItem ConvertItem(SamlItem item)
        {
            if (item is SamlGroup group)
            {
                return new SymbolArtGroup()
                {
                    Name = group.Name,
                    Visible = group.Visible,
                    Children = group.Children.Select(ConvertItem).ToList()
                };
            }
            else if (item is SamlLayer layer)
            {
                return new SymbolArtLayer()
                {
                    Index = _loadLayerIndex++,
                    Name = layer.Name,
                    Visible = layer.Visible,
                    Alpha = layer.Alpha,
                    Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(layer.Color),
                    Vertex1 = new System.Windows.Point(layer.Ltx, layer.Lty),
                    Vertex2 = new System.Windows.Point(layer.Lbx, layer.Lby),
                    Vertex3 = new System.Windows.Point(layer.Rbx, layer.Rby),
                    Vertex4 = new System.Windows.Point(layer.Rtx, layer.Rty),
                    SymbolId = layer.Type
                };
            }
            else
            {
                throw new Exception($"Unknown type {item.GetType().Name} encountered in SamlItem");
            }
        }

        public SamlItem ConvertItemBack(SymbolArtItem item)
        {
            if (item is SymbolArtGroup group)
            {
                return new SamlGroup()
                {
                    Name = group.Name,
                    Visible = group.Visible,
                    Children = group.Children.Select(ConvertItemBack).ToList()
                };
            }
            else if (item is SymbolArtLayer layer)
            {
                return new SamlLayer()
                {
                    Name = layer.Name,
                    Alpha = layer.Alpha,
                    Visible = layer.Visible,
                    Color = string.Format("#{0:x2}{1:x2}{2:x2}", layer.Color.R, layer.Color.G, layer.Color.B),
                    Type = layer.SymbolId,
                    Ltx = (short)Math.Round(layer.Vertex1.X),
                    Lty = (short)Math.Round(layer.Vertex1.Y),
                    Lbx = (short)Math.Round(layer.Vertex2.X),
                    Lby = (short)Math.Round(layer.Vertex2.Y),
                    Rbx = (short)Math.Round(layer.Vertex3.X),
                    Rby = (short)Math.Round(layer.Vertex3.Y),
                    Rtx = (short)Math.Round(layer.Vertex4.X),
                    Rty = (short)Math.Round(layer.Vertex4.Y)
                };
            }
            else
            {
                throw new Exception($"Unknown type {item.GetType().Name} encountered in SymbolArtItem");
            }
        }

        /// <inheritdoc />
        public SamlSymbolFile FromSymbolArt(SymbolArt item)
        {
            return new SamlSymbolFile()
            {
                Name = item.Name,
                AuthorId = item.AuthorId,
                Height = item.Height,
                Width = item.Width,
                Sound = item.Sound,
                Version = 4,
                Visible = item.Visible,
                Children = item.Children.Select(ConvertItemBack).ToList()
            };
        }

        /// <inheritdoc />
        public SamlSymbolFile LoadImplementationFromStream(Stream inputStream)
        {
            XmlSerializer xs = new(typeof(SamlSymbolFile));

            return (SamlSymbolFile?)xs.Deserialize(inputStream) ?? throw new NullReferenceException();
        }

        public void SaveToStream(SymbolArt item, Stream output)
        {
            SamlSymbolFile samlFile = FromSymbolArt(item);

            WriteImplementationToStream(samlFile, output);
        }

        public void WriteImplementationToStream(SamlSymbolFile item, Stream outputStream)
        {
            XmlSerializer xs = new(typeof(SamlSymbolFile));

            xs.Serialize(outputStream, item);
        }
    }
}
