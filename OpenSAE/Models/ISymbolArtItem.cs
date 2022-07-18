using OpenSAE.Core;

namespace OpenSAE.Models
{
    /// <summary>
    /// Interface representing a symbol art item that can be edited
    /// </summary>
    public interface ISymbolArtItem
    {
        bool IsVisible { get; }

        string? Name { get; set; }

        SymbolArtPoint[] Vertices { get; }

        SymbolArtPoint Position { get; set; }

        SymbolArtPoint Vertex1 { get; }

        SymbolArtPoint Vertex2 { get; }

        SymbolArtPoint Vertex3 { get; }

        SymbolArtPoint Vertex4 { get; }

        bool Visible { get; set; }

        void SetVertex(int vertexIndex, SymbolArtPoint point);
    }
}