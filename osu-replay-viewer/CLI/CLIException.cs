using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CLI
{
    public class CLIException : Exception
    {
        public string Cause { get; set; } = "Unknown";
        public string DisplayMessage { get; set; } = "idk";
        public string[] Suggestions { get; set; } = Array.Empty<string>();

        public CLIException() : base("CLI exception has occured. Look like it's from the user")
        {}
    }
}
