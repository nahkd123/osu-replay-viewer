using osu_replay_renderer_netcore.HUD.Builtin;
using System;

namespace osu_replay_renderer_netcore.HUD
{
    public static class GraphDisplay
    {
        public delegate void GraphWindowProcessor(
            PerformanceGraph graph,
            double pp,
            out double min,
            out double max
        );

        public static void Full(PerformanceGraph graph, double pp, out double min, out double max)
        {
            min = graph.Minimum;
            max = graph.Maximum;
        }

        public static void Windowed(PerformanceGraph graph, double pp, out double min, out double max)
        {
            min = pp - graph.WindowScale / 2.0;
            max = pp + graph.WindowScale / 2.0;
        }
    }
}
