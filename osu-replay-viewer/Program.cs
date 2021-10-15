using ManagedBass;
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
                Console.WriteLine("    replay view online:<Online Score ID>");
                Console.WriteLine("    replay view file:<Replay.osr>");
                Console.WriteLine("    replay view auto:<Online Beatmap ID>");
                Console.WriteLine("    Open Replay viewer");
                Console.WriteLine();
                Console.WriteLine("    replay list");
                Console.WriteLine("    View all downloaded scores. You might want to redirect outputs to file. Eg: program.dll list > scores.txt");
                Console.WriteLine();
                Console.WriteLine("    replay download <Online Score ID>");
                Console.WriteLine("    Download score from osu.ppy.sh");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("    --audio-devices");
                Console.WriteLine("    View all audio devices");
                Console.WriteLine();
                Console.WriteLine("    --headless");
                Console.WriteLine("    Use headless platform host (No images will be rendered)");
                Console.WriteLine();
                Console.WriteLine("    --headless-loopback <Input Device ID> <Output Device ID> <Output.wav>");
                Console.WriteLine("    Setup audio loopback for headless game host");
                Console.WriteLine();
                Console.WriteLine("    --record                               | -R");
                Console.WriteLine("    Use record platform host");
                Console.WriteLine();
                Console.WriteLine("    --record-resolution <Width> <Height>   | -RS <Width> <Height>");
                Console.WriteLine("    --record-fps <FPS>                     | -RF");
                Console.WriteLine("    --record-ffmpeg-preset <FFmpeg Preset> | -RP <FFmpeg Preset>");
                Console.WriteLine("    Set record parameters");
                Console.WriteLine();
                Console.WriteLine("    --record-frames-blending <Amount>      | -RB <Amount>");
                Console.WriteLine("    Set frames blending (1 or lower to disable)");
                Console.WriteLine();
                Console.WriteLine("    --record-minterpolate                  | -RMI");
                Console.WriteLine("    Enable Motion Interpolation (frames blending must be disabled)");
                Console.WriteLine();
                Console.WriteLine("    --record-output <Path/To/File.mp4>     | -O <...>");
                Console.WriteLine("    Set record output file path");
                Console.WriteLine();
                Console.WriteLine("    --mods-override <Mod1>[,<Mod2>,...]");
                Console.WriteLine("    Override mods list (Mod acronyms only)");
                Console.WriteLine();
                return;
            }

            string[] modsOverride = null;

            bool isHeadless = false;
            int headlessInput = -1, headlessOutput = -1;
            string headlessOutputFile = null;

            bool isRecord = false;
            int recordFPS = 60;
            int recordFramesBlending = 1;
            bool recordMinterpolate = false;
            string recordPreset = "veryslow";
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
                    case "--mods-override": modsOverride = args[i + 1].Split(","); i++; break;

                    case "--headless": isHeadless = true; break;
                    case "--headless-loopback":
                        headlessInput = int.Parse(args[i + 1]);
                        headlessOutput = int.Parse(args[i + 2]);
                        headlessOutputFile = args[i + 3];
                        i += 3;
                        break;
                    case "--audio-devices":
                        var devices = Bass.RecordingDeviceCount;
                        Console.WriteLine("BASS Input Devices (" + devices + "):");
                        for (int j = 0; j < devices; j++)
                        {
                            var device = Bass.RecordGetDeviceInfo(j);
                            Console.WriteLine(" #" + j + ": " + device.Name + (device.IsLoopback ? " (Loopback Device)" : ""));
                        }

                        devices = Bass.DeviceCount;
                        Console.WriteLine("BASS Output Devices (" + devices + "):");
                        for (int j = 0; j < devices; j++)
                        {
                            var device = Bass.GetDeviceInfo(j);
                            Console.WriteLine(" #" + j + ": " + device.Name);
                        }
                        return;

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

                    case "--record-output":
                    case "-O": recordOutput = args[i + 1]; i++; break;
                    default: break;
                }
            }

            Console.WriteLine("Launching... Please wait.");
            DesktopGameHost host;

            // In the future we'll use something that's cross-platform
            if (isHeadless) host = new WindowsHeadlessGameHost("osu", false, true)
            {
                OutputAudioToFile = headlessOutputFile,
                AudioInputDevice = headlessInput,
                AudioOutputDevice = headlessOutput
            };
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
                    Resolution = recordResolution,
                    OutputPath = recordOutput,
                    Preset = recordPreset,

                    // Smoothing options
                    FramesBlending = recordFramesBlending,
                    MotionInterpolation = recordMinterpolate,
                };
                host2.Encoder.StartFFmpeg();
                host = host2;
            }
            else host = Host.GetSuitableHost("osu", false);

            var game = new OsuGameRecorder(programArgs)
            {
                ModsOverride = modsOverride
            };
            host.Run(game);
            host.Dispose();
        }
    }
}
