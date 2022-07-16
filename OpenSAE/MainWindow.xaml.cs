using Microsoft.Win32;
using OpenSAE.Core;
using OpenSAE.Core.SAML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenSAE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SymbolArt _symbol;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile(string filename)
        {
            try
            {
                using var fs = File.OpenRead(filename);

                _symbol = SamlLoader.LoadFromStream(fs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Unable to open selected file: {ex.Message}");
                return;
            }

            RefreshCurrentSymbolArt();
        }

        private void RefreshCurrentSymbolArt()
        {
            model3dGroup.Children.Clear();
            model3dGroup.Children.Add(new AmbientLight(Colors.White));

            Task.Run(() =>
            {
                DrawSymbolArtGroup(_symbol);
            });
        }

        private void DrawSymbolArtGroup(ISymbolArtGroup group)
        {
            for (int i = group.Children.Count - 1; i >= 0; i--)
            {
                if (group.Children[i] is SymbolArtLayer layer)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Draw3d(layer);
                    });
                }
                else if (group.Children[i] is ISymbolArtGroup subGroup)
                {
                    DrawSymbolArtGroup(subGroup);
                }
            }
        }

        private void Draw3d(SymbolArtLayer layer)
        {
            // order:
            // left bottom
            // left top
            // right bottom
            // right top
            var points = new Point3DCollection()
            {
                new Point3D(layer.Lbx, -layer.Lby, 0),
                new Point3D(layer.Ltx, -layer.Lty, 0),
                new Point3D(layer.Rbx, -layer.Rby, 0),
                new Point3D(layer.Rtx, -layer.Rty, 0)
            };

            var uri = SymbolUtil.GetSymbolPackUri(layer.Type);

            if (uri == null)
            {
                return;
            }

            var material = new DiffuseMaterial()
            {
                Brush = new ImageBrush((ImageSource)new ImageSourceConverter().ConvertFromString(uri)!)
                {
                    Opacity = layer.Alpha
                },
                AmbientColor = (Color)ColorConverter.ConvertFromString(layer.Color ?? "#ffffff")
            };

            model3dGroup.Children.Add(new GeometryModel3D()
            {
                Geometry = new MeshGeometry3D()
                {
                    Positions = points,
                    TextureCoordinates = new PointCollection()
                    {
                        new Point(0, 1),
                        new Point(0, 0),
                        new Point(1, 1),
                        new Point(1, 0)
                    },
                    TriangleIndices = new Int32Collection()
                    {
                        0, 2, 1, 
                        2, 3, 1,
                    }
                },
                Material = material,
                BackMaterial = material,
            });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog()
            {
                Filter = "SAML symbol art (*.saml)|*.saml",
                Title = "Open existing symbol art file"
            };

            if (od.ShowDialog() == true)
            {
                OpenFile(od.FileName);
            }
        }
    }
}
