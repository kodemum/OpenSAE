using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OpenSAE.Models.FileBrowser
{
    public class FileBrowserModel : ObservableObject
    {
        private string? _rootPath;
        private ObservableCollection<FileModel>? _files;
        private BitmapSymbolArtRenderer _renderer;

        public string? RootPath
        {
            get => _rootPath;
            set
            {
                if (SetProperty(ref _rootPath, value))
                {
                    Files = value == null ? null : new ObservableCollection<FileModel>(Directory.GetFiles(value).Select(x => new FileModel(x, _renderer)));
                }
            }
        }

        public ObservableCollection<FileModel>? Files
        {
            get => _files;
            set => SetProperty(ref _files, value);
        }

        public FileBrowserModel()
        {
            _renderer = new BitmapSymbolArtRenderer();
            RootPath = null;
        }
    }
}
