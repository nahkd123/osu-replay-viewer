using ManagedBass;
using NUnit.Framework;
using osu.Framework;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Game.Screens.Play;
using osu_replay_renderer_netcore.CLI;
using osu_replay_renderer_netcore.CustomHosts;
using osu_replay_renderer_netcore.Patching;
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
            // Command tree
            OptionDescription alwaysYes;
            OptionDescription modOverride;
            OptionDescription query;

            OptionDescription generalHelp;
            OptionDescription generalList;
            OptionDescription generalView;

            OptionDescription headlessMode;
            OptionDescription headlessLoopback;

            OptionDescription recordMode;
            OptionDescription recordOutput;
            OptionDescription recordResolution;
            OptionDescription recordFPS;
            OptionDescription recordAudioOutput;

            OptionDescription ffmpegPreset;
            OptionDescription ffmpegFramesBlending;
            OptionDescription ffmpegMotionInterpolation;
            OptionDescription ffmpegVideoEncoder;

            OptionDescription experimental;
            OptionDescription test;

            CommandLineProcessor cli = new()
            {
                Options = new[]
                {
                    // General
                    alwaysYes = new()
                    {
                        Name = "Always Yes",
                        Description = "Always answer yes to all prompts. Similar to 'command | yes'",
                        DoubleDashes = new[] { "yes" }
                    },
                    modOverride = new()
                    {
                        Name = "Mod Override",
                        Description = "Override Mod(s). You can use 'no-mod' or 'acronyms:NM' to clear all mods",
                        DoubleDashes = new[] { "mod-override" },
                        SingleDash = new[] { "MOD" }
                    },
                    query = new()
                    {
                        Name = "Query",
                        Description = "Query data (Eg: find something in help index or query replays)",
                        DoubleDashes = new[] { "query" },
                        SingleDash = new[] { "q" },
                        Parameters = new[] { "Keyword" }
                    },

                    generalList = new()
                    {
                        Name = "List Replays",
                        Description = "List all local replays",
                        DoubleDashes = new[] { "list" },
                        SingleDash = new[] { "list", "l" }
                    },
                    generalView = new()
                    {
                        Name = "View Replay",
                        Description = "Select a replay to view. This options must be always present (excluding -list options)",
                        DoubleDashes = new[] { "view" },
                        SingleDash = new[] { "view", "i" },
                        Parameters = new[] { "Type (local/online/file/auto)", "Score ID/Beatmap ID (auto)/File.osr" }
                    },
                    generalHelp = new()
                    {
                        Name = "Help Index",
                        Description = "View help with details",
                        DoubleDashes = new[] { "help" },
                        SingleDash = new[] { "h" }
                    },

                    // Headless options
                    headlessMode = new()
                    {
                        Name = "Headless Mode",
                        Description = "Switch to headless mode (not rendering anything to screen)",
                        DoubleDashes = new[] { "headless" },
                        SingleDash = new[] { "H" }
                    },
                    headlessLoopback = new()
                    {
                        Name = "Headless Audio Loopback",
                        Description = "Record audio produced by headless host through loopback device",
                        DoubleDashes = new[] { "headless-loopback" },
                        SingleDash = new[] { "HL" },
                        Parameters = new[] { "Input Device ID", "Output Device ID", "Output File (.wav)" }
                    },

                    // Record options
                    recordMode = new()
                    {
                        Name = "Record Mode",
                        Description = "Switch to record mode",
                        DoubleDashes = new[] { "record" },
                        SingleDash = new[] { "R" }
                    },
                    recordOutput = new()
                    {
                        Name = "Record Output",
                        Description = "Set record output",
                        DoubleDashes = new[] { "record-output" },
                        SingleDash = new[] { "O" },
                        Parameters = new[] { "Output = osu-replay.mp4" },
                        ProcessedParameters = new[] { "osu-replay.mp4" }
                    },
                    recordAudioOutput = new()
                    {
                        Name = "Record Audio Output",
                        Description = "Set record audio output (the file is always in RIFF Wave format)",
                        DoubleDashes = new[] { "record-audio", "record-audio-output" },
                        SingleDash = new[] { "AO" },
                        Parameters = new[] { "Output = <--record-output>.wav" }
                    },
                    recordResolution = new()
                    {
                        Name = "Record Resolution",
                        Description = "Set the output resolution",
                        DoubleDashes = new[] { "record-resolution" },
                        SingleDash = new[] { "RSL" },
                        Parameters = new[] { "Width = 1280", "Height = 600" },
                        ProcessedParameters = new[] { "1280", "600" }
                    },
                    recordFPS = new()
                    {
                        Name = "Record FPS",
                        Description = "Set the output FPS",
                        DoubleDashes = new[] { "record-fps" },
                        SingleDash = new[] { "FPS" },
                        Parameters = new[] { "FPS = 60" },
                        ProcessedParameters = new[] { "60" }
                    },

                    // FFmpeg options
                    ffmpegPreset = new()
                    {
                        Name = "FFmpeg H264 Encoding Preset",
                        Description = "Set the FFmpeg H264 Encoding preset",
                        DoubleDashes = new[] { "ffmpeg-preset" },
                        SingleDash = new[] { "FPR" },
                        Parameters = new[] { "Preset = slow" },
                        ProcessedParameters = new[] { "slow" }
                    },
                    ffmpegFramesBlending = new()
                    {
                        Name = "FFmpeg Frames Blending",
                        Description = "Blend multiple frames to create smooth transition. Default is 1x",
                        DoubleDashes = new[] { "ffmpeg-frames-blending" },
                        SingleDash = new[] { "FBL" },
                        Parameters = new[] { "Blending = 1" },
                        ProcessedParameters = new[] { "1" }
                    },
                    ffmpegMotionInterpolation = new()
                    {
                        Name = "FFmpeg Motion Interpolation",
                        Description = "Use motion interpolation to create smooth transition",
                        DoubleDashes = new[] { "ffmpeg-minterpolation" },
                        SingleDash = new[] { "FMI" }
                    },
                    ffmpegVideoEncoder = new()
                    {
                        Name = "FFmpeg Video Encoder",
                        Description = "Set video encoder for FFmpeg. 'ffmpeg -encoders' for the list",
                        DoubleDashes = new[] { "ffmpeg-encoder" },
                        SingleDash = new[] { "FENC" },
                        Parameters = new[] { "Encoder = libx264" },
                        ProcessedParameters = new[] { "libx264" }
                    },

                    // Misc
                    experimental = new()
                    {
                        Name = "Experimental Toggle",
                        Description = "Toggle experimental feature",
                        DoubleDashes = new[] { "experimental" },
                        SingleDash = new[] { "experimental" },
                        Parameters = new[] { "Flag" }
                    },
                    test = new()
                    {
                        Name = "Test Mode",
                        Description = "Test various stuffs (offline audio mixing for now)",
                        DoubleDashes = new[] { "test" },
                        Parameters = new[] { "Test Type (see SimpleTest.cs)" }
                    }
                }
            };

            // Apply patches
            // We only want to apply audio-related patches for record mode
            AudioPatcher.DoPatching();

            var game = new OsuGameRecorder();
            modOverride.OnOptions += (args) => { game.ModsOverride.Add(args[0]); };
            experimental.OnOptions += (args) => { game.ExperimentalFlags.Add(args[0]); };
            test.OnOptions += (args) => { SimpleTest.ExecuteTest(args[0]); };
            GameHost host;

            try
            {
                var progParams = cli.ProcessOptionsAndFilter(args);
                if (test.Triggered) return;
                if (args.Length == 0 || generalHelp.Triggered)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("  dotnet run osu-replay-renderer [options...]");
                    Console.WriteLine("  osu-replay-renderer [options...]");
                    Console.WriteLine();
                    cli.PrintHelp(generalHelp.Triggered, query.Triggered ? query[0] : null);
                    return;
                }

                if (generalList.Triggered)
                {
                    game.ListReplays = true;
                    game.ListQuery = query.Triggered ? query[0] : null;
                }
                else if (!generalView.Triggered) throw new CLIException
                {
                    Cause = "General Problem",
                    DisplayMessage = "--view must be present (except for --list)",
                    Suggestions = new[] { "Add --list <Type> <ID/Path> to your command" }
                };
                else
                {
                    string path = generalView[1];
                    bool isValidInteger = long.TryParse(generalView[1], out long id);
                    bool isValidInt32 = id < int.MaxValue;

                    if (!generalView[0].Equals("file") && !isValidInteger) throw new CLIException
                    {
                        Cause = "Command-line Arguments (Parsing)",
                        DisplayMessage = $"Value {generalView[1]} is not an integer"
                    };

                    switch (generalView[0])
                    {
                        case "local":
                        case "auto":
                            if (!isValidInt32) throw new CLIException
                            {
                                Cause = "Command-line Arguments (Parsing)",
                                DisplayMessage = $"{id} exceed int32 limit (larger than {int.MaxValue})",
                                Suggestions = new[] { "Keep the number lower than the limit" }
                            };
                            if (generalView[0].Equals("local")) game.ReplayOfflineScoreID = (int)id;
                            else game.ReplayAutoBeatmapID = (int)id;
                            break;

                        case "online": game.ReplayOnlineScoreID = id; break;

                        case "file":
                            if (!File.Exists(path)) throw new CLIException
                            {
                                Cause = "Files",
                                DisplayMessage = $"{path} doesn't exists"
                            };
                            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory)) throw new CLIException
                            {
                                Cause = "Files",
                                DisplayMessage = $"{path} is detected as directory"
                            };
                            game.ReplayFileLocation = path;
                            break;

                        default:
                            throw new CLIException
                            {
                                Cause = "Command-line Arguments (Options)",
                                DisplayMessage = $"Unknown type: {generalView[0]}",
                                Suggestions = new[] { "Available types: local/online/file/auto" }
                            };
                    }
                    game.ReplayViewType = generalView[0];
                }

                if (recordMode.Triggered)
                {
                    if (!CLIUtils.AskFileDelete(alwaysYes.Triggered, recordOutput[0])) return;
                    var audioOutput = recordAudioOutput.ProcessedParameters.Length > 0 ? recordAudioOutput[0] : (recordOutput[0] + ".wav");
                    if (!CLIUtils.AskFileDelete(alwaysYes.Triggered, audioOutput)) return;

                    int fps = ParseIntOrThrow(recordFPS[0]);
                    int blending = ParseIntOrThrow(ffmpegFramesBlending[0]);
                    var recordHost = new WindowsRecordGameHost("osu", fps * blending);
                    host = recordHost;

                    recordHost.Resolution = new System.Drawing.Size
                    {
                        Width = ParseIntOrThrow(recordResolution[0]),
                        Height = ParseIntOrThrow(recordResolution[1])
                    };
                    recordHost.Encoder = new CustomHosts.Record.ExternalFFmpegEncoder()
                    {
                        FPS = fps,
                        Resolution = recordHost.Resolution,
                        OutputPath = recordOutput[0],
                        Preset = ffmpegPreset[0],
                        Encoder = ffmpegVideoEncoder[0],

                        // Smoothing options
                        FramesBlending = blending,
                        MotionInterpolation = ffmpegMotionInterpolation.Triggered,
                    };
                    recordHost.AudioOutput = audioOutput;
                    
                    recordHost.Encoder.StartFFmpeg();
                }
                else if (headlessMode.Triggered)
                {
                    var headlessHost = new WindowsHeadlessGameHost("osu", false, true);
                    if (headlessLoopback.Triggered)
                    {
                        headlessHost.AudioInputDevice = ParseIntOrThrow(headlessLoopback[0]);
                        headlessHost.AudioOutputDevice = ParseIntOrThrow(headlessLoopback[1]);
                        headlessHost.OutputAudioToFile = headlessLoopback[2];
                    }
                    host = headlessHost;
                }
                else host = Host.GetSuitableHost("osu", false);
            } catch (CLIException cliException)
            {
                Console.WriteLine("Error while processing CLI arguments:");
                Console.WriteLine($"  Cause:      {cliException.Cause}");
                Console.WriteLine($"  Message:    {cliException.DisplayMessage}");
                if (cliException.Suggestions.Length == 0) return;
                else if (cliException.Suggestions.Length == 1) Console.WriteLine($"  Suggestion: {cliException.Suggestions[0]}");
                else
                {
                    Console.WriteLine("  Suggestions:");
                    for (int i = 0; i < cliException.Suggestions.Length; i++) Console.WriteLine("  - " + cliException.Suggestions[i]);
                }
                return;
            }

            if (recordMode.Triggered) game.DecodeAudio = true;
            host.Run(game);
            host.Dispose();
        }

        static int ParseIntOrThrow(string str)
        {
            if (!int.TryParse(str, out int val)) throw new CLIException
            {
                Cause = "Command-line Arguments (Parsing)",
                DisplayMessage = $"Invalid integer: {str}"
            };
            return val;
        }
    }
}
