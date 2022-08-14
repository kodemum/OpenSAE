using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenSAE.Models.FileBrowser
{
    public class FileModel : ObservableObject
    {
        private string _path;
        private string? _loadError;
        private SymbolArtModel? _sa;

        public FileModel(string path)
        {
            _path = path;

            Task.Run(TryLoad);
        }

        public string FileName => Path.GetFileName(_path);

        public string FullPath => _path;

        public string? LoadError
        {
            get => _loadError;
            set => SetProperty(ref _loadError, value);
        }

        public SymbolArtModel? SymbolArt => _sa;

        public string? Name => _loadError != null ? "[load error]" : _sa?.Name;

        private void TryLoad()
        {
            try
            {
                _sa = new SymbolArtModel(new DummyUndoModel(), _path);
                OnPropertyChanged(nameof(SymbolArt));
            }
            catch (Exception ex)
            {
                _loadError = ex.Message;
            }
            finally
            {
                OnPropertyChanged(nameof(Name));
            }
        }
    }
}
