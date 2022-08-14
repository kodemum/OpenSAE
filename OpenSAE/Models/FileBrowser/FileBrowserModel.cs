using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSAE.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OpenSAE.Models.FileBrowser
{
    public class FileBrowserModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly Action<string> _openAction;
        private readonly Action<string> _openInNewWindowAction;
        private readonly BitmapSymbolArtRenderer _renderer;
        
        private string? _rootPath;
        private ObservableCollection<FileModel>? _files;
        private FileModel? _selectedFile;

        public string? RootPath
        {
            get => _rootPath;
            set
            {
                if (SetProperty(ref _rootPath, value))
                {
                    Properties.Settings.Default.BrowseWindowPath = value;

                    LoadFiles();
                }
            }
        }

        public ObservableCollection<FileModel>? Files
        {
            get => _files;
            set => SetProperty(ref _files, value);
        }

        public FileModel? SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (SetProperty(ref _selectedFile, value))
                {
                    OpenCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public ICommand BrowseCommand { get; }

        public IRelayCommand OpenCommand { get; }

        public FileBrowserModel(IDialogService dialogService, Action<string> openAction, Action<string> openInNewWindowAction)
        {
            _renderer = new BitmapSymbolArtRenderer();
            _dialogService = dialogService;
            _openAction = openAction;
            _openInNewWindowAction = openInNewWindowAction;

            BrowseCommand = new RelayCommand(BrowseCommand_Implementation);
            OpenCommand = new RelayCommand<string>(OpenCommand_Implementation, (_) => SelectedFile != null);
        }

        private void OpenCommand_Implementation(string? obj)
        {
            if (SelectedFile == null)
                return;

            if (obj == "new")
            {
                _openInNewWindowAction.Invoke(SelectedFile.FullPath);
            }
            else
            {
                _openAction.Invoke(SelectedFile.FullPath);
            }
        }

        private void BrowseCommand_Implementation()
        {
            string? dirName = _dialogService.BrowseOpenDirectory("Select folder to browse symbol arts in", RootPath);

            if (dirName != null)
            {
                RootPath = dirName;
            }
        }

        private void LoadFiles()
        {
            try
            {
                if (_rootPath != null && Directory.Exists(_rootPath))
                {
                    Files = new ObservableCollection<FileModel>(Directory.GetFiles(_rootPath)
                        .Where(x => Path.GetExtension(x).ToLower() == ".sar" || Path.GetExtension(x).ToLower() == ".saml")
                        .Select(x => new FileModel(x, _renderer)));
                }
                else
                {
                    Files = new ObservableCollection<FileModel>();
                }
            }
            catch
            {
            }
        }
    }
}
