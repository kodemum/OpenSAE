using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpenSAE.Core
{
    public class Symbol
    {
        public static bool DisablePreloading { get; set; }

        private static readonly string[] _symbolUris = new string[]
        {
            "symbols_r.png",
            "symbols_g.png",
            "symbols_b.png",
            "symbols_color.png"
        };

        private static BitmapImage[]? _sheetImages = null;
        private const int SymbolSizePixels = 64;
        private const int SymbolsPerSheetWidth = 16;

        private static BitmapImage[] GetSheetImages()
        {
            if (_sheetImages == null)
            {
                _sheetImages = new BitmapImage[_symbolUris.Length];

                for (int i = 0; i < _symbolUris.Length; i++)
                {
                    _sheetImages[i] = new BitmapImage();
                    _sheetImages[i].BeginInit();
                    _sheetImages[i].CacheOption = BitmapCacheOption.OnLoad;
                    _sheetImages[i].UriSource = new Uri($"pack://application:,,,/OpenSAE.Core;component/assets/{_symbolUris[i]}");
                    _sheetImages[i].EndInit();
                    _sheetImages[i].Freeze();
                }
            }

            return _sheetImages;
        }

        private BitmapSource? _image;

        public Symbol(int id, string name, SymbolGroup group, SymbolFlag flags = SymbolFlag.None)
            : this(id, name, name, group, flags)
        {
        }

        public Symbol(int id, string name, string description, SymbolGroup group, SymbolFlag flags = SymbolFlag.None)
            : this(id, name, 0, 0, description, group)
        {
            Flags = flags;
        }

        public Symbol(int id, string name, double kerning, string description, SymbolGroup group, SymbolFlag flags = SymbolFlag.None)
            : this(id, name, kerning / 2, kerning / 2, description, group, flags)
        {
        }

        public Symbol(int id, string name, double kerningRight, double kerningLeft, string description, SymbolGroup group, SymbolFlag flags = SymbolFlag.None)
        {
            Id = id;
            Name = name;
            Description = description;
            Group = group;
            KerningRight = kerningRight;
            KerningLeft = kerningLeft;

            if (!DisablePreloading)
            {
                _ = this.Image;
            }

            Flags = flags;
        }

        public int Id { get; }

        public double KerningLeft { get; }

        public double KerningRight { get; }

        public string Name { get; }

        public string Description { get; }

        public string Uri => $"pack://application:,,,/OpenSAE.Core;component/assets/{Id}.png";

        public BitmapSource? Image
        {
            get
            {
                _image ??= LoadCroppedSymbolImage();

                return _image;
            }
        }

        public SymbolGroup Group { get; }

        public SymbolFlag Flags { get; }

        public bool HasColor => Group == SymbolGroup.GamePortraits;

        /// <summary>
        /// Returns a cropped bitmap source that contains the symbol bitmap data.
        /// </summary>
        /// <returns></returns>
        private BitmapSource? LoadCroppedSymbolImage()
        {
            (int sheetIndex, int indexInSheet) = GetSymbolIndexInSheet();

            if (sheetIndex == -1)
                return null;

            int y = indexInSheet / SymbolsPerSheetWidth;
            int x = indexInSheet - (y * SymbolsPerSheetWidth);

            var imageSheet = GetSheetImages()[sheetIndex];

            CroppedBitmap image = new(
                imageSheet, 
                new System.Windows.Int32Rect(x * SymbolSizePixels, y * SymbolSizePixels, SymbolSizePixels, SymbolSizePixels));

            image.Freeze();

            return image;
        }

        /// <summary>
        /// Gets the index of the image sheet that contains the symbol and the index
        /// of the symbol within the sheet. Both 0-indexed.
        /// </summary>
        /// <returns></returns>
        private (int image, int index) GetSymbolIndexInSheet()
        {
            // the various checks are because the symbol ids for some reason
            // do not always match the order of the images in the image sheets

            int offset;

            // letters and punctuation are all in the first sheet
            if (Id <= 80)
            {
                return (0, Id - 1);
            }

            if (Id > 240 && Id <= 480)
            {
                offset = 241;

                if (Id >= 305)
                {
                    offset += 16;

                    if (Id > 400)
                    {
                        offset += 16;
                    }
                }

                return (1, Id - offset);
            }

            if (Id > 480 && Id <= 720)
            {
                offset = 481;

                if (Id >= 561)
                {
                    offset += 16;

                    if (Id >= 641)
                    {
                        offset += 16;
                    }
                }

                return (2, Id - offset);
            }

            // full-color symbols (game portraits mostly) are in the last sheet
            if (Id > 720 && Id <= 768)
            {
                return (3, Id - 721);
            }

            return (-1, -1);
        }
    }
}
