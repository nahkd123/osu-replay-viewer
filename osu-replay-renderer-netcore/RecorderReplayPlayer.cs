using osu.Framework.Extensions;
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
        }
    }
}
