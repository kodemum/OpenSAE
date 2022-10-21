using CommandLine;
using CommandLine.Text;

namespace OpenSAE.CLI
{
    internal class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<RenderVerb, InfoVerb>(args);

            return result.MapResult<RenderVerb, InfoVerb, int>(
                RenderCommand.Render,
                InfoCommand.ShowInfo,
                errs =>
                {
                    var helpText = HelpText.AutoBuild(result, h => HelpText.DefaultParsingErrorsHandler(result, h), e => e, verbsIndex: true);

                    ConsoleUtil.WriteConsoleHeader();
                    Console.WriteLine(helpText);

                    return -1;
                }
            );
        }
    }
}