using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSAE.Core;
using OpenSAE.Core.BitmapConverter;
using OpenSAE.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenSAE.Models
{
    internal class BitmapConverterModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly Action<SymbolArtGroup> _openAction;
        private SymbolArtModel? _currentSymbolArt;
        private SymbolArtGroup? _currentGroup;
        private string? _bitmapFilename;
        private string? _errorMessage;

        public SymbolArtModel? CurrentSymbolArt
        {
            get => _currentSymbolArt;
            set
            {
                if (SetProperty(ref _currentSymbolArt, value))
                {
                    OnPropertyChanged(nameof(TooManyLayers));
                }
            }
        }

        public string? BitmapFilename
        {
            get => _bitmapFilename;
            set => SetRefreshProperty(ref _bitmapFilename, value);
        }

        public bool TooManyLayers => CurrentSymbolArt?.LayerCount > 225;

        public BitmapToSymbolArtConverterOptionsViewModel Options { get; }

        public SymbolListModel SymbolsList { get; }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public IRelayCommand BrowseCommand { get; }

        public IRelayCommand AcceptCommand { get; }

        public BitmapConverterModel(IDialogService dialogService, Action<SymbolArtGroup> openAction)
        {
            _dialogService = dialogService;
            _openAction = openAction;
            BrowseCommand = new RelayCommand(BrowseCommand_Implementation);
            AcceptCommand = new RelayCommand(AcceptCommand_Implementation, () => CurrentSymbolArt != null);

            Options = new BitmapToSymbolArtConverterOptionsViewModel();
            Options.PropertyChanged += (_, __) => Reload();
            SymbolsList = new();
        }

        private void AcceptCommand_Implementation()
        {
            if (_currentGroup != null)
            {
                _openAction?.Invoke(_currentGroup);
            }
        }

        private void BrowseCommand_Implementation()
        {
            string? filename = _dialogService.BrowseOpenFile("Open overlay image", AppModel.BitmapFormatFilter);

            if (filename == null)
                return;

            BitmapFilename = filename;
        }

        private bool SetRefreshProperty<T>(ref T prop, T value, [CallerMemberName] string? propertyName = null)
        {
            bool different = SetProperty(ref prop, value, propertyName);

            if (different)
            {
                Reload();
            }

            return different;
        }

        private void Reload()
        {
            if (string.IsNullOrEmpty(BitmapFilename))
            {
                CurrentSymbolArt = null;
            }
            else
            {
                try
                {
                    using var converter = new BitmapToSymbolArtConverter(BitmapFilename, Options.GetOptions());

                    _currentGroup = converter.Convert();

                    var sa = new SymbolArtModel(new DummyUndoModel());
                    sa.Children.Add(new SymbolArtGroupModel(sa.Undo, _currentGroup, sa));

                    CurrentSymbolArt = sa;
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Unable to convert bitmap: " + ex.Message;
                }
            }

            AcceptCommand.NotifyCanExecuteChanged();
        }
    }
}
