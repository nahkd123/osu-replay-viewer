using NUnit.Framework;
using osu.Framework;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Game.Screens.Play;
using osu_replay_renderer_netcore.CustomHosts;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;

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
                //Console.WriteLine("    replay view file:<Replay.osr>");
                //Console.WriteLine("    Open Replay viewer with score from file");
                //Console.WriteLine();
                Console.WriteLine("    replay list");
                Console.WriteLine("    View all downloaded scores. You might want to redirect outputs to file. Eg: program.dll list > scores.txt");
                Console.WriteLine();
                Console.WriteLine("    replay download <Online Score ID>");
                Console.WriteLine("    Download score from osu.ppy.sh");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("    --headless");
                Console.WriteLine("    Use headless platform host (No images will be rendered)");
                Console.WriteLine();
                Console.WriteLine("    --record");
                Console.WriteLine("    Use record platform host");
                Console.WriteLine();
                Console.WriteLine("    --record-resolution <Width> <Height>");
                Console.WriteLine("    Set record resolution (Only for record platform host)");
                Console.WriteLine();
                Console.WriteLine("    --record-fps <FPS>");
                Console.WriteLine("    Set record FPS (Only for record platform host)");
                Console.WriteLine();
                Console.WriteLine("    --record-ffmpeg-preset <FFmpeg Preset>");
                Console.WriteLine("    Set FFmpeg encoding preset");
                Console.WriteLine();
                Console.WriteLine("    --record-middleman <png/mjpeg>");
                Console.WriteLine("    Set image middleman. PNG gives better quality, but slow to encode");
                Console.WriteLine();
                Console.WriteLine("    --record-output <Path/To/File.mp4>");
                Console.WriteLine("    Set record output file path");
                Console.WriteLine();
                return;
            }

            bool isHeadless = false;

            bool isRecord = false;
            int recordFPS = 60;
            string recordPreset = "veryslow";
            string recordMiddlemanTarget = "png";
            string recordOutput = "osu-replay.mp4";
            System.Drawing.Size recordResolution = new System.Drawing.Size { Width = 1280, Height = 600 };

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
                    case "--record": isRecord = true; break;
                    case "--record-resolution":
                        recordResolution = new System.Drawing.Size(int.Parse(args[i + 1]), int.Parse(args[i + 2]));
                        i += 2;
                        break;
                    case "--record-fps": recordFPS = int.Parse(args[i + 1]); i++; break;
                    case "--record-ffmpeg-preset": recordPreset = args[i + 1]; i++; break;
                    case "--record-middleman": recordMiddlemanTarget = args[i + 1]; i++; break;
                    case "--record-output": recordOutput = args[i + 1]; i++; break;
                    default: break;
                }
            }

            Console.WriteLine("Launching... Please wait.");
            DesktopGameHost host;

            // In the future we'll use something that's cross-platform
            if (isHeadless) host = new WindowsHeadlessGameHost("osu", false, false);
            else if (isRecord)
            {
                var host2 = new WindowsRecordGameHost("osu", recordFPS);
                host2.Resolution = recordResolution;
                if (File.Exists(recordOutput))
                {
                    Console.Error.WriteLine("Unable to start record platform host: File already exists: " + recordOutput);
                    Console.Error.WriteLine("Either delete or rename it");
                    return;
                }
                host2.Encoder = new CustomHosts.Record.ExternalFFmpegEncoder(
                    recordFPS,
                    recordMiddlemanTarget,
                    recordOutput,
                    recordPreset
                );
                host2.RecordMiddleman = recordMiddlemanTarget;
                host = host2;
            }
            else host = Host.GetSuitableHost("osu", false);

            var game = new OsuGameRecorder(programArgs);
            host.Run(game);
            host.Dispose();
        }
    }
}
