using CommunityToolkit.Mvvm.ComponentModel;
using FramePFX.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Models
{
    public class ThemeModel
    {
        public ThemeModel(ThemeType type, string name)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public ThemeType Type { get; }

        public bool IsCurrent
        {
            get => ThemesController.CurrentTheme == Type;
            set => ThemesController.SetTheme(Type);
        }
    }
}
