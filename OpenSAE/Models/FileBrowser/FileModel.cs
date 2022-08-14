using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Windows.Media;

namespace OpenSAE.Models.FileBrowser
{
    public class FileModel : ObservableObject
    {
        private string _path;
        private string? _loadError;
        private bool _isLoaded;
        private SymbolArtModel? _sa;
        private BitmapSymbolArtRenderer _renderer;
        private ImageSource? _saImage;

        public FileModel(string path, BitmapSymbolArtRenderer renderer)
        {
            _path = path;
            _renderer = renderer;
        }

        public string FileName => Path.GetFileName(_path);

        public string? LoadError
        {
            get => _loadError;
            set => SetProperty(ref _loadError, value);
        }

        public SymbolArtModel? SymbolArt => _sa;

        public string? Name
        {
            get
            {
                if (!_isLoaded)
                {
                    TryLoad();
                }

                return _sa?.Name;
            }
        }

        public ImageSource? SymbolImage
        {
            get
            {
                if (!_isLoaded)
                {
                    TryLoad();
                }

                if (_saImage == null)
                {
                    _saImage = _sa != null ? _renderer.RenderToBitmapTarget(_sa, _sa.Width, _sa.Height) : null;
                }

                return _saImage;
            }
        }

        private void TryLoad()
        {
            try
            {
                _sa = new SymbolArtModel(new DummyUndoModel(), _path);
            }
            catch (Exception ex)
            {
                _loadError = ex.Message;
            }
            finally
            {
                _isLoaded = true;
            }
        }
    }
}
