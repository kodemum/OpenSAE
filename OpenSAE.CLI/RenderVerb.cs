using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.CLI
{
    [Verb("render", true, HelpText = "Renders a specified symbol art to a bitmap file")]
    public class RenderVerb
    {
        [Option('i', "input", HelpText = "Path to symbol art file to convert", Required = true)]
        public string InputPath { get; set; } = null!;

        [Option('o', "output", HelpText = "Path to output bitmap file", Required = true)]
        public string OutputPath { get; set; } = null!;

        [Option("scale", Default = 100, HelpText = "Scale at which to render the symbol art, in percent")]
        public int RenderScale { get; set; }

        [Option("info-json", HelpText = "Save symbol art information to specified file as JSON")]
        public string? InfoJsonPath { get; set; }
    }
}
