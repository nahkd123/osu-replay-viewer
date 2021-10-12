using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CustomHosts
{
    public class WindowsHeadlessGameHost : HeadlessGameHost
    {
        public override IEnumerable<string> UserStoragePaths => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Yield();

        public WindowsHeadlessGameHost(
            string gameName = null,
            bool bindIPC = false,
            bool realtime = true,
            bool portableInstallation = false
        ) : base(gameName, bindIPC, realtime, portableInstallation)
        {}

        int frames = 0;

        protected override void DrawFrame()
        {
            if (Root == null) return;
            var container = Root.Child as PlatformActionContainer;

            // Here we'll do something to the container
            // Let's just print it out as a tree!
            frames++;
            if ((frames) % 240 == 0)
            {
                Console.Clear();
                Console.WriteLine("Screen Report:");
                PrintAsTree(container, 0);
                Console.WriteLine();
            }
        }

        private void PrintAsTree(Drawable drawable, int depth)
        {
            string spaces = "".PadLeft(depth * 2, ' ');
            Console.WriteLine(spaces + drawable.GetType().Name + " (" + drawable.X + ", " + drawable.Y + ")");
            if (depth > 6) return;

            if (drawable is Container container1)
            {
                foreach (var child in container1.Children) PrintAsTree(child, depth + 1);
            }
            else if (drawable is Container<Drawable> container2)
            {
                foreach (var child in container2.Children) PrintAsTree(child, depth + 1);
            }
            else if (drawable is CompositeDrawable composite)
            {
                //Console.WriteLine(spaces + "(Composite Drawable)");
                foreach (var child in DrawablesUtils.GetInternalChildren(composite)) PrintAsTree(child, depth + 1);
            }
        }
    }
}
