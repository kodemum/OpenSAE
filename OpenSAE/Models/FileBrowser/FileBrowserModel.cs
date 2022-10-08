using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSAE.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace OpenSAE.Models.FileBrowser
{
    public class FileBrowserModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly Action<string> _openAction;
        private readonly Action<string> _openInNewWindowAction;
        
        private string? _rootPath;
        private ObservableCollection<FileModel>? _files;
        private FileModel? _selectedFile;
        private bool _showAll;
        private bool _showOnlyAllianceFlags;
        private bool _showOnlySymbolArts;

        public bool ShowAll
        {
            get => _showAll;
            set
            {
                if (SetProperty(ref _showAll, value))
                {
                    FilesView.Refresh();
                }
            }
        }

        public bool ShowOnlyAllianceFlags
        {
            get => _showOnlyAllianceFlags;
            set
            {
                if (SetProperty(ref _showOnlyAllianceFlags, value))
                {
                    FilesView.Refresh();
                }
            }
        }

        public bool ShowOnlySymbolArts
        {
            get => _showOnlySymbolArts;
            set
            {
                if (SetProperty(ref _showOnlySymbolArts, value))
                {
                    FilesView.Refresh();
                }
            }
        }

        public Action<string?> OnPathChangeAction { get; set; }

        public string? RootPath
        {
            get => _rootPath;
            set
            {
                if (SetProperty(ref _rootPath, value))
                {
                    OnPathChangeAction?.Invoke(value);
                    LoadFiles();
                }
            }
        }

        public ObservableCollection<FileModel>? Files
        {
            get => _files;
            set
            {
                if (SetProperty(ref _files, value))
                {
                    FilesView = CollectionViewSource.GetDefaultView(Files);
                    FilesView.Filter = Filter;
                    OnPropertyChanged(nameof(FilesView));
                }
            }
        }

        private bool Filter(object obj)
        {
            if (obj is FileModel fileModel)
            {
                if (_showAll)
                {
                    return true;
                }

                if (fileModel.SymbolArt?.Size == Core.SymbolArtSize.AllianceLogo)
                {
                    return _showOnlyAllianceFlags;
                }
                else
                {
                    return _showOnlySymbolArts;
                }
            }
            else
            {
                return true;
            }
        }

        public ICollectionView FilesView { get; private set; }

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

        public FileBrowserModel(IDialogService dialogService)
            : this(dialogService, (_) => { }, (_) => { })
        {
        }

        public FileBrowserModel(IDialogService dialogService, Action<string> openAction, Action<string> openInNewWindowAction)
        {
            _dialogService = dialogService;
            _openAction = openAction;
            _openInNewWindowAction = openInNewWindowAction;
            _showAll = true;

            BrowseCommand = new RelayCommand(BrowseCommand_Implementation);
            OpenCommand = new RelayCommand<string>(OpenCommand_Implementation, (_) => SelectedFile != null);
        }

        public void DeleteSelectedFile()
        {
            if (SelectedFile != null)
            {
                File.Delete(SelectedFile.FullPath);
                Files?.Remove(SelectedFile);
                SelectedFile = null;
            }
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

        public void LoadFiles()
        {
            try
            {
                if (_rootPath != null && Directory.Exists(_rootPath))
                {
                    Files = new ObservableCollection<FileModel>(Directory.GetFiles(_rootPath)
                        .Where(x => Path.GetExtension(x).ToLower() == ".sar" || Path.GetExtension(x).ToLower() == ".saml")
                        .Select(x => new FileModel(x)));
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
