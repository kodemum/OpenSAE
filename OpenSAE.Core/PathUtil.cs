using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenSAE.Core
{
    public static class PathUtil
    {
        private static Lazy<Regex> InvalidFilenameRegex { get; }

        static PathUtil()
        {
            InvalidFilenameRegex = new Lazy<Regex>(() => new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]"));
        }

        public static string SanitizeFilename(string input)
        {
            return InvalidFilenameRegex.Value.Replace(input.Trim(), "");
        }
    }
}
