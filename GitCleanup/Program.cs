using System;

namespace GitCleanup
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var parser = CommandLine.Init();
            var result = parser.Parse(args);
            if (result.HasErrors)
            {
                Console.WriteLine($"Error: {result.ErrorText}");
                Console.WriteLine();
                CommandLine.PrintUsage(parser);
                return;
            }
            if (result.HelpCalled)
            {
                CommandLine.PrintUsage(parser);
                return;
            }
            CleanupProcessor.Run(parser.Object);
        }
    }
}