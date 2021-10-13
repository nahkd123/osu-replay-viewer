﻿using NUnit.Framework;
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
                Console.WriteLine("    replay view file:<Replay.osr>");
                Console.WriteLine("    Open Replay viewer with score from file");
                Console.WriteLine();
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
                Console.WriteLine("    --record                               | -R");
                Console.WriteLine("    Use record platform host");
                Console.WriteLine();
                Console.WriteLine("    --record-resolution <Width> <Height>   | -RS <Width> <Height>");
                Console.WriteLine("    Set record resolution (Only for record platform host)");
                Console.WriteLine();
                Console.WriteLine("    --record-fps <FPS>                     | -RF");
                Console.WriteLine("    Set record FPS (Only for record platform host)");
                Console.WriteLine();
                Console.WriteLine("    --record-frames-blending <Amount>      | -RB <Amount>");
                Console.WriteLine("    Set frames blending (1 or lower to disable)");
                Console.WriteLine();
                Console.WriteLine("    --record-minterpolate                  | -RMI");
                Console.WriteLine("    Enable Motion Interpolation (frames blending must be disabled)");
                Console.WriteLine();
                Console.WriteLine("    --record-ffmpeg-preset <FFmpeg Preset> | -RP <FFmpeg Preset>");
                Console.WriteLine("    Set FFmpeg encoding preset");
                Console.WriteLine();
                Console.WriteLine("    --record-middleman <png/mjpeg>         | -RMM <png/mjpeg>");
                Console.WriteLine("    Set image middleman. PNG gives better quality, but slow to encode");
                Console.WriteLine();
                Console.WriteLine("    --record-output <Path/To/File.mp4>     | -O <...>");
                Console.WriteLine("    Set record output file path");
                Console.WriteLine();
                return;
            }

            bool isHeadless = false;

            bool isRecord = false;
            int recordFPS = 60;
            int recordFramesBlending = 1;
            bool recordMinterpolate = false;
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

                    case "--record":
                    case "-R": isRecord = true; break;

                    case "--record-resolution":
                    case "-RS":
                        recordResolution = new System.Drawing.Size(int.Parse(args[i + 1]), int.Parse(args[i + 2]));
                        i += 2;
                        break;

                    case "--record-fps":
                    case "-RF": recordFPS = int.Parse(args[i + 1]); i++; break;

                    case "--record-frames-blending":
                    case "-RB": recordFramesBlending = int.Parse(args[i + 1]); i++; break;

                    case "--record-minterpolate":
                    case "-RMI": recordMinterpolate = true; break;

                    case "--record-ffmpeg-preset":
                    case "-RP": recordPreset = args[i + 1]; i++; break;

                    case "--record-middleman":
                    case "-RMM": recordMiddlemanTarget = args[i + 1]; i++; break;

                    case "--record-output":
                    case "-O": recordOutput = args[i + 1]; i++; break;
                    default: break;
                }
            }

            Console.WriteLine("Launching... Please wait.");
            DesktopGameHost host;

            // In the future we'll use something that's cross-platform
            if (isHeadless) host = new WindowsHeadlessGameHost("osu", false, false);
            else if (isRecord)
            {
                var host2 = new WindowsRecordGameHost("osu", recordFPS * Math.Max(recordFramesBlending, 1));
                host2.Resolution = recordResolution;
                if (File.Exists(recordOutput))
                {
                    Console.Error.WriteLine("Unable to start record platform host: File already exists: " + recordOutput);
                    Console.Error.WriteLine("Either delete or rename it");
                    return;
                }
                host2.Encoder = new CustomHosts.Record.ExternalFFmpegEncoder()
                {
                    FPS = recordFPS,
                    ImageFormat = recordMiddlemanTarget,
                    OutputPath = recordOutput,
                    Preset = recordPreset,

                    // Smoothing options
                    FramesBlending = recordFramesBlending,
                    MotionInterpolation = recordMinterpolate,
                };
                host2.Encoder.StartFFmpeg();
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