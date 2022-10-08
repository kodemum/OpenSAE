using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSAE.Core;
using OpenSAE.Properties;
using OpenSAE.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace OpenSAE.Models.FileBrowser
{
    public class BackupManagerModel : ObservableObject
    {
        private string? _psoPath;
        private readonly IDialogService _dialogService;
        private string? _selectedUser;
        private string _backupPath;
        private List<string> _users = new();

        public BackupManagerModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            _backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OpenSAE", "Backup");
            Directory.CreateDirectory(_backupPath);

            BrowseCommand = new RelayCommand(BrowseCommand_Implementation);
            DeleteCommand = new RelayCommand<string>(DeleteCommand_Implementation, DeleteCommand_CanRun);
            BackupCommand = new RelayCommand<string>(BackupCommand_Implementation, BackupCommand_CanRun);
            ImportCommand = new RelayCommand(ImportCommand_Implementation, ImportCommand_CanRun);

            ImportSymbolArts = new FileBrowserModel(_dialogService);
            CacheSymbolArts = new FileBrowserModel(_dialogService);
            UserSymbolArts = new FileBrowserModel(_dialogService);
            BackupSymbolArts = new FileBrowserModel(_dialogService)
            {
                RootPath = _backupPath
            };

            if (string.IsNullOrEmpty(Settings.Default.PsoDirectory))
            {
                string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SEGA", "PHANTASYSTARONLINE2_NA_STEAM");

                if (Directory.Exists(defaultPath))
                {
                    PsoPath = defaultPath;
                }
            }
            else
            {
                PsoPath = Settings.Default.PsoDirectory;
            }

            foreach (var model in new FileBrowserModel[] { ImportSymbolArts, CacheSymbolArts, UserSymbolArts, BackupSymbolArts })
            {
                model.PropertyChanged += (_, __) =>
                {
                    DeleteCommand.NotifyCanExecuteChanged();
                    BackupCommand.NotifyCanExecuteChanged();
                };
            }

            BackupSymbolArts.PropertyChanged += (_, __) => ImportCommand.NotifyCanExecuteChanged();
        }

        public ICommand BrowseCommand { get; }

        public IRelayCommand DeleteCommand { get; }

        public IRelayCommand BackupCommand { get; }

        public IRelayCommand ImportCommand { get; }

        public string? PsoPath
        {
            get => _psoPath;
            set
            {
                if (SetProperty(ref _psoPath, value))
                {
                    OnPropertyChanged(nameof(ImportSymbolArts));
                    OnPropertyChanged(nameof(CacheSymbolArts));
                    Settings.Default.PsoDirectory = value;

                    LoadSymbolArts();
                }
            }
        }

        public string? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    LoadUserSymbolArts();
                }
            }
        }

        public List<string> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public FileBrowserModel ImportSymbolArts { get; }

        public FileBrowserModel CacheSymbolArts { get; }

        public FileBrowserModel UserSymbolArts { get; }

        public FileBrowserModel BackupSymbolArts { get; }

        public bool ShowAll
        {
            get => ImportSymbolArts.ShowAll;
            set
            {
                ImportSymbolArts.ShowAll = value;
                CacheSymbolArts.ShowAll = value;
                UserSymbolArts.ShowAll = value;
                BackupSymbolArts.ShowAll = value;
            }
        }

        public bool ShowOnlyAllianceFlags
        {
            get => ImportSymbolArts.ShowOnlyAllianceFlags;
            set
            {
                ImportSymbolArts.ShowOnlyAllianceFlags = value;
                CacheSymbolArts.ShowOnlyAllianceFlags = value;
                UserSymbolArts.ShowOnlyAllianceFlags = value;
                BackupSymbolArts.ShowOnlyAllianceFlags = value;
            }
        }

        public bool ShowOnlySymbolArts
        {
            get => ImportSymbolArts.ShowOnlySymbolArts;
            set
            {
                ImportSymbolArts.ShowOnlySymbolArts = value;
                CacheSymbolArts.ShowOnlySymbolArts = value;
                UserSymbolArts.ShowOnlySymbolArts = value;
                BackupSymbolArts.ShowOnlySymbolArts = value;
            }
        }

        private bool ImportCommand_CanRun()
        {
            return BackupSymbolArts.SelectedFile != null;
        }

        private void ImportCommand_Implementation()
        {
            if (BackupSymbolArts.SelectedFile != null)
            {
                try
                {
                    string newFilename = $"{PathUtil.SanitizeFilename(BackupSymbolArts.SelectedFile.Name!)}.{Path.GetExtension(BackupSymbolArts.SelectedFile.FileName)}";
                    string newPath = Path.Combine(ImportSymbolArts.RootPath!, newFilename);

                    if (File.Exists(newPath))
                    {
                        if (!_dialogService.ShowConfirmation("Import symbol art already exists", $"File \"{newFilename}\" already exists in the import directory - overwrite?"))
                            return;
                    }

                    File.Copy(BackupSymbolArts.SelectedFile.FullPath, newPath, true);

                    ImportSymbolArts.Files?.Add(new FileModel(newPath));
                }
                catch (Exception ex)
                {
                    _dialogService.ShowErrorMessage("Error importing symbol art", "An error occurred while trying to copy the symbol art to the import directory", ex);
                }
            }
        }

        private bool BackupCommand_CanRun(string? target)
        {
            var model = GetModel(target);

            return model?.SelectedFile != null;
        }

        private void BackupCommand_Implementation(string? target)
        {
            var model = GetModel(target);

            if (model?.SelectedFile != null)
            {
                try
                {
                    string newFilename = $"{PathUtil.SanitizeFilename(model.SelectedFile.Name!)}.{model.SelectedFile.FileName}";
                    string newPath = Path.Combine(_backupPath, newFilename);

                    if (File.Exists(newPath))
                    {
                        if (!_dialogService.ShowConfirmation("Backup already exists", $"File \"{newFilename}\" already exists in the backup directory - overwrite?"))
                            return;
                    }

                    File.Copy(model.SelectedFile.FullPath, newPath, true);

                    BackupSymbolArts.Files?.Add(new FileModel(newPath));
                }
                catch (Exception ex)
                {
                    _dialogService.ShowErrorMessage("Error backing up symbol art", "An error occurred while trying to copy the symbol art to the backup directory", ex);
                }
            }
        }

        private bool DeleteCommand_CanRun(string? target)
        {
            var model = GetModel(target);

            return model?.SelectedFile != null;
        }

        private void DeleteCommand_Implementation(string? target)
        {
            var model = GetModel(target);

            if (model is not null)
            {
                try
                {
                    model.DeleteSelectedFile();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowErrorMessage("Error deletingn symbol art", "An error occurred while trying to delete the symbol art", ex);
                }
            }
        }

        private void LoadSymbolArts()
        {
            ImportSymbolArts.RootPath = string.IsNullOrEmpty(PsoPath) ? null : Path.Combine(PsoPath, "symbolarts", "import");
            CacheSymbolArts.RootPath = string.IsNullOrEmpty(PsoPath) ? null : Path.Combine(PsoPath, "symbolarts", "cache");

            Users = new List<string>();
            SelectedUser = null;

            if (!string.IsNullOrEmpty(PsoPath))
            {
                string userPath = Path.Combine(PsoPath, "symbolarts", "user");

                if (Directory.Exists(userPath))
                {
                    Users = Directory.GetDirectories(userPath).Select(x => Path.GetFileName(x)).ToList();
                    SelectedUser = Users.FirstOrDefault();
                }
            }
        }

        private void LoadUserSymbolArts()
        {
            if (!string.IsNullOrEmpty(PsoPath) && !string.IsNullOrEmpty(SelectedUser))
            {
                var path = Path.Combine(PsoPath, "symbolarts", "user", SelectedUser);

                UserSymbolArts.RootPath = Directory.Exists(path) ? path : null;
            }
            else
            {
                UserSymbolArts.RootPath = null;
            }
        }

        private FileBrowserModel? GetModel(string? target)
        {
            switch (target)
            {
                case "import":
                    return ImportSymbolArts;

                case "cache":
                    return CacheSymbolArts;

                case "user":
                    return UserSymbolArts;

                case "backup":
                    return BackupSymbolArts;

                default:
                    return null;
            }
        }

        private void BrowseCommand_Implementation()
        {
            string? dirName = _dialogService.BrowseOpenDirectory("Select PSO2 user data directory (in My Documents)", PsoPath);

            if (dirName != null)
            {
                PsoPath = dirName;
            }
        }
    }
}
