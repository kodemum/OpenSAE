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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace OpenSAE.Models
{
    internal class BitmapConverterModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly Action<SymbolArtGroup> _openAction;
        private SymbolArtModel? _currentSymbolArt;
        private SymbolArtGroupModel? _currentGroup;
        private string? _bitmapFilename;
        private string? _errorMessage;
        private double _symbolCountReduction = 100;
        private int _visibleLayerCount = 0;
        private bool _lockSymbolCount;
        private bool _isLoading;
        private double _loadProgress = 0;

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

        public bool TooManyLayers => LayerCount > 225;

        public int LayerCount
        {
            get => _visibleLayerCount;
            set
            {
                if (SetProperty(ref _visibleLayerCount, value))
                {
                    OnPropertyChanged(nameof(TooManyLayers));
                }
            }
        }

        public BitmapToSymbolArtConverterOptionsViewModel Options { get; }

        public SymbolListModel SymbolsList { get; }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public double SymbolCountReduction
        {
            get => _symbolCountReduction;
            set
            {
                if (SetProperty(ref _symbolCountReduction, value))
                {
                    RefreshSymbolAmount();
                }
            }
        }

        public bool LockSymbolCount
        {
            get => _lockSymbolCount;
            set
            {
                if (SetProperty(ref _lockSymbolCount, value))
                {
                    RefreshSymbolAmount();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public double LoadProgress
        {
            get => _loadProgress;
            set => SetProperty(ref _loadProgress, value);
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
                var group = (SymbolArtGroup)_currentGroup.ToSymbolArtItem();

                group.RemoveAllEmpty();

                _openAction?.Invoke(group);
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

        private void RefreshSymbolAmount()
        {
            if (CurrentSymbolArt == null)
                return;

            if (LockSymbolCount)
            {
                _symbolCountReduction = Math.Min(100, 225.0 / CurrentSymbolArt.LayerCount * 100);
                OnPropertyChanged(nameof(SymbolCountReduction));
            }

            var symbols = CurrentSymbolArt.GetAllLayers().OrderByDescending(layer =>
            {
                var bounds = layer.BoundingVertices;

                return (bounds[2].X - bounds[0].X) * (bounds[2].Y - bounds[0].Y);
            }).ToArray();

            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i].Visible = i < Math.Floor(symbols.Length * SymbolCountReduction / 100);
            }

            LayerCount = symbols.Count(x => x.Visible);
        }

        private CancellationTokenSource convertCancelTokenSource;

        private void Reload()
        {
            if (string.IsNullOrEmpty(BitmapFilename))
            {
                CurrentSymbolArt = null;
            }
            else
            {
                var dispatcher = Dispatcher.CurrentDispatcher;

                LoadProgress = 0;
                IsLoading = true;

                convertCancelTokenSource?.Cancel();
                convertCancelTokenSource = new CancellationTokenSource();

                Task.Run(() =>
                {
                    try
                    {
                        using var converter = new BitmapToSymbolArtConverter(BitmapFilename, Options.GetOptions());

                        var group = converter.Convert(convertCancelTokenSource.Token, new Progress<double>(x => LoadProgress = x));

                        var sa = new SymbolArtModel(new DummyUndoModel());
                        _currentGroup = new SymbolArtGroupModel(sa.Undo, group, sa);

                        sa.Children.Add(_currentGroup);

                        dispatcher.Invoke(() =>
                        {
                            CurrentSymbolArt = sa;
                            LayerCount = sa.LayerCount;

                            RefreshSymbolAmount();
                            ErrorMessage = null;
                            IsLoading = false;
                        });
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = "Unable to convert bitmap: " + ex.Message;
                        IsLoading = false;
                    }
                });
            }

            AcceptCommand.NotifyCanExecuteChanged();
        }
    }
}
