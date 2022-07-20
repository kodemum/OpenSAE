using OpenSAE.Core;
using System.Windows;
using System.Windows.Media;

namespace OpenSAE.Models
{
    /// <summary>
    /// Interface representing a symbol art item that can be edited
    /// </summary>
    public interface ISymbolArtItem
    {
        bool IsVisible { get; }

        string? Name { get; set; }

        Point[] Vertices { get; }

        Point Position { get; set; }

        Point Vertex1 { get; }

        Point Vertex2 { get; }

        Point Vertex3 { get; }

        Point Vertex4 { get; }

        bool Visible { get; set; }

        double Alpha { get; set; }

        Color Color { get; set; }

        void SetVertex(int vertexIndex, Point point);

        void Rotate(double angle);

        void TemporaryRotate(double angle);

        void CommitRotate();
    }
}