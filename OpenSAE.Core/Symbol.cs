﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OpenSAE.Core
{
    public class Symbol
    {
        public static bool DisablePreloading { get; set; }

        private BitmapImage _image;

        public Symbol(int id, string name, SymbolGroup group)
            : this(id, name, name, group)
        {
        }

        public Symbol(int id, string name, string description, SymbolGroup group)
            : this(id, name, 0, 0, description, group)
        {
        }

        public Symbol(int id, string name, double kerning, string description, SymbolGroup group)
            : this(id, name, kerning / 2, kerning / 2, description, group)
        {
        }

        public Symbol(int id, string name, double kerningRight, double kerningLeft, string description, SymbolGroup group)
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
        }

        public int Id { get; }

        public double KerningLeft { get; }

        public double KerningRight { get; }

        public string Name { get; }

        public string Description { get; }

        public string Uri => $"pack://application:,,,/OpenSAE.Core;component/assets/{Id}.png";

        public BitmapImage Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage();
                    _image.BeginInit();
                    _image.CacheOption = BitmapCacheOption.OnLoad;
                    _image.UriSource = new Uri(Uri);
                    _image.EndInit();
                    _image.Freeze();
                }

                return _image;
            }
        }

        public SymbolGroup Group { get; }

        public Vector[]? Vectors { get; set; }
    }
}
