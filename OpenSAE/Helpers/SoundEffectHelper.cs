using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace OpenSAE.Helpers
{
    internal static class SoundEffectHelper
    {
        private readonly static Lazy<MediaPlayer> _mediaPlayer = new(() => new MediaPlayer());

        private readonly static string _mediaDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenSAE", "Media");

        public static void PlaySoundEffectIfExists(SymbolArtSoundEffect sound)
        {
            string filename = Path.Combine(_mediaDirectory, $"{sound}.mp3");

            if (!File.Exists(filename))
            {
                try
                {
                    var mediaStream = Application.GetResourceStream(new Uri($"pack://application:,,,/assets/{sound}.mp3"));

                    using (mediaStream.Stream)
                    {
                        Directory.CreateDirectory(_mediaDirectory);

                        using var fs = File.OpenWrite(filename);

                        mediaStream.Stream.CopyTo(fs);
                    }
                }
                catch (IOException)
                {
                    return;
                }
            }

            _mediaPlayer.Value.Open(new Uri(filename));
            _mediaPlayer.Value.Play();
        }
    }
}
