using CommandLine;

namespace OpenSAE.CLI
{
    [Verb("info", false, HelpText = "Displays information on a symbol art")]
    public class InfoVerb
    {
        [Option('i', "input", HelpText = "Path to symbol art file", Required = true)]
        public string InputPath { get; set; } = null!;

        [Option("json", HelpText = "Output information as JSON")]
        public bool AsJson { get; set; }
    }
}
