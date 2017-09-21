using System;
using System.Linq;
using Fclp;

namespace GitCleanup
{
    internal static class CommandLine
    {
        public static IFluentCommandLineParser<CleanupSettings> Init()
        {
            var parser = new FluentCommandLineParser<CleanupSettings> { IsCaseSensitive = false };

            parser.SetupHelp("h");

            parser.Setup(x => x.Path)
                  .As('d', "Directory")
                  .Required()
                  .WithDescription("Path to the local repository.");

            parser.Setup(x => x.BatchSize)
                  .As('b', "Batch")
                  .SetDefault(10)
                  .WithDescription("Delete remotes in batches. It s faster but fails sometime.");

            parser.Setup(x => x.MaxDaysSinceMerged)
                  .As('m', "Merged")
                  .SetDefault(7)
                  .WithDescription("Maximum days since merged. 7 days by default");

            parser.Setup(x => x.MaxDaysSinceOrphan)
                  .As('o', "Orphan")
                  .SetDefault(30 * 6)
                  .WithDescription("Maximum days since orphaned (has incomplete development). 180 days by default.");

            parser.Setup(x => x.AuthType)
                  .As('a', "AuthType")
                  .SetDefault(AuthType.Basic)
                  .WithDescription("Authentication type (Basic|Ntlm). Basic by default.");

            parser.Setup(x => x.UserName)
                  .As('u', "UserName")
                  .SetDefault(null)
                  .WithDescription("User name.");

            parser.Setup(x => x.Password)
                  .As('p', "Password")
                  .SetDefault(null)
                  .WithDescription("Password.");

            parser.Setup(x => x.RunMode)
                  .As('r', "RunMode")
                  .SetDefault(RunMode.List)
                  .WithDescription("(List|Local|Full) run mode. Default is List.");

            parser.Setup(x => x.OutFileName)
                  .As('f', "OutputFile")
                  .SetDefault(null)
                  .WithDescription("Output orphans to a file.");

            parser.Setup(x => x.ExclusionMasks)
                  .As('e', "Mask")
                  .SetDefault(null)
                  .WithDescription("Set of regular expressions to mark branches for exclusion from processing.");

            return parser;
        }

        public static void PrintUsage(IFluentCommandLineParser<CleanupSettings> parser)
        {
            Console.WriteLine("GitCleanup usage: -d <directory> -u <user> -p <password>");
            Console.WriteLine("Extra options:");
            foreach (var o in parser.Options)
            {
                Console.WriteLine($"{"",-3}-{o.ShortName}, --{o.LongName,-10} : {o.Description}");
            }   
        }
    }
}
