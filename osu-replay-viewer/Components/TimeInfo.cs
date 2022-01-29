using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu_replay_renderer_netcore.Components
{
    public class TimeInfo : FillFlowContainer
    {
        private OsuSpriteText trackTimerMinor;
        private OsuSpriteText trackTimerMajor;
        private IClock clock;

        public TimeInfo(IClock clock = null)
        {
            this.clock = clock ?? Clock;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Direction = FillDirection.Horizontal;
            Children = new Drawable[]
            {
                trackTimerMinor = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 25, fixedWidth: true),
                    Text = "00:00",
                    Colour = OsuColour.Gray(1.0f)
                },
                trackTimerMajor = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Font = OsuFont.GetFont(size: 16, fixedWidth: true),
                    X = 50,
                    Text = ".000",
                    Colour = OsuColour.Gray(0.5f)
                }
            };
        }

        protected override void Update()
        {
            base.Update();
            TimeSpan ts = TimeSpan.FromMilliseconds(clock.CurrentTime);
            trackTimerMinor.Text = $"{(ts < TimeSpan.Zero ? " - " : string.Empty)}{(int)ts.TotalMinutes:00}:{ts:ss}";
            trackTimerMajor.Text = $".{ts:fff}";
        }
    }
}
