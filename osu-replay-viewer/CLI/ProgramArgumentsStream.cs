using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CLI
{
    public class ProgramArgumentsStream
    {
        public string[] Arguments { get; private set; }
        public int Pointer { get; set; } = 0;
        public int Available => Arguments.Length - Pointer;

        public ProgramArgumentsStream(string[] args)
        { Arguments = args; }

        public string Next()
        {
            if (Pointer < Arguments.Length) return Arguments[Pointer++];
            return null;
        }
    }
}
