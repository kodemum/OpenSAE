using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSAE.Core;
using OpenSAE.Core.BitmapConverter;
using OpenSAE.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OpenSAE.Models
{
    public class BitmapConverterModel : ObservableObject
    {
        private IDialogService _dialogService;
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
        private double _score;
        private byte[]? _currentImageData;
        private BitmapImage? _currentImage;
        private readonly Dispatcher _dispatcher;
        private Symbol? _selectedSymbol;
        private Symbol? _selectedPendingSymbol;

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
            set
            {
                if (SetProperty(ref _bitmapFilename, value))
                {
                    LoadBitmapBackgroundColor();
                    Reload();
                }
            }
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

        public Symbol? SelectedSymbol
        {
            get => _selectedSymbol;
            set
            {
                if (SetProperty(ref _selectedSymbol, value))
                    SymbolCommand.NotifyCanExecuteChanged();
            }
        }

        public Symbol? SelectedPendingSymbol
        {
            get => _selectedPendingSymbol;
            set
            {
                if (SetProperty(ref _selectedPendingSymbol, value))
                    SymbolCommand.NotifyCanExecuteChanged();
            }
        }

        public BitmapToSymbolArtConverterOptionsViewModel Options { get; }

        public ObservableCollection<Symbol> AvailableSymbols { get; }

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

        public double Score
        {
            get => _score;
            set => SetProperty(ref _score, value);
        }

        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        public byte[]? ImageData
        {
            get => _currentImageData;
            set
            {
                if (SetProperty(ref _currentImageData, value))
                {
                    if (value != null)
                    {
                        var ms = new MemoryStream(value);

                        BitmapImage image = new();

                        image.BeginInit();
                        image.StreamSource = ms;
                        image.EndInit();

                        CurrentImage = image;
                    }
                    else
                    {
                        CurrentImage = null;
                    }
                }
            }
        }

        public IRelayCommand BrowseCommand { get; }

        public IRelayCommand AcceptCommand { get; }

        public IRelayCommand SymbolCommand { get; }

        public BitmapConverterModel(IDialogService dialogService, Action<SymbolArtGroup> openAction)
        {
            _dialogService = dialogService;
            _openAction = openAction;
            BrowseCommand = new RelayCommand(BrowseCommand_Implementation);
            AcceptCommand = new RelayCommand(AcceptCommand_Implementation, () => CurrentSymbolArt != null);
            SymbolCommand = new RelayCommand<string>(SymbolCommand_Implementation, SymbolCommand_CanRun);

            Options = new BitmapToSymbolArtConverterOptionsViewModel();
            Options.PropertyChanged += (_, __) => Reload();
            AvailableSymbols = new(SymbolUtil.SymbolsUsableByBitmapConverter);

            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        private bool SymbolCommand_CanRun(string? obj)
        {
            return obj == "add" ? _selectedPendingSymbol != null : _selectedSymbol != null;
        }

        private void SymbolCommand_Implementation(string? obj)
        {
            if (obj == "add")
            {
                if (_selectedPendingSymbol != null)
                {
                    if (!Options.ShapeSymbolsToUse.Contains(_selectedPendingSymbol))
                    {
                        Options.ShapeSymbolsToUse.Add(_selectedPendingSymbol);
                    }
                }
            }
            else
            {
                if (_selectedSymbol != null)
                {
                    Options.ShapeSymbolsToUse.Remove(_selectedSymbol);
                }
            }
        }

        public void SetDialogService(IDialogService service)
        {
            _dialogService = service;
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
            string? filename = _dialogService.BrowseOpenFile("Convert bitmap image", AppModel.BitmapFormatFilter);

            if (filename == null)
                return;

            BitmapFilename = filename;
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

        private void LoadBitmapBackgroundColor()
        {
            try
            {
                if (BitmapFilename is not null)
                {
                    using var image = Image.Load<Rgba32>(BitmapFilename);

                    var mostCommon = image.FindMostCommonColor();
                    if (mostCommon.A > 127)
                    {
                        Options.BackgroundColor = mostCommon.ToWindowsMediaColor();
                    }
                    else
                    {
                        // image is transparent, default to white background
                        Options.BackgroundColor = Colors.White;
                    }
                }
            }
            catch
            { 
            }
        }

        private CancellationTokenSource? convertCancelTokenSource;
        private Task? convertTask;

        private void Reload()
        {
            if (string.IsNullOrEmpty(BitmapFilename))
            {
                CurrentSymbolArt = null;
            }
            else
            {
                LoadProgress = 0;
                IsLoading = true;

                convertCancelTokenSource?.Cancel();
                convertCancelTokenSource = new CancellationTokenSource();

                var token = convertCancelTokenSource.Token;

                void action()
                {
                    try
                    {
                        _dispatcher.Invoke(() =>
                        {
                            CurrentSymbolArt = new SymbolArtModel(new DummyUndoModel());
                            _currentGroup = new SymbolArtGroupModel(CurrentSymbolArt.Undo, new SymbolArtGroup() { Visible = true }, CurrentSymbolArt);

                            CurrentSymbolArt.Children.Add(_currentGroup);
                            ImageData = null;
                        });

                        token.ThrowIfCancellationRequested();

                        Action<Image>? imageCallback = Options.ShowViewPort ? (image) =>
                        {
                            using var ms = new MemoryStream();
                            image.SaveAsPng(ms);

                            _dispatcher.Invoke(() => ImageData = ms.ToArray());
                        } : null;

                        using var converter = new GeometrizeBitmapConverter(BitmapFilename, Options.GetOptions());

                        converter.Convert(AddLayer, token, new Progress<GeometrizeProgress>(x =>
                        {
                            LoadProgress = x.Percentage;
                            Score = x.Score;
                        }), imageCallback);

                        _dispatcher.Invoke(() =>
                        {
                            LayerCount = CurrentSymbolArt?.LayerCount ?? 0;

                            RefreshSymbolAmount();
                            ErrorMessage = null;
                            IsLoading = false;
                            AcceptCommand.NotifyCanExecuteChanged();
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
                }

                if (convertTask is null)
                {
                    convertTask = Task.Run(action);
                }
                else
                {
                    convertTask = convertTask.ContinueWith((t) => action());
                }
            }
        }

        private void AddLayer(SymbolArtLayer layer)
        {
            if (CurrentSymbolArt is null || _currentGroup is null)
                return;

            var layerModel = new SymbolArtLayerModel(CurrentSymbolArt.Undo, layer, _currentGroup);

            try
            {
                _dispatcher.Invoke(() =>
                {
                    _currentGroup.Children.Insert(0, layerModel);

                    LayerCount = CurrentSymbolArt.LayerCount;

                    RefreshSymbolAmount();
                    OnPropertyChanged(nameof(CurrentSymbolArt));
                });
            }
            catch (TaskCanceledException) { }
        }
    }
}
