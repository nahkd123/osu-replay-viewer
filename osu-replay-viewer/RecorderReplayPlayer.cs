using osu.Framework.Extensions;
using osu.Framework.Timing;
using osu.Game;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore
{
    class RecorderReplayPlayer : ReplayPlayer
    {
        public Score GivenScore { get; private set; }
        public bool ManipulateClock { get; set; } = false;

        public RecorderReplayPlayer(Score score) : base(score)
        {
            GivenScore = score;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            HUDOverlay.ShowHud.Value = false;
            HUDOverlay.HoldToQuit.Hide();
            //DrawablesUtils.RemoveRecursive(HUDOverlay, v => v == HUDOverlay.PlayerSettingsOverlay);
            HUDOverlay.RemoveRecursive(v => v == HUDOverlay.PlayerSettingsOverlay);
            GameplayClockContainer.RemoveRecursive(v => v is SkipOverlay);
        }

        protected override void StartGameplay()
        {
            if (ManipulateClock)
            {
                GameplayClockContainer.Reset();
                GameplayClockContainer.Start();
                var clock = (GameplayClockContainer.GameplayClock.Source as FramedOffsetClock).Source as OsuGameRecorder.WrappedClock;
                clock.TimeOffset = -clock.CurrentTime - 2000;
            } else base.StartGameplay();
        }
    }
}
