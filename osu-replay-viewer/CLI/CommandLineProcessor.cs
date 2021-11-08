using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CLI
{
    public class CommandLineProcessor
    {
        public OptionDescription[] Options { get; set; } = Array.Empty<OptionDescription>();

        public string[] ProcessOptionsAndFilter(string[] args)
        {
            List<string> strList = new();
            ProgramArgumentsStream stream = new(args);
            while (stream.Available > 0)
            {
                string a = stream.Next();
                if (a.StartsWith("-"))
                {
                    bool found = false;
                    for (int i = 0; i < Options.Length; i++)
                    {
                        if (Options[i].IsOptionFlag(a))
                        {
                            found = true;
                            Options[i].Process(stream);
                            break;
                        }
                    }

                    if (!found) throw new CLIException
                    {
                        Cause = "Command-line Arguments (Options)",
                        DisplayMessage = $"Unknown option flag: {a}",
                        Suggestions = new[] { "Use -h or --help for information" }
                    };
                }
                else strList.Add(a);
            }
            return strList.ToArray();
        }

        public void PrintHelp(bool details, string query)
        {
            for (int i = 0; i < Options.Length; i++)
            {
                if (!Options[i].TryQuery(query)) continue;

                var longSwitch = "--" + Options[i].DoubleDashes[0];
                var parameters = "";
                for (int j = 0; j < Options[i].Parameters.Length; j++) parameters += $" <{Options[i].Parameters[j]}>";
                Console.WriteLine("  " + longSwitch.PadRight(24) + parameters);
                
                if (details)
                {
                    var alt = "";
                    for (int j = 1; j < Options[i].DoubleDashes.Length; j++) alt += (alt.Length == 0 ? "" : ", ") + "--" + Options[i].DoubleDashes[j];
                    for (int j = 0; j < Options[i].SingleDash.Length; j++) alt += (alt.Length == 0 ? "" : ", ") + "-" + Options[i].SingleDash[j];
                    if (alt.Length > 0) Console.WriteLine("    Alternatives: " + alt);
                    Console.WriteLine("    " + Options[i].Name);
                    Console.WriteLine("    " + Options[i].Description);
                    Console.WriteLine();
                }
            }
        }
    }
}
