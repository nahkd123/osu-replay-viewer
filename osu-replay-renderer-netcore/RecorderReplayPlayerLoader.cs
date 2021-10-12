using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;

namespace osu_replay_renderer_netcore
{
    class RecorderReplayPlayerLoader : PlayerLoader
    {
        public RecorderReplayPlayerLoader(RecorderReplayPlayer player) : base(() => player)
        {}

        protected override void LoadComplete()
        {
            base.LoadComplete();
            PlayerSettings.RemoveAll(v => true);

            // kill the LoadingLayer
            var logoTrackingContainer = InternalChild as LogoTrackingContainer;
            var metadata = logoTrackingContainer[0] as BeatmapMetadataDisplay;
            (metadata.Children[0] as FillFlowContainer).RemoveRecursive(v => v is LoadingLayer);
        }
    }
}
