using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;
using osuTK.Graphics;
using System;

namespace osu_replay_renderer_netcore.Components
{
    public class PlaybackControl : Container
    {
        private GameplayClockContainer ClockContainer;
        private IconButton playPause;

        public PlaybackControl(GameplayClockContainer gameplayClockContainer)
        {
            ClockContainer = gameplayClockContainer;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(new Drawable[] {
                new TimeInfo(ClockContainer.GameplayClock)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both
                },
                playPause = new IconButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Width = 30,
                    Height = 30,
                    Icon = FontAwesome.Solid.Play,
                    Colour = OsuColour.Gray(1.0f),
                    Enabled = { Value = true },
                    Action = () =>
                    {
                        if (ClockContainer.IsPaused.Value) ClockContainer.Start();
                        else ClockContainer.Stop();
                    },
                    TooltipText = "Resume/Pause (Space)"
                },
            });
        }

        protected override void Update()
        {
            base.Update();
            playPause.Icon = ClockContainer.IsPaused.Value ? FontAwesome.Solid.Play : FontAwesome.Solid.Pause;
        }

    }
}
