using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSAE.Core.SAML;
using OpenSAE.Services;
using System;
using System.Collections.Generic;
using System.IO;
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
            set => SetProperty(ref _currentSymbolArt, value);
        }

        /// <summary>
        /// Currently selected item (group, layer, SA root)
        /// </summary>
        public SymbolArtItemModel? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand OpenFileCommand { get; }

        public ICommand ExitCommand { get; }

        public AppModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            OpenFileCommand = new RelayCommand(OpenFile_Implementation);
            ExitCommand = new RelayCommand(() => ExitRequested?.Invoke(this, EventArgs.Empty));
        }

        public bool RequestExit()
        {
            return true;
        }

        private void OpenFile_Implementation()
        {
            string? filename = _dialogService.BrowseOpenFile("Open existing symbol art file", "SAML symbol art (*.saml)|*.saml");

            if (filename == null)
                return;

            try
            {
                using var fs = File.OpenRead(filename);
                CurrentSymbolArt = new SymbolArtModel(SamlLoader.LoadFromStream(fs));
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Error opening file", "Unable to open the selected symbol art file.", ex);
            }
        }
    }
}
