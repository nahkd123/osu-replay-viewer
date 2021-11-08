using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CLI
{
    public class OptionDescription
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string[] DoubleDashes { get; set; } = Array.Empty<string>();
        public string[] SingleDash { get; set; } = Array.Empty<string>();
        public string[] Parameters { get; set; } = Array.Empty<string>();

        public bool Triggered = false;
        public string[] ProcessedParameters { get; set; } = Array.Empty<string>();

        public string this[int i]
        {
            get { return ProcessedParameters[i]; }
            set { ProcessedParameters[i] = value; }
        }

        public bool IsOptionFlag(string flag)
        {
            for (int i = 0; i < DoubleDashes.Length; i++) if (flag.Equals("--" + DoubleDashes[i])) return true;
            for (int i = 0; i < SingleDash.Length; i++) if (flag.Equals("-" + SingleDash[i])) return true;
            return false;
        }

        public bool TryQuery(string keyword)
        {
            if (keyword == null) return true;
            keyword = keyword.ToLower();
            if (Name.ToLower().Contains(keyword) || Description.ToLower().Contains(keyword)) return true;
            for (int i = 0; i < DoubleDashes.Length; i++) if (DoubleDashes[i].ToLower().Contains(keyword)) return true;
            for (int i = 0; i < SingleDash.Length; i++) if (SingleDash[i].ToLower().Contains(keyword)) return true;
            return false;
        }

        public event Action<string[]> OnOptions;

        public void Process(ProgramArgumentsStream stream)
        {
            if (stream.Available < Parameters.Length) throw new CLIException
            {
                Cause = "Command-line Arguments (Options)",
                DisplayMessage = $"--{Name} requires {Parameters.Length} parameters, but only {stream.Available} found",
                Suggestions = new[]
                {
                    $"Append {Parameters.Length - stream.Available} more parameters"
                }
            };
            ProcessedParameters = new string[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++) ProcessedParameters[i] = stream.Next();
            Triggered = true;

            OnOptions?.Invoke(ProcessedParameters);
        }
    }
}
