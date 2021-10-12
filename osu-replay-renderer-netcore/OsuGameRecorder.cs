﻿using AutoMapper.Internal;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Statistics;
using System;
using System.Linq;
using System.Reflection;

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
                    long scoreId = info.OnlineScoreID ?? -1;
                    Console.WriteLine("#" + info.ID + ": " + info.BeatmapInfo.GetDisplayTitle());
                    Console.WriteLine("Played by " + info.UserString + " (Online Score ID: #" + (scoreId == -1? "n/a" : scoreId) + ")");
                    Console.WriteLine("Ranked Score: " + NumberFormatter.PrintWithSiSuffix(info.TotalScore));
                    Console.WriteLine();
                }
                Console.WriteLine("--------------------");
                Console.WriteLine();
                GracefullyExit();
            }
            if (subcommand.Equals("download"))
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
            if (subcommand.Equals("view"))
            {
                string scoreId = ProgramArguments[1].ToLower();
                Score score;
                if (scoreId.StartsWith("online:"))
                {
                    long onlineId = long.Parse(scoreId.Substring(7));
                    score = ScoreManager.GetScore(ScoreManager.QueryScores(v => v.OnlineScoreID == onlineId).First());
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
        }

        [Resolved]
        private FrameworkConfigManager config { get; set; }

        private void LoadViewer(Score score, Ruleset ruleset)
        {
            // Apply some configurations
            config.SetValue(FrameworkSetting.ConfineMouseMode, ConfineMouseMode.Never);

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
                    // If ppy changed the StatisticsPanel.cs again, please notify me.
                    Scheduler.AddDelayed(() =>
                    {
                        statisticsPanel.ToggleVisibility();
                    }, 2500);
                };
            }
        }
    }
}
