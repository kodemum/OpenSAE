using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.CLI
{
    internal static class ConsoleUtil
    {
        public static void WriteNamedProperty(string propertyName, object? propertyValue, ConsoleColor color = ConsoleColor.White)
        {
            Console.Write("{0,-16}: ", propertyName);
            Console.ForegroundColor = color;
            Console.WriteLine(propertyValue?.ToString());
            Console.ResetColor();
        }

        public static void WriteConsoleHeader()
        {
            Console.WriteLine("OpenSAE command-line interface {0}", Assembly.GetEntryAssembly()?.GetName().Version);
            Console.WriteLine();
        }
    }
}
