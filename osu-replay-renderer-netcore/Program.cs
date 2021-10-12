using NUnit.Framework;
using osu.Framework;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Game.Screens.Play;
using osu_replay_renderer_netcore.CustomHosts;
using System;
using System.Collections.Generic;

namespace osu_replay_renderer_netcore
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("osu! replay viewer");
                Console.WriteLine("Usage: replay [Options...] <Subcommand> <...Subcommand Arguments>");
                Console.WriteLine();
                Console.WriteLine("    replay view <Local Score ID>");
                Console.WriteLine("    Open Replay viewer with Local Score ID");
                Console.WriteLine();
                Console.WriteLine("    replay view online:<Online Score ID>");
                Console.WriteLine("    Open Replay viewer with Online Score ID");
                Console.WriteLine();
                Console.WriteLine("    replay list");
                Console.WriteLine("    View all downloaded scores. You might want to redirect outputs to file. Eg: program.dll list > scores.txt");
                Console.WriteLine();
                Console.WriteLine("    replay download <Online Score ID>");
                Console.WriteLine("    Download score from osu.ppy.sh");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("    --headless");
                Console.WriteLine("    Use headless platform host");
                //Console.WriteLine();
                //Console.WriteLine("    --record");
                //Console.WriteLine("    Use record platform host");
                return;
            }

            bool isHeadless = false;
            //bool isRecord = false;
            string[] programArgs = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("-"))
                {
                    programArgs = new List<string>(args).GetRange(i, args.Length - i).ToArray();
                    break;
                }
                switch (args[i])
                {
                    case "--headless": isHeadless = true; break;
                    case "--record": break;
                }
            }

            Console.WriteLine("Launching. Please wait...");
            DesktopGameHost host;

            // In the future we'll use something that's cross-platform
            if (isHeadless) host = new WindowsHeadlessGameHost("osu", false, false);
            else host = Host.GetSuitableHost("osu", false);

            var game = new OsuGameRecorder(programArgs);
            var stopwatch = new StopwatchClock(true);
            var framed = new FramedClock(stopwatch);
            stopwatch.Rate = 1.00;
            game.Clock = framed;
            host.Run(game);
            host.Dispose();
        }
    }
}
