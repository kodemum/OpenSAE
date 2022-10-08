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
        private double _symbolSizeOffset;
        private int _resizeImageHeight;
        private int _maxColors;
        private bool _removeWhite;

        public SymbolArtModel? CurrentSymbolArt
        {
            get => _currentSymbolArt;
            set => SetProperty(ref _currentSymbolArt, value);
        }

        public string? BitmapFilename
        {
            get => _bitmapFilename;
            set => SetRefreshProperty(ref _bitmapFilename, value);
        }

        public double SymbolSizeOffset
        {
            get => _symbolSizeOffset;
            set => SetRefreshProperty(ref _symbolSizeOffset, value);
        }

        public int ResizeImageHeight
        {
            get => _resizeImageHeight;
            set => SetRefreshProperty(ref _resizeImageHeight, value);
        }

        public int MaxColors
        {
            get => _maxColors;
            set => SetRefreshProperty(ref _maxColors, value);
        }

        public bool RemoveWhite
        {
            get => _removeWhite;
            set => SetRefreshProperty(ref _removeWhite, value);
        }

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

            var defaultOptions = new BitmapToSymbolArtConverterOptions();
            _symbolSizeOffset = defaultOptions.SizeOffset;
            _resizeImageHeight = defaultOptions.ResizeImageHeight;
            _maxColors = defaultOptions.MaxColors;
            _removeWhite = defaultOptions.RemoveWhite;
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
                    _currentGroup = BitmapToSymbolArtConverter.BitmapToSymbolArt(BitmapFilename, new BitmapToSymbolArtConverterOptions()
                    {
                        ResizeImageHeight = _resizeImageHeight,
                        MaxColors = _maxColors,
                        SizeOffset = _symbolSizeOffset,
                        RemoveWhite = _removeWhite
                    });

                    CurrentSymbolArt = new SymbolArtModel(new DummyUndoModel());
                    CurrentSymbolArt.Children.Add(new SymbolArtGroupModel(CurrentSymbolArt.Undo, _currentGroup, CurrentSymbolArt));
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
