﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FramePFX.Themes;
using OpenSAE.Core;
using OpenSAE.Helpers;
using OpenSAE.Properties;
using OpenSAE.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OpenSAE.Models
{
    /// <summary>
    /// Main model for the app
    /// </summary>
    public class AppModel : ObservableObject
    {
        private const string OpenFormatFilter = "Symbol art files (*.saml,*.sar)|*.saml;*.sar|SAML symbol art (*.saml)|*.saml|SAR symbol art (*.sar)|*.sar";
        private const string SaveFormatFilter = "SAML symbol art (*.saml)|*.saml";
        private const string ExportFormatFilter = "SAR symbol art (*.sar)|*.sar";
        internal const string BitmapFormatFilter = "Bitmap images (*.png,*.jpg...)|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tif;*.webp|PNG image (*.png)|*.png|JPEG image (*.jpg)|*.jpg;*.jpeg|GIF image (*.gif)|*.gif|BMP image (*.bmp)|*.bmp|TIFF image (*.tif)|*.tif|WEBP image (*.webp)|*.webp";
        private const string ClipboardItemFormat = "OpenSAE.Item";
        private const double DefaultSymbolUnitWidth = 240;
        public const double MinimumSymbolUnitWidth = 24;
        public const double MaximumSymbolUnitWidth = 960;
        private static readonly double[] zoomLevels = new double[] { 25, 33, 50, 67, 100, 150, 200, 300, 400, 500, 600, 800, 1000 };

        private readonly IDialogService _dialogService;
        private SymbolArtModel? _currentSymbolArt;
        private IReadOnlyList<SymbolArtItemModel> _selectedItems = new List<SymbolArtItemModel>();
        private SymbolArtItemModel? _selectedItem;
        private bool _applyToneCurve;
        private bool _guideLinesEnabled;
        private bool _showHelpScreen;
        private bool _showImageOverlays;
        private double _displaySymbolUnitWidth = DefaultSymbolUnitWidth;
        private Point _viewPosition = new(0, 0);
        private int _tabIndex = 0;
        private CanvasEditMode _canvasEditMode;
        private DisplaySettingFlags _displayFlags;
        private EditViewModel? _currentEditView;

        public event EventHandler? ExitRequested;

        public event EventHandler<BitmapConversionRequestedEventArgs>? BitmapConversionRequested;

        /// <summary>
        /// Currently opened Symbol Art
        /// </summary>
        public SymbolArtModel? CurrentSymbolArt
        {
            get => _currentSymbolArt;
            private set
            {
                var previous = _currentSymbolArt;

                if (SetProperty(ref _currentSymbolArt, value))
                {
                    OnPropertyChanged(nameof(AppTitle));
                    ZoomPercentage = 100;
                    ViewPosition = new(0, 0);
                    ShowHelpScreen = false;
                    TabIndex = 0;
                    CurrentEditView = null;
                    SaveCommand.NotifyCanExecuteChanged();
                    SaveAsCommand.NotifyCanExecuteChanged();
                    AddImageLayerCommand.NotifyCanExecuteChanged();
                    OpenViewCurrentItemCommand.NotifyCanExecuteChanged();
                    SelectedItem = _currentSymbolArt;

                    if (previous != null)
                    {
                        previous.PropertyChanged -= CurrentSymbolArt_PropertyChanged;
                    }

                    if (value != null)
                    {
                        value.PropertyChanged += CurrentSymbolArt_PropertyChanged;
                    }
                }
            }
        }

        public EditViewModel? CurrentEditView
        {
            get => _currentEditView;
            set => SetProperty(ref _currentEditView, value);
        }

        private void CurrentSymbolArt_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SymbolArtModel.Name):
                    OnPropertyChanged(nameof(AppTitle));
                    break;

                case nameof(SymbolArtModel.SoundEffect):
                    if (CurrentSymbolArt != null)
                    {
                        try
                        {
                            SoundEffectHelper.PlaySoundEffectIfExists(CurrentSymbolArt.SoundEffect);
                        }
                        catch
                        {
                            // should this fail - say if the machine doesn't have audio support,
                            // it's really not that important
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Currently selected item or items (group, layer, SA root, collection of selected items)
        /// </summary>
        public SymbolArtItemModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                var list = new List<SymbolArtItemModel>();

                if (value != null)
                    list.Add(value);

                SelectedItems = list;
            }
        }

        /// <summary>
        /// First item selected if any; does not aggregate multiple selected items into a collection.
        /// </summary>
        public SymbolArtItemModel? FirstSelectedItem => _selectedItems.Count > 0 ? _selectedItems[0] : null;

        /// <summary>
        /// Helper for data binding as we cannot databind an a generic list. Use <see cref="SelectedItems"/> instead.
        /// </summary>
        public IEnumerable SelectedItemsBind
        {
            get => SelectedItems;
            set => SelectedItems = value?.Cast<SymbolArtItemModel>()?.ToList() ?? new List<SymbolArtItemModel>();
        }

        public IReadOnlyList<SymbolArtItemModel> SelectedItems
        {
            get => _selectedItems;
            set
            {
                if (value is IReadOnlyList<SymbolArtItemModel> list && SetProperty(ref _selectedItems, list))
                {
                    CurrentItemCommand.NotifyCanExecuteChanged();
                    RotateCurrentItemCommand.NotifyCanExecuteChanged();

                    if (list.Count == 0)
                        _selectedItem = null;
                    else if (list.Count == 1)
                        _selectedItem = list[0];
                    else
                        _selectedItem = SymbolArtItemCollection.Create(list);

                    OnPropertyChanged(nameof(SelectedItemIsLayer));
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(SelectedItemsBind));
                }
            }
        }

        public bool ShowImageOverlays
        {
            get => _showImageOverlays;
            set => SetProperty(ref _showImageOverlays, value);
        }

        /// <summary>
        /// If the tone curve mapping is enabled to better approximate how colors look in-game
        /// </summary>
        public bool ApplyToneCurve
        {
            get => _applyToneCurve;
            set => SetProperty(ref _applyToneCurve, value);
        }

        public int TabIndex
        {
            get => _tabIndex;
            set
            {
                if (SetProperty(ref _tabIndex, value) && value > 0)
                {
                    CurrentSymbolArt?.RaisePaletteChanged();
                }
            }
        }

        /// <summary>
        /// If guide lines should be shown for the center of the symbol art
        /// </summary>
        public bool GuideLinesEnabled
        {
            get => _guideLinesEnabled;
            set => SetProperty(ref _guideLinesEnabled, value);
        }

        /// <summary>
        /// Width of viewport in symbol art units. Shows the full width when equal to the width
        /// of the current symbol art, any other value zooms in or out.
        /// </summary>
        public double DisplaySymbolUnitWidth
        {
            get => _displaySymbolUnitWidth;
            set
            {
                if (SetProperty(ref _displaySymbolUnitWidth, value))
                {
                    OnPropertyChanged(nameof(ZoomPercentage));
                }
            }
        }

        public double ZoomPercentage
        {
            get => DefaultSymbolUnitWidth / _displaySymbolUnitWidth * 100;
            set
            {
                DisplaySymbolUnitWidth = DefaultSymbolUnitWidth / value * 100;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplaySymbolUnitWidth));
            }
        }

        /// <summary>
        /// Position of viewport relative to the center (0,0) (symbol art units)
        /// </summary>
        public Point ViewPosition
        {
            get => _viewPosition;
            set => SetProperty(ref _viewPosition, value);
        }

        public bool ShowHelpScreen
        {
            get => _showHelpScreen;
            set => SetProperty(ref _showHelpScreen, value);
        }

        public CanvasEditMode CanvasEditMode
        {
            get => _canvasEditMode;
            set
            {
                if (SetProperty(ref _canvasEditMode, value))
                {
                    OnPropertyChanged(nameof(ColorPickerEnabled));
                }
            }
        }

        public bool ColorPickerEnabled
        {
            get => _canvasEditMode == CanvasEditMode.ColorPicker;
            set => CanvasEditMode = value ? CanvasEditMode.Default : CanvasEditMode.ColorPicker;
        }

        public bool NaturalSymbolSelectionEnabled
        {
            get => DisplaySettingFlags.HasFlag(DisplaySettingFlags.NaturalSymbolSelection);
            set => SetDisplayFlag(DisplaySettingFlags.NaturalSymbolSelection, value);
        }

        public bool RestrictToAffineManipulation
        {
            get => DisplaySettingFlags.HasFlag(DisplaySettingFlags.RestrictToAffineManipulation);
            set => SetDisplayFlag(DisplaySettingFlags.RestrictToAffineManipulation, value);
        }

        public bool IngameRenderModeEnabled
        {
            get => DisplaySettingFlags.HasFlag(DisplaySettingFlags.IngameRenderMode);
            set => SetDisplayFlag(DisplaySettingFlags.IngameRenderMode, value);
        }

        public DisplaySettingFlags DisplaySettingFlags
        {
            get => _displayFlags;
            set => SetProperty(ref _displayFlags, value);
        }

        public List<ThemeModel> AvailableThemes { get; }
            = new List<ThemeModel>()
            {
                new(ThemeType.SoftDark, "Soft dark"),
                new(ThemeType.DeepDark, "Deep dark"),
                new(ThemeType.GreyTheme, "Grey"),
                new(ThemeType.DarkGreyTheme, "Dark grey"),
                new(ThemeType.RedBlackTheme, "Red/black"),
                new(ThemeType.LightGreyTheme, "Light grey"),
                new(ThemeType.WhiteTheme, "White")
            };

        public Controls.IsChildOfPredicate HierarchyPredicate { get; } = SymbolArtItemModel.IsChildOf;

        public bool SelectedItemIsLayer => SelectedItem is SymbolArtLayerModel;

        public string AppTitle
        {
            get
            {
                string state = string.Empty;

                if (CurrentSymbolArt != null)
                {
                    if (CurrentSymbolArt.FileName != null)
                    {
                        state = $"{Path.GetFileName(CurrentSymbolArt.FileName)} - ";
                    }

                    state += $"{CurrentSymbolArt.Name} - ";
                }

                return state + "OpenSAE Symbol Art Editor";
            }
        }

        public SymbolListModel SymbolsList { get; }

        public UndoModel Undo { get; }

        public ObservableCollection<string> RecentFiles { get; }

        public ICommand NewFileCommand { get; }

        public ICommand OpenFileCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand SaveAsCommand { get; }

        public RelayCommand<string> CurrentItemCommand { get; }

        public RelayCommand<string> RotateCurrentItemCommand { get; }

        public RelayCommand<string> ChangeSettingCommand { get; }

        public RelayCommand<string> ZoomCommand { get; set; }

        public RelayCommand HelpCommand { get; set; }

        public ICommand ExitCommand { get; }

        public RelayCommand<string> AddImageLayerCommand { get; }

        public RelayCommand<string> ClipboardCommand { get; }

        public RelayCommand<string> OpenViewCurrentItemCommand { get; }

        public ICommand CurrentViewActionCommand { get; }

        public IRelayCommand CanvasModeCommand { get; }

        public IRelayCommand ImportCommand { get; }

        public IDialogService DialogService => _dialogService;

        public AppModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            SymbolsList = new SymbolListModel();
            RecentFiles = Settings.Default.RecentFiles != null ? new ObservableCollection<string>(Settings.Default.RecentFiles.ToEnumerable()!) : new ObservableCollection<string>();
            ApplyToneCurve = Settings.Default.ApplyToneCurve;
            ShowImageOverlays = Settings.Default.ShowImageOverlays;
            _displayFlags = (DisplaySettingFlags)Settings.Default.DisplayFlags;
            Undo = new UndoModel();

            OpenFileCommand = new RelayCommand<string>(OpenFile_Implementation);
            NewFileCommand = new RelayCommand(NewFile_Implementation);
            ExitCommand = new RelayCommand(() => ExitRequested?.Invoke(this, EventArgs.Empty));
            SaveCommand = new RelayCommand(() => SaveCurrent(), () => CurrentSymbolArt != null);
            SaveAsCommand = new RelayCommand(() => SaveCurrentAs(), () => CurrentSymbolArt != null);
            AddImageLayerCommand = new RelayCommand<string>(AddImageLayer_Implementation, (arg) => CurrentSymbolArt != null);
            ImportCommand = new RelayCommand(Import_Implementation);

            CurrentItemCommand = new RelayCommand<string>(CurrentItemActionCommand_Implementation, (arg) => SelectedItem != null);
            RotateCurrentItemCommand = new RelayCommand<string>(RotateCurrentItemCommand_Implementation, (_) => SelectedItem != null);
            ChangeSettingCommand = new RelayCommand<string>(ChangeSetting_Implementation);
            ZoomCommand = new RelayCommand<string>(Zoom_Implementation);
            HelpCommand = new RelayCommand(() => ShowHelpScreen = !ShowHelpScreen);
            ClipboardCommand = new RelayCommand<string>(ClipboardCommand_Implementation, ClipboardCommand_CanRun);
            CanvasModeCommand = new RelayCommand<string>(CanvasModeCommand_Implementation);
            OpenViewCurrentItemCommand = new RelayCommand<string>(OpenViewCurrentItemCommand_Implementation, (arg) => CurrentSymbolArt != null);
            CurrentViewActionCommand = new RelayCommand<string>(CurrentViewActionCommand_Implementation);

            CommandManager.RequerySuggested += (_, __) => ClipboardCommand.NotifyCanExecuteChanged();

            if (!Settings.Default.HelpShown)
            {
                ShowHelpScreen = true;
                Settings.Default.HelpShown = true;
            }
        }

        private void CurrentViewActionCommand_Implementation(string? action)
        {
            if (CurrentEditView is null)
            {
                return;
            }

            try
            {
                switch (action)
                {
                    case "apply":
                        CurrentEditView.ApplyChanges();
                        CurrentEditView = null;
                        break;

                    case "cancel":
                        CurrentEditView.Cancel();
                        CurrentEditView = null;
                        break;

                    default:
                        throw new Exception($"Unknown view action {action}");
                }
                
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Edit view action error", "An error occurred while trying to apply or cancel an edit view.", ex);
            }
        }

        private void OpenViewCurrentItemCommand_Implementation(string? obj)
        {
            if (SelectedItem is null)
            {
                return;
            }

            switch (obj)
            {
                case "adjustColor":
                    CurrentEditView = new SymbolArtColorAdjustmentModel(Undo, SelectedItem);
                    break;
            }
        }

        private void CanvasModeCommand_Implementation(string? option)
        {
            switch (option)
            {
                case "colorPicker":
                    CanvasEditMode = CanvasEditMode == CanvasEditMode.ColorPicker ? CanvasEditMode.Default : CanvasEditMode.ColorPicker;
                    break;
            }
        }

        private void AddImageLayer_Implementation(string? filename)
        {
            if (filename == null)
            {
                filename = _dialogService.BrowseOpenFile("Open overlay image", BitmapFormatFilter);

                if (filename == null)
                    return;
            }

            byte[] imageBuffer;

            try
            {
                imageBuffer = File.ReadAllBytes(filename);
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Unable to open image", "Unable to read the content of the specified file", ex);
                return;
            }

            try
            {
                // ensure the image is readable
                Converters.ImageSourceConverter.AssertImageIsReadable(imageBuffer);
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Unable to open image", "Image could not be opened - unknown format?", ex);
                return;
            }

            AddItemToCurrentSymbolArt((group) => new SymbolArtImageLayerModel(Undo, $"{Path.GetFileNameWithoutExtension(filename)} (overlay)", imageBuffer, group));

            ShowImageOverlays = true;
        }

        private void Zoom_Implementation(string? obj)
        {
            if (obj == null)
            {
                ZoomPercentage = 100;
            }
            else
            {
                bool zoomIn = obj == "in";

                if (zoomIn && ZoomPercentage < zoomLevels[zoomLevels.Length - 1])
                {
                    ZoomPercentage = zoomLevels.FirstOrDefault(x => x > ZoomPercentage);
                }
                else if (!zoomIn && ZoomPercentage > zoomLevels[0])
                {
                    ZoomPercentage = zoomLevels.LastOrDefault(x => x < ZoomPercentage);
                }
            }
        }

        private void ChangeSetting_Implementation(string? operation)
        {
            switch (operation)
            {
                case "toneCurve":
                    ApplyToneCurve = !ApplyToneCurve;
                    break;

                case "guideLines":
                    GuideLinesEnabled = !GuideLinesEnabled;
                    break;

                case "ingameRenderMode":
                    IngameRenderModeEnabled = !IngameRenderModeEnabled;
                    break;
            }
        }

        private void RotateCurrentItemCommand_Implementation(string? angle)
        {
            if (SelectedItem == null || angle == null)
                return;

            SelectedItem.Manipulate($"Rotate {angle}°", x => x.Rotate(double.Parse(angle) / 180 * Math.PI));
        }

        private bool ClipboardCommand_CanRun(string? operation)
        {
            switch (operation)
            {
                case "copy":
                    return SelectedItem != null;

                case "paste":
                    return CurrentSymbolArt != null && Clipboard.ContainsData(ClipboardItemFormat);

                default:
                    return true;
            }
        }

        private void ClipboardCommand_Implementation(string? operation)
        {
            switch (operation)
            {
                case "copy":
                    if (SelectedItem != null)
                    {
                        Clipboard.SetData(ClipboardItemFormat, SelectedItem.ToSymbolArtItem().Serialize());
                    }
                    break;

                case "paste":
                    int newIndex = 0;
                    SymbolArtItemModel targetParent;

                    if (SelectedItem is SymbolArtGroupModel selectedGroup)
                    {
                        targetParent = selectedGroup;
                    }
                    else if (SelectedItem?.Parent != null)
                    {
                        targetParent = SelectedItem.Parent;
                        newIndex = SelectedItem.IndexInParent;
                    }
                    else
                    {
                        targetParent = CurrentSymbolArt ?? throw new InvalidOperationException("No symbol art loaded");
                    }

                    try
                    {
                        SymbolArtItemModel? newItem = null;

                        if (Clipboard.GetData(ClipboardItemFormat) is string serializedItem)
                        {
                            var deserializedItem = Core.SymbolArtItem.Deserialize(serializedItem);

                            if (deserializedItem is Core.SymbolArtLayer layer)
                            {
                                newItem = new SymbolArtLayerModel(Undo, layer, targetParent);
                            }
                            else if (deserializedItem is Core.SymbolArtGroup group)
                            {
                                newItem = new SymbolArtGroupModel(Undo, group, targetParent);
                            }
                        }

                        if (newItem != null)
                        {
                            var currentSelection = SelectedItems;

                            if (newItem.Name == SymbolArtItemCollection.CollectionIdentifier)
                            {
                                Undo.Do($"Paste {newItem.Children.Count} items",
                                    () =>
                                    {
                                        foreach (var item in newItem.Children.Reverse())
                                        {
                                            targetParent.Children.Insert(newIndex, item);
                                        }

                                        SelectedItems = new List<SymbolArtItemModel>(newItem.Children);
                                    },
                                    () =>
                                    {
                                        foreach (var item in newItem.Children)
                                        {
                                            targetParent.Children.Remove(item);
                                        }
                                        
                                        SelectedItems = currentSelection;
                                    }
                                );
                            }
                            else
                            {
                                Undo.Do($"Paste {newItem.ItemTypeName}",
                                    () =>
                                    {
                                        targetParent.Children.Insert(newIndex, newItem);
                                        SelectedItem = newItem;
                                    },
                                    () =>
                                    {
                                        targetParent.Children.Remove(newItem);
                                        SelectedItems = currentSelection;
                                    }
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _dialogService.ShowErrorMessage("Unable to paste item", "Unable to paste item from clipboard", ex);
                    }
                    break;
            }
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

                case "toggleImageOverlays":
                    ShowImageOverlays = !ShowImageOverlays;
                    break;

                case "deselect":
                    SelectedItem = null;
                    break;

                case "delete":
                    DeleteItem(SelectedItem);
                    break;

                case "flipX":
                    SelectedItem.Manipulate("Horizontal flip", x => x.FlipX());
                    break;

                case "flipY":
                    SelectedItem.Manipulate("Vertical flip", x => x.FlipY());
                    break;

                case "moveUp":
                    SelectedItem.MoveUp();
                    break;

                case "moveDown":
                    SelectedItem.MoveDown();
                    break;

                case "addLayer":
                    AddItemToCurrentSymbolArt((group) => new SymbolArtLayerModel(Undo, CurrentSymbolArt!.LayerCount + 1, group));
                    break;

                case "addGroup":
                    AddItemToCurrentSymbolArt((parentGroup) => new SymbolArtGroupModel(Undo, "Group", true, parentGroup));
                    break;

                case "groupSelection":
                    CreateGroupForCurrentlySelectedItems();
                    break;

                case "export":
                    ExportCurrent();
                    break;

                case "exportBitmap":
                    ExportCurrentAsBitmap();
                    break;

                case "cleanupBounds":
                    CleanupOutsideActiveArea();
                    break;

                case "groupByColor":
                    GroupLayersBasedOnColor();
                    break;

                case "restrictToAffine":
                    RestrictTransformationsToAffine();
                    break;

                case "corruptNonAffine":
                    CorruptNonAffine();
                    break;

                case "duplicate":
                    if (SelectedItem.Parent == null)
                        return;

                    var currentSelection = SelectedItems;
                    List<SymbolArtItemModel> duplicates = new();
                    SymbolArtItemModel parent = SelectedItem.Parent;
                    int currentIndex = SelectedItem.IndexInParent;

                    foreach (var item in SelectedItems)
                    {
                        duplicates.Add(item.Duplicate(parent));
                    }

                    duplicates.Reverse();

                    Undo.Do(SelectedItems.Count == 1 ? $"Duplicate {SelectedItem.ItemTypeName}" : $"Duplicate {SelectedItems.Count} items",
                        () =>
                        {
                            foreach (var item in duplicates)
                            {
                                parent.Children.Insert(currentIndex, item);
                            }

                            SelectedItems = duplicates;
                        },
                        () =>
                        {
                            foreach (var item in duplicates)
                            {
                                parent.Children.Remove(item);
                            }

                            SelectedItems = currentSelection;
                        });

                    break;
            }
        }

        private void RestrictTransformationsToAffine()
        {
            if (CurrentSymbolArt == null)
                return;

            using var scope = Undo.StartAggregateScope("Convert to affine");

            foreach (var layer in CurrentSymbolArt.GetAllLayers())
            {
                var diff1 = layer.Vertex1 - layer.Vertex2;
                var diff2 = layer.Vertex4 - layer.Vertex3;

                if (diff1 != diff2)
                {
                    var avgDiff = (diff1 + diff2) / 2;

                    var oldVertices = layer.Vertices;

                    Undo.Do("Update vertices", () =>
                    {
                        layer.Vertex3 = oldVertices[3] - avgDiff;
                        layer.Vertex1 = oldVertices[1] + avgDiff;
                    }, () =>
                    {
                        layer.Vertices = oldVertices;
                    });
                }
            }
        }

        private void CorruptNonAffine()
        {
            if (CurrentSymbolArt == null)
                return;

            using var scope = Undo.StartAggregateScope("Corrupt non-affine");

            foreach (var layer in CurrentSymbolArt.GetAllLayers())
            {
                var diff1 = layer.Vertex1 - layer.Vertex2;
                var diff2 = layer.Vertex4 - layer.Vertex3;

                if (diff1 != diff2)
                {
                    var avgDiff = (diff1 + diff2) / 4;
                    var oldVertices = layer.Vertices;

                    Undo.Do("Update vertices", () =>
                    {
                        layer.Vertex4 = layer.Vertex3 + avgDiff;
                        layer.Vertex3 -= avgDiff;
                        layer.Vertex1 = layer.Vertex2 + avgDiff;
                        layer.Vertex2 -= avgDiff;
                    }, () =>
                    {
                        layer.Vertices = oldVertices;
                    });
                }
            }
        }

        public bool ExportCurrentAsBitmap()
        {
            if (CurrentSymbolArt == null)
                return false;

            string? filename = _dialogService.BrowseSaveFile("Export symbol art image", BitmapFormatFilter, Path.ChangeExtension(CurrentSymbolArt.FileName, ".png"));

            if (filename == null)
                return false;

            try
            {
                BitmapEncoder encoder = Path.GetExtension(filename).ToLowerInvariant() switch
                {
                    ".png" => new PngBitmapEncoder(),
                    ".jpg" => new JpegBitmapEncoder(),
                    ".jpeg" => new JpegBitmapEncoder(),
                    ".gif" => new GifBitmapEncoder(),
                    ".bmp" => new BmpBitmapEncoder(),
                    ".tif" => new TiffBitmapEncoder(),
                    _ => throw new InvalidOperationException($"Unknown bitmap file format {Path.GetExtension(filename)}")
                };

                int height, width;

                if (CurrentSymbolArt.Size == Core.SymbolArtSize.AllianceLogo)
                {
                    width = 512;
                    height = 512; 
                }
                else
                {
                    width = 1920;
                    height = 960;
                }

                using FileStream fs = File.Open(filename, FileMode.Create);

                BitmapSymbolArtRenderer.RenderToStream(CurrentSymbolArt, encoder, width, height, fs);

                return true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Error saving file", "Unable to export the symbol art to a bitmap image.", ex);
            }

            return false;
        }

        public void DeleteItem(SymbolArtItemModel item)
        {
            var targetItem = item;
            var targetParent = targetItem.Parent;
            var currentIndex = item.IndexInParent;
            var currentSelection = SelectedItems;

            using var scope = Undo.StartAggregateScope($"Delete {item.ItemTypeName}", afterUndo: () => SelectedItems = currentSelection);

            // deleting the item adds it to the undo scope
            item.Delete();

            // but we also want to handle selecting the appropriate item
            Undo.Do("Change selection",
                () =>
                {
                    // select the item at the same position as the one that was deleted
                    // unless it was the last, in which case select the previous item
                    // If there are no other items, select the parent
                    if (targetParent?.Children.Count > currentIndex)
                        SelectedItem = targetParent.Children[currentIndex];
                    else if (targetParent?.Children.Count > currentIndex - 1 && currentIndex > 0)
                        SelectedItem = targetParent.Children[currentIndex - 1];
                    else
                        SelectedItem = targetParent;
                },
                () => { }
            );
        }

        public SymbolArtItemModel? CopyItemTo(SymbolArtItemModel source, SymbolArtItemModel target, bool considerCollectionSelection)
        {
            if (source.Parent == null)
                return null;

            if (considerCollectionSelection && SelectedItems.Count > 1 && SelectedItems.Contains(source))
            {
                using var scope = Undo.StartAggregateScope($"Copy {SelectedItems.Count} items");

                List<SymbolArtItemModel> newSelectedItems = new();
                var oldSelectedItems = SelectedItems;

                foreach (var item in SelectedItems.Reverse().ToArray())
                {
                    var itemCopy = CopyItemTo(item, target, false);

                    if (itemCopy != null)
                        newSelectedItems.Add(itemCopy);
                }

                Undo.Do($"Select symbols", () => SelectedItems = newSelectedItems, () => SelectedItems = oldSelectedItems);

                return null;
            }

            SymbolArtItemModel group;
            int targetIndex;

            if (target is SymbolArtGroupModel targetGroup)
            {
                group = targetGroup;
                targetIndex = 0;
            }
            else if (target.Parent != null)
            {
                group = target.Parent;
                targetIndex = target.IndexInParent;
            }
            else
            {
                throw new Exception("Invalid copy target");
            }

            var copy = source.Duplicate(group);
            var currentSelection = SelectedItems;

            Undo.Do($"Copy {source.ItemTypeName}",
                () =>
                {
                    group.Children.Insert(targetIndex, copy);
                    SelectedItem = copy;
                },
                () =>
                {
                    group.Children.Remove(copy);
                    SelectedItems = currentSelection;
                }
            );

            return copy;
        }

        public void MoveItemTo(SymbolArtItemModel source, SymbolArtItemModel target, bool considerCollectionSelection)
        {
            if (source.Parent == null)
                return;

            if (considerCollectionSelection && SelectedItems.Count > 1 && SelectedItems.Contains(source))
            {
                using var scope = Undo.StartAggregateScope($"Reorder {SelectedItems.Count} items");

                foreach (var item in SelectedItems.Reverse().ToArray())
                    MoveItemTo(item, target, false);
                
                return;
            }

            int currentIndex = source.IndexInParent;
            int newIndex;
            SymbolArtItemModel targetParent;

            if (target is SymbolArtGroupModel group)
            {
                newIndex = 0;
                targetParent = group;
            }
            else if (target.Parent != null)
            {
                // move to group at same index as target
                targetParent = target.Parent;
                newIndex = target.IndexInParent;
            }
            else
            {
                throw new Exception("Invalid move target");
            }

            var currentParent = source.Parent;
            
            // if already in the same group, we only need to change the index
            if (currentParent == targetParent)
            {
                // already in same group
                source.IndexInParent = target.IndexInParent;
            }
            else
            {
                var currentSelection = SelectedItems;

                Undo.Do($"Reorder {source.ItemTypeName}", 
                    () =>
                    {
                        currentParent.Children.Remove(source);
                        targetParent.Children.Insert(newIndex, source);
                        source.Parent = targetParent;
                        SelectedItem = source;
                    },
                    () =>
                    {
                        targetParent.Children.Remove(source);
                        currentParent.Children.Insert(currentIndex, source);
                        source.Parent = currentParent;
                        SelectedItems = currentSelection;
                    }
                );
            }
        }

        private void NewFile_Implementation()
        {
            if (!ConfirmCloseOpenFile())
                return;

            CurrentSymbolArt = new SymbolArtModel(Undo);
            Undo.ResetWith("Create new");
            SelectedItem = CurrentSymbolArt;
        }

        private void Import_Implementation()
        {
            string? filename = _dialogService.BrowseOpenFile("Import bitmap image", AppModel.BitmapFormatFilter);

            if (filename == null)
                return;

            var model = new BitmapConverterModel(DialogService, OpenConvertedGroup)
            {
                BitmapFilename = filename
            };

            BitmapConversionRequested?.Invoke(this, new BitmapConversionRequestedEventArgs(model));
        }

        private bool ConfirmCloseOpenFile()
        {
            if (Undo.ContainsNonPersistedChanges)
            {
                var result = _dialogService.ShowYesNoCancel("Save changes", "Would you like to save the changes made to the current symbol art?");

                switch (result)
                {
                    case true:
                        if (!SaveCurrent())
                        {
                            return false;
                        }
                        break;

                    case false:
                        break;

                    default:
                        return false;
                }
            }

            return true;
        }

        private void CreateGroupForCurrentlySelectedItems()
        {
            if (FirstSelectedItem == null)
                return;

            // create new group as child of parent of first selected item, or possibly the root item
            var targetParent = FirstSelectedItem.Parent ?? CurrentSymbolArt!;
            var targetIndex = FirstSelectedItem.IndexInParent;
            var currentSelection = SelectedItems;
            var newGroup = new SymbolArtGroupModel(Undo, $"{SelectedItems.Count} item group", true, targetParent);

            using var scope = Undo.StartAggregateScope($"Group {SelectedItems.Count} items");

            Undo.Do("Add group",
                () => targetParent.Children.Insert(targetIndex, newGroup),
                () => targetParent.Children.Remove(newGroup)
                );

            foreach (var item in SelectedItems.Reverse())
                MoveItemTo(item, newGroup, false);

            Undo.Do("Update selection", () => SelectedItem = newGroup, () => SelectedItems = currentSelection);
        }

        private void AddItemToCurrentSymbolArt(Func<SymbolArtGroupModel, SymbolArtItemModel> itemCreationPredicate)
        {
            // find group to add layer to - may be current item, its parent or possibly the root symbol art
            var selectedItem = FirstSelectedItem;

            SymbolArtGroupModel targetGroup =
                selectedItem as SymbolArtGroupModel
                ?? selectedItem?.Parent as SymbolArtGroupModel
                ?? CurrentSymbolArt!;

            var item = itemCreationPredicate(targetGroup);

            // if selected item is a layer, add the new layer before it
            // and if it is a group, add the layer as the first
            var index = selectedItem is SymbolArtLayerModel ? targetGroup.Children.IndexOf(selectedItem) : 0;
            var currentSelection = SelectedItems;

            Undo.Do($"Add {item.ItemTypeName}", () =>
                {
                    targetGroup.Children.Insert(index, item);
                    SelectedItem = item;
                },
                () =>
                {
                    targetGroup.Children.Remove(item);
                    SelectedItems = currentSelection;
                }
            );
        }

        public bool RequestExit()
        {
            if (!ConfirmCloseOpenFile())
                return false;

            Settings.Default.RecentFiles = RecentFiles.ToStringCollection();
            Settings.Default.ApplyToneCurve = ApplyToneCurve;
            Settings.Default.GuideLinesEnabled = GuideLinesEnabled;
            Settings.Default.ShowImageOverlays = ShowImageOverlays;
            Settings.Default.DisplayFlags = (int)DisplaySettingFlags;

            return true;
        }

        public void OpenFile_Implementation(string? filename)
        {
            if (!ConfirmCloseOpenFile())
                return;

            if (filename == null)
            {
                filename = _dialogService.BrowseOpenFile("Open existing symbol art file", OpenFormatFilter);

                if (filename == null)
                    return;
            }

            try
            {
                CurrentSymbolArt = new SymbolArtModel(Undo, filename);
                Undo.ResetWith("Open");
                SelectedItem = CurrentSymbolArt;
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Error opening file", "Unable to open the selected symbol art file.", ex);
            }

            AddRecentFile(filename);
        }

        public bool SaveCurrent()
        {
            if (CurrentSymbolArt == null)
                return false;

            if (string.IsNullOrEmpty(CurrentSymbolArt.FileName) || Path.GetExtension(CurrentSymbolArt.FileName).ToLowerInvariant() == ".sar")
            {
                return SaveCurrentAs();
            }
            else
            {
                try
                {
                    CurrentSymbolArt.Save();
                    return true;
                }
                catch (Exception ex)
                {
                    _dialogService.ShowErrorMessage("Error saving file", "Unable to save the symbol art file.", ex);
                }
            }

            return false;
        }

        public bool ExportCurrent()
        {
            if (CurrentSymbolArt == null)
                return false;

            int layerCount = CurrentSymbolArt.GetAllLayers().Where(x => x.IsVisible).Count();

            if (layerCount > 225)
            {
                if (!_dialogService.ShowConfirmation("Too many layers in symbol art", $"The current symbol art has more layers ({layerCount}) than PSO2 will accept (225). " +
                    $"You can still save the file as SAR, but you will not be able to import it to the game. Continue anyway?"))
                {
                    return false;
                }
            }

            string? filename = _dialogService.BrowseSaveFile("Export symbol art", ExportFormatFilter, Path.ChangeExtension(CurrentSymbolArt.FileName, ".sar"));

            if (filename == null)
                return false;

            try
            {
                CurrentSymbolArt.SaveAs(filename, Core.SymbolArtFileFormat.SAR, false);

                return true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Error saving file", "Unable to save the symbol art file.", ex);
            }

            return false;
        }

        public bool SaveCurrentAs()
        {
            if (CurrentSymbolArt == null)
                return false;

            string? filename = _dialogService.BrowseSaveFile("Save symbol art file", SaveFormatFilter, CurrentSymbolArt.FileName);

            if (filename == null)
                return false;

            try
            {
                CurrentSymbolArt.SaveAs(filename, Core.SymbolArtFileFormat.SAML, true);

                AddRecentFile(filename);

                OnPropertyChanged(nameof(AppTitle));

                return true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorMessage("Error saving file", "Unable to save the symbol art file.", ex);
            }

            return false;
        }

        public void OpenConvertedGroup(SymbolArtGroup group)
        {
            if (CurrentSymbolArt == null)
            {
                CurrentSymbolArt = new SymbolArtModel(Undo);
                SelectedItem = CurrentSymbolArt;
            }
            else
            {
                var response = _dialogService.ShowYesNoCancel("Confirm conversion", "Do you wish to add the converted bitmap to the current symbol art [Yes] or create a new one [No]?");
                if (response == null)
                {
                    return;
                }

                if (response == false)
                {
                    if (!ConfirmCloseOpenFile())
                        return;

                    CurrentSymbolArt = new SymbolArtModel(Undo);
                    SelectedItem = CurrentSymbolArt;
                }
            }

            CurrentSymbolArt.Children.Insert(0, new SymbolArtGroupModel(Undo, group, CurrentSymbolArt));
        }

        private void AddRecentFile(string filename)
        {
            int recentPos = RecentFiles.IndexOf(filename);
            if (recentPos != -1)
            {
                RecentFiles.RemoveAt(recentPos);
            }

            RecentFiles.Insert(0, filename);

            while (RecentFiles.Count > Settings.Default.MaxRecentFiles)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }
        }

        private void CleanupOutsideActiveArea()
        {
            if (CurrentSymbolArt == null)
                return;

            double boundryY = CurrentSymbolArt.Height / 2;
            double boundryX = CurrentSymbolArt.Width / 2;

            Point[] extent = new Point[]
            {
                new(-boundryX, -boundryY),
                new(-boundryX, boundryY),
                new(boundryX, boundryY),
                new(boundryX, -boundryY)
            };

            var targetLayers = CurrentSymbolArt
                .GetAllLayers()
                .Where(layer => !layer.Vertices.Any(vertex => extent.IsPointInside(vertex)) && !extent.IntersectsWith(layer.Vertices))
                .ToList();

            if (targetLayers.Count > 0)
            {
                using var scope = Undo.StartAggregateScope("Clean up symbols");

                targetLayers.ForEach(x => x.Delete());
            }
        }

        private void GroupLayersBasedOnColor()
        {
            if (CurrentSymbolArt == null)
                return;

            int groupCount = 1;

            using var scope = Undo.StartAggregateScope("Group color symbols");

            void AddColorGroup(System.Windows.Media.Color color, int index, SymbolArtItemModel parent, List<SymbolArtLayerModel> layers)
            {
                var colorGroup = new SymbolArtGroupModel(Undo, $"Color {groupCount++} - {ColorNameMapper.GetNearestName(color)}", true, parent);

                parent.Children.Insert(index, colorGroup);

                layers.ForEach(x => MoveItemTo(x, colorGroup, false));
            }

            void ProcessGroup(SymbolArtGroupModel group)
            {
                System.Windows.Media.Color? currentColor = null;
                List<SymbolArtLayerModel> layers = new();

                foreach (var item in group.Children.ToArray())
                {
                    if (item is SymbolArtLayerModel layer)
                    {
                        if (layer.Color == currentColor)
                        {
                            layers.Add(layer);
                        }
                        else
                        {
                            if (layers.Count > 1 && currentColor != null)
                            {
                                AddColorGroup(currentColor.Value, layer.IndexInParent, group, layers);
                            }

                            layers.Clear();
                            layers.Add(layer);
                            currentColor = layer.Color;
                        }
                    }
                    else if (item is SymbolArtGroupModel subGroup)
                    {
                        ProcessGroup(subGroup);
                    }
                }

                if (layers.Count > 1 && currentColor != null)
                {
                    AddColorGroup(currentColor.Value, group.Children.Count - 1, group, layers);
                }
            }

            ProcessGroup(CurrentSymbolArt);
        }

        private void SetDisplayFlag(DisplaySettingFlags flag, bool set, [CallerMemberName] string? propertyName = null)
        {
            if (set)
            {
                DisplaySettingFlags |= flag;
            }
            else
            {
                DisplaySettingFlags &= ~flag;
            }

            OnPropertyChanged(propertyName);
        }
    }
}
