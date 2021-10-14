using AutoMapper.Internal;
using osu.Framework.Allocation;
using osu.Framework.Audio.Mixing;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Statistics;
using osu_replay_renderer_netcore.CustomHosts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace osu_replay_renderer_netcore
{
    class OsuGameRecorder : OsuGameBase
    {
        RecorderScreenStack ScreenStack;
        RecorderReplayPlayer Player;

        private string[] ProgramArguments;

        public OsuGameRecorder(string[] args)
        {
            ProgramArguments = args;
        }

        protected override void LoadComplete()
        {
            string subcommand = ProgramArguments[0].ToLower();
            Ruleset ruleset = new OsuRuleset().RulesetInfo.CreateInstance();

            if (subcommand.Equals("list"))
            {
                Console.WriteLine();
                Console.WriteLine("--------------------");
                Console.WriteLine("Listing all downloaded scores:");
                Console.WriteLine();
                foreach (ScoreInfo info in ScoreManager.QueryScores(info => true))
                {
                    // Filter osu! ruleset scores
                    if (info.Ruleset.ID != ruleset.RulesetInfo.ID) continue;

                    long scoreId = info.OnlineScoreID ?? -1;
                    string mods = "(no mod)";
                    if (info.Mods.Length > 0)
                    {
                        mods = "";
                        foreach (var mod in info.Mods) mods += (mods.Length > 0 ? ", " : "") + mod.Name;
                    }

                    Console.WriteLine("#" + info.ID + ": " + info.BeatmapInfo.GetDisplayTitle() + " (Ruleset: " + info.Ruleset.Name + ")");
                    Console.WriteLine("Played by " + info.UserString + " (Online Score ID: #" + (scoreId == -1? "n/a" : scoreId) + ")");
                    Console.WriteLine("Ranked Score: " + NumberFormatter.PrintWithSiSuffix(info.TotalScore) + " - Mods: " + mods);
                    Console.WriteLine();
                }
                Console.WriteLine("--------------------");
                Console.WriteLine();
                GracefullyExit();
            }
            else if (subcommand.Equals("download"))
            {
                Console.WriteLine("Downloading " + ProgramArguments[1] + "...");
                var model = new ScoreInfo()
                {
                    OnlineScoreID = long.Parse(ProgramArguments[1]),
                    Ruleset = ruleset.RulesetInfo
                };

                ScoreManager.Download(model, false);
                var obj = ScoreManager.GetExistingDownload(model);
                obj.DownloadProgressed += progress =>
                {
                    Console.WriteLine("Progress: " + (int)(progress * 100.0) + "%");
                    if (progress >= 1.0) Console.WriteLine("Importing...");
                };
                obj.Success += success =>
                {
                    Console.WriteLine("Replay downloaded: " + success);
                    GracefullyExit();
                };
                obj.Failure += fail =>
                {
                    Console.WriteLine("Replay download failed");
                    throw fail;
                };
            }
            else if (subcommand.Equals("view"))
            {
                string scoreId = ProgramArguments[1].ToLower();
                Score score;
                if (scoreId.StartsWith("online:"))
                {
                    long onlineId = long.Parse(scoreId.Substring(7));
                    score = ScoreManager.GetScore(ScoreManager.QueryScores(v => v.OnlineScoreID == onlineId).First());
                }
                else if (scoreId.StartsWith("file:"))
                {
                    string filePath = scoreId.Substring(5);
                    if (!File.Exists(filePath))
                    {
                        Console.Error.WriteLine("Score not found: " + filePath);
                        GracefullyExit();
                        return;
                    }

                    using (FileStream stream = new(filePath, FileMode.Open)) {
                        var decoder = new DatabasedLegacyScoreDecoder(RulesetStore, BeatmapManager);
                        try
                        {
                            score = decoder.Parse(stream);
                            score.ScoreInfo.BeatmapInfoID = BeatmapManager.QueryBeatmap(v => v.OnlineBeatmapID == score.ScoreInfo.BeatmapInfo.OnlineBeatmapID).ID;
                        }
                        catch (LegacyScoreDecoder.BeatmapNotFoundException e)
                        {
                            Console.Error.WriteLine("Beatmap not found while opening replay: " + e.Message);
                            score = null;
                        }
                    }
                }
                else
                {
                    int localId = int.Parse(scoreId);
                    score = ScoreManager.GetScore(ScoreManager.QueryScores(v => v.ID == localId).First());
                }

                if (score == null)
                {
                    Console.Error.WriteLine("Unable to open " + scoreId + ": Score not found in osu!lazer installation");
                    GracefullyExit();
                }

                LoadViewer(score, ruleset);
            }
            else
            {
                Console.Error.WriteLine(" !! ");
                Console.Error.WriteLine(" !! ");
                Console.Error.WriteLine(" !! Unknown subcommand: " + subcommand);
                Console.Error.WriteLine(" !! ");
                Console.Error.WriteLine(" !! ");
                GracefullyExit();
            }
        }

        [Resolved]
        private FrameworkConfigManager config { get; set; }

        private void LoadViewer(Score score, Ruleset ruleset)
        {
            // Apply some stuffs
            config.SetValue(FrameworkSetting.ConfineMouseMode, ConfineMouseMode.Never);
            if (!(Host is WindowsRecordGameHost)) config.SetValue(FrameworkSetting.FrameSync, FrameSync.VSync);
            Audio.Balance.Value = 0;
            Audio.TrackMixer.Balance.Value = 0;
            Audio.SampleMixer.Balance.Value = 0;

            ScreenStack = new RecorderScreenStack();
            LoadComponent(ScreenStack);
            Add(ScreenStack);

            var rulesetInfo = ruleset.RulesetInfo;
            Ruleset.Value = rulesetInfo;

            var working = BeatmapManager.GetWorkingBeatmap(BeatmapManager.QueryBeatmap(beatmap => score.ScoreInfo.BeatmapInfoID == beatmap.ID));
            Beatmap.Value = working;
            SelectedMods.Value = score.ScoreInfo.Mods;
            Console.WriteLine(score.ScoreInfo.BeatmapInfoID);

            Player = new RecorderReplayPlayer(score);

            RecorderReplayPlayerLoader loader = new RecorderReplayPlayerLoader(Player);
            ScreenStack.Push(loader);
            ScreenStack.ScreenPushed += ScreenStack_ScreenPushed;

            MenuCursorContainer.Cursor.RemoveAll(v => true);

            if (Host is HeadlessGameHost headless)
            {
                Console.WriteLine("Headless Host detected");

                // (Audio.TrackMixer as BassAudioMixer)
                // This is some sort of bad practice, but we want to capture audio output that's
                // generated from Bass
                // Hopefully we can replace audio mixer with our own in the future...

                /*var BassAudioMixer = typeof(AudioMixer).Assembly.GetType("osu.Framework.Audio.Mixing.Bass.BassAudioMixer");
                var getHandle = BassAudioMixer.GetDeclaredMethod("get_Handle");
                int trackHandle = (int)getHandle.Invoke(Audio.TrackMixer, null);
                int sampleHandle = (int)getHandle.Invoke(Audio.SampleMixer, null);*/
                if (headless is WindowsHeadlessGameHost wrv)
                {
                    /*wrv.TrackMixerHandle = trackHandle;
                    wrv.SampleMixerHandle = sampleHandle;*/
                    wrv.PrepareAudioDevices();
                }
            }
        }

        private void ScreenStack_ScreenPushed(IScreen lastScreen, IScreen newScreen)
        {
            Console.WriteLine("screen push: " + newScreen.GetType());
            ScreenStack.Parallax = 0.0f;

            if (newScreen is SoloResultsScreen soloResult)
            {
                soloResult.OnLoadComplete += (d) =>
                {
                    //MethodInfo internalChildMethod = typeof(CompositeDrawable).GetDeclaredMethod("get_InternalChild");
                    //GridContainer grid = internalChildMethod.Invoke(soloResult, null) as GridContainer;
                    GridContainer grid = DrawablesUtils.GetInternalChild(soloResult) as GridContainer;

                    var container = grid.Content[1][0] as Container;
                    container.RemoveAll(v => true);
                    container.Height = 0;

                    MethodInfo scrollContentMethod = typeof(ResultsScreen).GetDeclaredMethod("get_VerticalScrollContent");
                    OsuScrollContainer scrollContent = scrollContentMethod.Invoke(soloResult, null) as OsuScrollContainer;

                    var statisticsPanel = (scrollContent.Child as Container).Children[0] as StatisticsPanel;
                    MethodInfo internalChildStatsMethod = typeof(CompositeDrawable).GetDeclaredMethod("get_InternalChild");
                    var container2 = internalChildStatsMethod.Invoke(statisticsPanel, null) as Container;
                    container2.Remove(container2.Children[1]); // kill the loading spinner

                    Scheduler.AddDelayed(() =>
                    {
                        statisticsPanel.ToggleVisibility();
                    }, 2500);
                    
                    if (Host is WindowsRecordGameHost || Host is HeadlessGameHost)
                    {
                        Scheduler.AddDelayed(() =>
                        {
                            if (Host is WindowsRecordGameHost recordHost) recordHost.UsingEncoder = false;
                            if (Host is WindowsHeadlessGameHost headlessHost && headlessHost.OutputAudioToFile != null) headlessHost.UsingAudioRecorder = false;
                        }, 10000);
                        Scheduler.AddDelayed(() =>
                        {
                            if (Host is WindowsRecordGameHost recordHost)
                            {
                                recordHost.Encoder.FFmpeg.StandardInput.Close();
                                recordHost.Encoder = null;
                            }
                            GracefullyExit();
                        }, 11000);
                    }
                };
            }
            if (newScreen is RecorderReplayPlayer player && Host is WindowsRecordGameHost)
            {
                player.ManipulateClock = true;

                MethodInfo getGameplayClockContainer = typeof(Player).GetDeclaredMethod("get_GameplayClockContainer");
                var clockContainer = getGameplayClockContainer.Invoke(player, null) as GameplayClockContainer;
                //clockContainer.GameplayClock

                //MethodInfo setGameplayClock = typeof(GameplayClockContainer).GetDeclaredMethod("set_GameplayClock");
                var wrapped = new WrappedClock(Clock);
                (clockContainer.GameplayClock.Source as FramedOffsetClock).ChangeSource(wrapped);
            }
        }

        /// <summary>
        /// Dirty wrapped clock which allow me to manipulate gameplay clock
        /// </summary>
        public class WrappedClock : IFrameBasedClock
        {
            private IFrameBasedClock wrap;
            public double TimeOffset { get; set; } = 0;
            public IApplicableToRate RateMod { get; set; } = null;

            public WrappedClock(IFrameBasedClock wrap)
            {
                this.wrap = wrap;
            }

            public double ElapsedFrameTime => wrap.ElapsedFrameTime;
            public double FramesPerSecond => wrap.FramesPerSecond;
            public FrameTimeInfo TimeInfo => new FrameTimeInfo { Current = CurrentTime, Elapsed = ElapsedFrameTime };
            public double UnderlyingTime => wrap.CurrentTime + TimeOffset;
            public double CurrentTime {
                get
                {
                    if (RateMod == null) return UnderlyingTime;
                    return UnderlyingTime * RateMod.ApplyToRate(UnderlyingTime);
                }
            }
            public double Rate
            {
                get
                {
                    if (RateMod == null) return 1.0;
                    return RateMod.ApplyToRate(UnderlyingTime);
                }
            }
            public bool IsRunning => wrap.IsRunning;

            public void ProcessFrame() { wrap.ProcessFrame(); }
        }
    }
}
