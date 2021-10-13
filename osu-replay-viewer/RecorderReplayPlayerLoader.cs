using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;
using System.Linq;

namespace osu_replay_renderer_netcore
{
    class RecorderReplayPlayerLoader : PlayerLoader
    {
        private RecorderReplayPlayer player;

        public RecorderReplayPlayerLoader(RecorderReplayPlayer player) : base(() => player)
        {
            this.player = player;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            PlayerSettings.RemoveAll(v => true);

            (MetadataInfo.Children[0] as FillFlowContainer).RemoveRecursive(v => v is LoadingLayer);
            var mapMetadata = (MetadataInfo.Children[0] as FillFlowContainer).Children[5] as GridContainer;
            mapMetadata.Content = new[]
            {
                mapMetadata.Content[0].ToArray(),
                mapMetadata.Content[1].ToArray(),
                new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Margin = new MarginPadding { Right = 5 },
                        Colour = OsuColour.Gray(0.8f),
                        Text = "Played by"
                    },
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding { Left = 5 },
                        Text = player.GivenScore.ScoreInfo.UserString
                    }
                }
            };
        }
    }
}
