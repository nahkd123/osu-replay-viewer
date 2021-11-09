using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.HUD.Builtin
{
    // Should we split graph to seperate abstract class?

    /// <summary>
    /// Performance points graph. This HUD is the PP graph, which shows historical PP
    /// data (it might store some future data if you use seeking feature)
    /// </summary>
    public class PerformanceGraph : BoxOverlay
    {
        private const int NUM_OF_VERTICES = 1000;
        private const double MS_PER_SEGMENT = 1;
        public Vector2[] Vertices;
        public Path Path2D;

        public Bindable<double> PP = new BindableDouble(0);

        public PerformanceGraph() : base("Performance Graph")
        {
            var bgColour = new Color4(0f, 0f, 0f, 0.6f);
            Vertices = new Vector2[NUM_OF_VERTICES];

            Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 100,
                        Colour = bgColour
                    },
                    Path2D = new Path
                    {
                        PathRadius = 1.2f,
                        Colour = OsuColour.Gray(1.0f),
                        Vertices = Vertices
                    }
                }
            });

            double prevTime = 0.0;
            double prevTime2 = 0.0;

            OnUpdate += (drawable) =>
            {
                double currentTime = prevTime + Time.Elapsed;
                double timeDelta2 = currentTime - prevTime2;
                int segmentsUpdate = (int)Math.Floor(timeDelta2 / MS_PER_SEGMENT);
                if (segmentsUpdate == 0)
                {
                    prevTime = currentTime;
                    return;
                }
                
                while (segmentsUpdate > 0)
                {
                    segmentsUpdate--;

                    // Shift
                    for (int i = 0; i < NUM_OF_VERTICES; i++)
                    {
                        Vertices[i].X = i * DrawWidth / NUM_OF_VERTICES;
                        if (i > 0) Vertices[i - 1].Y = Vertices[i].Y;
                    }
                    Vertices[NUM_OF_VERTICES - 1].Y = (float)PP.Value;
                    Path2D.Vertices = Vertices;
                }
                prevTime2 = currentTime;
                prevTime = currentTime;
            };
        }
    }
}
