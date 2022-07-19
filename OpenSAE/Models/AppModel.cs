using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSAE.Properties;
using OpenSAE.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenSAE.Models
{
    /// <summary>
    /// Main model for the app
    /// </summary>
    public class AppModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private SymbolArtModel? _currentSymbolArt;
        private SymbolArtItemModel? _selectedItem;

        public event EventHandler? ExitRequested;

        /// <summary>
        /// Currently opened Symbol Art
        /// </summary>
        public SymbolArtModel? CurrentSymbolArt
        {
            get => _currentSymbolArt;
            set
            {
                if (SetProperty(ref _currentSymbolArt, value))
                {
                    OnPropertyChanged(nameof(AppTitle));
                    SaveCommand.NotifyCanExecuteChanged();
                    SaveAsCommand.NotifyCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Currently selected item (group, layer, SA root)
        /// </summary>
        public SymbolArtItemModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    CurrentItemCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string AppTitle
        {
            get
            {
                if (CurrentSymbolArt != null)
                {
                    return $"{CurrentSymbolArt.Name} - OpenSAE Symbol Art Editor";
                }
                else
                {
                    return "OpenSAE Symbol Art Editor";
                }
            }
        }

        public ICommand NewFileCommand { get; }

        public ICommand OpenFileCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand SaveAsCommand { get; }

        public RelayCommand<string> CurrentItemCommand { get; }

        public ICommand ExitCommand { get; }

        public AppModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            RecentFiles = Settings.Default.RecentFiles != null ? new ObservableCollection<string>(Settings.Default.RecentFiles.ToEnumerable()!) : new ObservableCollection<string>();

            OpenFileCommand = new RelayCommand<string>(OpenFile_Implementation);
            NewFileCommand = new RelayCommand(NewFile_Implementation);
            ExitCommand = new RelayCommand(() => ExitRequested?.Invoke(this, EventArgs.Empty));
            SaveCommand = new RelayCommand(Save_Implementation, () => CurrentSymbolArt != null);
            SaveAsCommand = new RelayCommand(SaveAs_Implementation, () => CurrentSymbolArt != null);

            CurrentItemCommand = new RelayCommand<string>(CurrentItemActionCommand_Implementation, (arg) => SelectedItem != null);
        }

        private void CurrentItemActionCommand_Implementation(string? operation)
        {
            if (SelectedItem == null)
                return;

            switch (operation)
            {
                case "toggleVisibility":
                    SelectedItem.Visible = !SelectedItem.Visible;
                    break;

                case "deselect":
                    SelectedItem = null;
                    break;

                case "delete":
                    SelectedItem.Delete();
                    SelectedItem = null;
                    break;

                case "duplicate":
                    if (SelectedItem.Parent == null)
                        return;

                    var duplicate = SelectedItem.Duplicate(SelectedItem.Parent);

                    var currentIndex = SelectedItem.Parent.Children.IndexOf(SelectedItem);

                    SelectedItem.Parent.Children.Insert(currentIndex, duplicate);
                    break;
            }
        }

        private void NewFile_Implementation()
        {
            CurrentSymbolArt = new SymbolArtModel();
            SelectedItem = CurrentSymbolArt;
        }

        public bool RequestExit()
        {
            Settings.Default.RecentFiles = RecentFiles.ToStringCollection();

            return true;
        }

        public ObservableCollection<string> RecentFiles { get; }

        private void OpenFile_Implementation(string? filename)
        {
            if (filename == null)
            {
                filename = _dialogService.BrowseOpenFile("Open existing symbol art file", "SAML symbol art (*.saml)|*.saml");

                if (filename == null)
                    return;
            }

            try
            {
                CurrentSymbolArt = new SymbolArtModel(filename);
                SelectedItem = CurrentSymbolArt;
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Error opening file", "Unable to open the selected symbol art file.", ex);
            }

            int recentPos = RecentFiles.IndexOf(filename);

            if (recentPos != -1)
            {
                RecentFiles.RemoveAt(recentPos);
            }

            RecentFiles.Insert(0, filename);
        }

        private void Save_Implementation()
        {
            if (CurrentSymbolArt == null)
                return;

            if (string.IsNullOrEmpty(CurrentSymbolArt.FileName))
            {
                SaveAs_Implementation();
            }
            else
            {
                try
                {
                    CurrentSymbolArt.Save();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowErrorMessage("Error saving file", "Unable to save the symbol art file.", ex);
                }
            }
        }

        private void SaveAs_Implementation()
        {
            if (CurrentSymbolArt == null)
                return;

            string? filename = _dialogService.BrowseSaveFile("Save symbol art file", "SAML symbol art (*.saml)|*.saml", CurrentSymbolArt.FileName);

            if (filename == null)
                return;

            try
            {
                CurrentSymbolArt.SaveAs(filename, Core.SymbolArtFileFormat.SAML);
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Error saving file", "Unable to save the symbol art file.", ex);
            }
        }
    }
}
